|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.6 — Dígitos Verificadores (solo cálculo y mantenimiento) — Plataforma Operativ

Primera etapa del requerimiento 10.8.3 de la Carpeta de Tecnología (Integridad de Datos con DVH/DVV). **Alcance acotado por decisión: este parche solo llena y mantiene los dígitos** — DVH en cada fila de las tablas críticas (existentes y cada vez que se modifican) y DVV por tabla en la nueva tabla `DigitosVerticales`. **Toda la parte de chequeo queda fuera de scope** (verificación en login, ERR04, pantalla "Reparar Base de Datos", CU-001-002): será un parche posterior que se apoya en los datos que este deja calculados y al día.

Escrito contra el estado real de `main` en `TFI.Operativ.WebApp` (revisado el 17/08/2026).

**Decisiones ya tomadas (refinadas en conversación):**
- Algoritmo: **sumatoria ponderada literal**, como define la carpeta (no hash).
- Alcance de tablas: **las 8 actuales** — `Usuario`, `Bitacora` y las de permisos (`Familia`, `Patente`, `UsuarioFamilia`, `UsuarioPatente`, `FamiliaPatente`, `FamiliaFamilia`).
- Chequeo/verificación, arranque en frío y acceso de emergencia: **diferidos**.

---

## 1. Fórmula (única fuente de verdad: `IntegridadHelper`)

> **Verificar contra la carpeta**: las fórmulas de la carpeta están en imágenes que no pude leer. Propongo la sumatoria ponderada clásica de abajo; si la de la carpeta difiere en algún detalle (pesos cíclicos, módulo, etc.), el único lugar a tocar es `IntegridadHelper` — nada más del diseño cambia.

**Cadena base de una fila**: concatenación de todos los campos de la fila (excluyendo `DVH`), en el orden de las columnas de la tabla, separados por `|`, con formato determinístico e invariante:
- strings tal cual; enteros y decimales con `CultureInfo.InvariantCulture`;
- `bit` como `1`/`0`; fechas como `yyyy-MM-dd HH:mm:ss.fff`;
- `NULL` como cadena vacía.

**DVH** (por fila): sumatoria ponderada por posición sobre la cadena base, almacenada como `BIGINT`:

```csharp
public static long CalcularDVH(string cadenaBase)
{
    long suma = 0;

    for (int posicion = 0; posicion < cadenaBase.Length; posicion++)
    {
        suma = suma + ((long)cadenaBase[posicion] * (posicion + 1));
    }

    return suma;
}
```

**DVV** (por tabla): sumatoria de todos los DVH de la tabla (`BIGINT`), guardada en `DigitosVerticales`. Cuando exista el chequeo, permitirá detectar inserciones/borrados externos; por ahora solo se calcula y se mantiene al día.

Como todo cálculo ocurre siempre en C# leyendo las filas vía ADO.NET, **no hace falta replicar la fórmula en T-SQL** — no hay riesgo de divergencia entre dos implementaciones.

## 2. Base de datos — cambios en `Scripts/CrearBaseDatos.sql`

- Agregar columna `DVH BIGINT NULL` a las 8 tablas (última columna de cada `CREATE TABLE`).
- Nueva tabla de control:

```sql
CREATE TABLE DigitosVerticales
(
    IdDigitoVertical INT IDENTITY(1,1) NOT NULL,
    NombreTabla VARCHAR(100) NOT NULL,
    ValorDVV BIGINT NOT NULL,
    FechaCalculo DATETIME NOT NULL CONSTRAINT DF_DigitosVerticales_FechaCalculo DEFAULT (GETDATE()),
    CONSTRAINT PK_DigitosVerticales PRIMARY KEY (IdDigitoVertical),
    CONSTRAINT UQ_DigitosVerticales_NombreTabla UNIQUE (NombreTabla)
);
```

- El script **no** calcula nada (los `DVH` quedan `NULL` y `DigitosVerticales` vacía): el llenado inicial lo hace la aplicación al arrancar (punto 5).
- Nota para las tablas de clave compuesta (`UsuarioFamilia`, `UsuarioPatente`, `FamiliaPatente`, `FamiliaFamilia`): no tienen columna identity; el `UPDATE` de su DVH filtra por las columnas de la PK compuesta, y el orden de recorrido es por esas mismas columnas.

## 3. DAL — cálculo y mantenimiento

**`DAL/Integridad/IntegridadHelper.cs`** (estático — vive en el DAL, no en SEC, porque la cascada de referencias UI → BLL/SEC → DAL → BE impide que el DAL llame a SEC):
- `ConstruirCadenaBase(DataRow fila)`: aplica el formato determinístico del punto 1, salteando la columna `DVH`.
- `CalcularDVH(string cadenaBase)` y `CalcularDVV(List<long> valoresDvh)`: las fórmulas.
- `ActualizarIntegridad(string nombreTabla, string columnaId, int id)` y `ActualizarIntegridadClaveCompuesta(string nombreTabla, List<SqlParameter> clavesFila)`: la mecánica genérica — releer la fila afectada, recalcular y actualizar su DVH, recalcular la suma de DVH de la tabla y actualizar (`UPDATE`-o-`INSERT`) su fila en `DigitosVerticales`. Todo dentro de **una** conexión/transacción propia (`SqlConnection` + `SqlTransaction` con `using` clásicos, permitido por el estándar §6).

**`DAL/Contratos/IVerificable.cs`** — contrato que marca a todo repositorio cuya tabla lleva DVH/DVV y lo obliga a implementar la actualización:

```csharp
public interface IVerificable
{
    void ActualizarDVH(int id);
}
```

- El `ActualizarDVH` de cada repositorio es una delegación de una línea a `IntegridadHelper.ActualizarIntegridad(...)` con el nombre de tabla y columna id propios. La interfaz fuerza el contrato; el helper evita el copy-paste.
- **Todo método de escritura de un repositorio `IVerificable` termina llamando a su propio `ActualizarDVH(id)`** — esa es la regla: cada operación contra la tabla actualiza sus dígitos.

Repositorios que implementan `IVerificable` en este parche:

| Repositorio | Tabla propia | Métodos de escritura que llaman a `ActualizarDVH` |
| --- | --- | --- |
| `UsuarioRepositorio` | `Usuario` | `Insertar`, `Modificar`, `BajaLogica`, `ActualizarContrasena`, `ActualizarIntentosFallidos`, `ResetearIntentosFallidos`, `Desbloquear` |
| `BitacoraRepositorio` | `Bitacora` | `Registrar` (⚠️ hoy no recupera el id insertado: el `INSERT` pasa a cerrar con `SELECT CAST(SCOPE_IDENTITY() AS INT)` vía `EjecutarEscalar`, para poder llamar a `ActualizarDVH` con el `IdBitacora` nuevo) |
| `FamiliaRepositorio` | `Familia` | los métodos de escritura que tenga hoy (revisar al implementar; si es solo lectura, implementa `IVerificable` recién cuando gane escrituras) |

**Caso especial — tablas de clave compuesta sin repositorio propio** (`UsuarioFamilia`, escrita por `UsuarioRepositorio.AsignarFamilia`): `IVerificable.ActualizarDVH(int id)` mapea un repositorio a su tabla principal, así que no cubre este caso. `AsignarFamilia` llama directamente a `IntegridadHelper.ActualizarIntegridadClaveCompuesta(...)` después del `INSERT`. Las demás tablas de permisos (`UsuarioPatente`, `FamiliaPatente`, `FamiliaFamilia`) hoy no tienen escrituras desde la aplicación — se cargan por script — así que solo participan del cálculo inicial; cuando alguna gane un ABM, su repositorio nace implementando `IVerificable` o usando la variante compuesta según corresponda.

**`DAL/Contratos/IIntegridadRepositorio.cs` + `DAL/Implementaciones/IntegridadRepositorio.cs`** (instanciado vía `FabricaRepositorio`) — solo lo transversal que este parche necesita:

```csharp
bool ExisteLineaBase();
void RecalcularTodo();
```

- `ExisteLineaBase`: `true` si `DigitosVerticales` tiene al menos una fila.
- `RecalcularTodo`: recorre las 8 tablas recalculando y persistiendo DVH fila por fila y el DVV de cada una, en una única transacción. La lista de tablas críticas (nombre + columna/s id) vive como lista privada única acá, no repartida por el código. (Cuando llegue el parche de chequeo, `VerificarIntegridad()` se suma a esta misma clase y reutiliza la misma lista.)

> **Limitación aceptada (anotar y seguir):** la escritura original y la actualización de dígitos ocurren en dos transacciones separadas — si el proceso muere justo entre ambas, los dígitos quedan desactualizados hasta el próximo recálculo. Unificarlas exigiría rehacer `AccesoDatos` para compartir conexión/transacción entre llamadas; queda como endurecimiento futuro.

## 4. SEC — servicio de integridad (mínimo)

**`SEC/Contratos/IIntegridadService.cs` + `SEC/Implementaciones/IntegridadService.cs`**, instanciado vía `FabricaSeguridad`:

```csharp
void InicializarDigitos();
```

- `InicializarDigitos`: si `integridadRepositorio.ExisteLineaBase()` da `false`, llama a `RecalcularTodo()`. Si ya existe línea base, no hace nada. Es el único método por ahora — la verificación y la reparación bajo demanda se sumarán acá en el parche de chequeo.
- Sin registro en bitácora en este parche: el llenado inicial ocurre al arrancar la aplicación, sin sesión de usuario, y `Bitacora.IdUsuario` es `NOT NULL` — no hay a quién atribuírselo. La acción `ReparacionBaseDatos` de bitácora llegará con la pantalla de reparación, que sí tiene usuario ejecutor.

## 5. Web — llenado inicial al arrancar

Único cambio de la capa Web: en `Global.asax.cs`, `Application_Start` llama a `integridadService.InicializarDigitos()` (vía `FabricaSeguridad`, respetando la cascada UI → SEC → DAL). Así, con una base recién creada por el script, el primer arranque de la aplicación llena todos los DVH existentes y las 8 filas de `DigitosVerticales`, y de ahí en más los mantienen las escrituras vía `IVerificable`.

Sin pantallas nuevas, sin cambios en Login, sin errores nuevos: nada del chequeo entra en este parche.

## 6. Pasos de implementación (orden sugerido)

1. `Scripts/CrearBaseDatos.sql`: columnas `DVH` + tabla `DigitosVerticales`.
2. `DAL/Integridad/IntegridadHelper` (formato de cadena base, fórmulas, `ActualizarIntegridad` y variante de clave compuesta).
3. `DAL/Contratos/IVerificable.cs`; `IIntegridadRepositorio`/`IntegridadRepositorio` (`ExisteLineaBase`, `RecalcularTodo`) + registro en `FabricaRepositorio`.
4. Implementar `IVerificable` en `UsuarioRepositorio` y `BitacoraRepositorio` (con el cambio de `Registrar` a `SCOPE_IDENTITY`); cada método de escritura termina llamando a su `ActualizarDVH(id)`; `AsignarFamilia` usa la variante de clave compuesta.
5. `IIntegridadService`/`IntegridadService` (`InicializarDigitos`) + registro en `FabricaSeguridad`.
6. `Global.asax.cs`: llamada a `InicializarDigitos()` en `Application_Start`.
7. Probar (punto 7).

## 7. Pruebas manuales

1. **Instalación limpia**: correr el script (DVH `NULL`, `DigitosVerticales` vacía) → arrancar la aplicación → verificar por SSMS que las 8 tablas tienen DVH no nulo en todas sus filas y `DigitosVerticales` tiene 8 filas.
2. **Segundo arranque**: reiniciar la aplicación → `DigitosVerticales` no cambia `FechaCalculo` (la línea base ya existía, `InicializarDigitos` no recalcula).
3. **Escrituras mantienen los dígitos**: alta/modificación/baja/desbloqueo de usuario, cambio de clave, login exitoso y fallido (escriben bitácora e intentos fallidos), asignación de familia en el alta → después de cada una, verificar por SSMS que el DVH de la fila afectada cambió y el `ValorDVV`/`FechaCalculo` de esa tabla en `DigitosVerticales` se actualizó.
4. **Consistencia recalculable**: correr dos veces la misma operación de lectura del DVH de una fila que no cambió → mismo valor (el formato de cadena base es determinístico, sin depender de cultura del servidor).

## 8. Definición de "terminado"

- Las 8 tablas tienen columna `DVH` llena para todas las filas existentes tras el primer arranque, y `DigitosVerticales` tiene el DVV de cada una.
- Toda escritura de la aplicación deja el DVH de la fila y el DVV de su tabla al día.
- Todo repositorio con tabla verificable implementa `IVerificable` y ningún método de escritura suyo termina sin llamar a `ActualizarDVH`.
- La fórmula existe en un único lugar (`IntegridadHelper`).
- Ninguna sentencia SQL nueva concatena valores (solo nombres de tabla/columna internos de la lista fija; los valores siempre como `SqlParameter`).
- Cero cambios de comportamiento visible: ni Login, ni pantallas, ni mensajes nuevos.

## 9. Fuera de alcance / diferido (explícito)

- **Todo el chequeo**: `VerificarIntegridad`, ERR04 en Login, pantalla "Reparar Base de Datos" (CU-001-002), registro de reparación en bitácora — próximo parche, sobre los dígitos que este deja calculados.
- Arranque en frío robusto y acceso de emergencia con `Usuario` corrupta.
- Extensión a tablas futuras de negocio (Cliente, Paquete, Suscripción...): columna `DVH` + alta en la lista fija + repositorio `IVerificable`.
- Atomicidad escritura+dígitos en una única transacción (limitación aceptada, punto 3).
- Confirmar la fórmula exacta contra las imágenes de la carpeta (punto 1) — si difiere, se ajusta solo `IntegridadHelper`.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.6 | Dígitos verificadores, etapa de cálculo y mantenimiento: columna `DVH` (sumatoria ponderada) en las 8 tablas + tabla `DigitosVerticales` con el DVV por tabla; interfaz `IVerificable` (contrato `ActualizarDVH`) en los repositorios de tablas verificables, llamado por todo método de escritura; llenado inicial automático en `Application_Start` vía `IntegridadService.InicializarDigitos()`. El chequeo (ERR04, Login, Reparar Base de Datos) queda explícitamente fuera de scope para un parche posterior. |
