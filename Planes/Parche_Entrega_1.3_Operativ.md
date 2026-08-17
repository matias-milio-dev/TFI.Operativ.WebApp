# Parche Entrega 1.3 — Bitácora / Auditoría — Plataforma Operativ

Este parche se aplica sobre el código ya generado de la Entrega Oficial 1 + Parche 1.1 (multi-idioma y estilo) + Parche 1.2 (ajustes de Notificaciones). Agrega el requerimiento de **Bitácora**: la tabla en base de datos y el grabado de eventos en los puntos exactos que especifica la carpeta de casos de uso (STF, sección 11) para la funcionalidad ya implementada en esta entrega (Login, Recuperar Contraseña, Logout). No se toca ninguna otra regla de negocio, flujo ni pantalla ya construida.

Se apoya en `Estandares_Codigo_y_Estilo_Operativ.md` (sigue vigente todo lo ahí definido) y actualiza el checklist de `Plan_Entregable_1_Operativ.md`: el punto **4. Bitácora / Auditoría** pasa a **incluido** (parcialmente: solo el registro, no la consulta — ver sección 6).

---

## 1. Objetivo y alcance

Los casos de uso de la carpeta STF marcan el registro en bitácora (`#R5`) como paso del sistema en prácticamente todas las operaciones de negocio, pero **en esta entrega el único código que existe** es Login, Recuperar Contraseña y Logout. Este parche agrega bitácora únicamente en los 4 puntos que ya están implementados y que los casos de uso especifican explícitamente para ellos:

| # | Caso de uso (STF, sección 11) | Momento exacto según la especificación narrativa |
| --- | --- | --- |
| 1 | **CU-001-001 Login** (curso normal) | *"El Sistema registra en bitacora el ingreso"* — tras validar credenciales correctamente. |
| 2 | **CU-001-001 Login** (alternativa 4.1) | *"El Sistema procesa que se ingresó tres veces incorrectamente la contraseña... El Sistema registra en bitacora el bloqueo de usuario"* — en el momento exacto del 3er intento fallido, no en cada intento fallido. |
| 3 | **CU-001-012 Recuperar Contraseña** | *"El sistema registra en bitácora la operación [#R5]"* — tras actualizar la contraseña, antes de mostrarla al usuario. |
| 4 | **CU-001-013 Logout** | *"El sistema registra la operación en bitácora [#R5]"* — tras invalidar la autenticación, antes de redirigir a Login. |

Todo lo demás que menciona `#R5` en la carpeta STF (alta/baja de usuarios, familias, patentes, clientes, paquetes, suscripciones, pagos, facturas, activos, incidentes) corresponde a módulos que **no existen todavía** en el código (están fuera del alcance de la Entrega Oficial 1, sección 13). Cuando esos módulos se construyan en una entrega futura, van a llamar al mismo `IBitacoraRepositorio` que este parche introduce.

---

## 2. Base de datos

### 2.1 Tabla `Bitacora`

El caso de uso CU-003-011 "Consultar Bitácora" especifica que los filtros de búsqueda son **fecha, usuario, acción y criticidad** — esas son las columnas mínimas necesarias.

Agregar este bloque a `Scripts/CrearBaseDatos.sql`, **después** de la tabla `Usuario` (por la FK) y antes de los `INSERT` de datos semilla — por ejemplo a continuación de `FamiliaFamilia`:

```sql
CREATE TABLE Bitacora
(
    IdBitacora INT IDENTITY(1,1) NOT NULL,
    IdUsuario INT NOT NULL,
    FechaHora DATETIME NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT (GETDATE()),
    Accion VARCHAR(50) NOT NULL,
    Criticidad VARCHAR(20) NOT NULL,
    Descripcion VARCHAR(300) NULL,
    CONSTRAINT PK_Bitacora PRIMARY KEY (IdBitacora),
    CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario (IdUsuario)
);
GO
```

No lleva datos semilla: se puebla sola cuando se ejecutan Login, Recuperar Contraseña y Logout.

**Nota (fuera de alcance, no implementar ahora):** la sección 10.8.3 de la carpeta STF menciona que cada entrada de bitácora se valida con su propio dígito verificador ("para asegurar que el rastro de auditoría no haya sido manipulado"). Eso es DVH/DVV, que en `Plan_Entregable_1_Operativ.md` (checklist ítem 15) está explícitamente sin marcar para entregas futuras. **No agregar columnas de DVH/DVV a `Bitacora` en este parche.**

### 2.2 Supuesto a confirmar: valores de `Criticidad`

La carpeta STF no define una lista cerrada de valores de criticidad para bitácora (solo aparece como filtro en CU-003-011). Para mantener consistencia con la clasificación que ya existe en el Anexo de errores (`Advertencia`, `Critico`, `Grave`), este parche propone:

| Valor | Uso |
| --- | --- |
| `Informativo` | Eventos normales del sistema (login exitoso, logout). No existe en el Anexo de errores porque esos son casos de éxito, no errores. |
| `Advertencia` | Eventos sensibles pero no críticos (recuperación de contraseña). |
| `Critico` | Eventos de seguridad relevantes (bloqueo de usuario) — mismo nivel que `#ERR03` en el Anexo. |
| `Grave` | Reservado para eventos futuros (ej. falla de integridad, reparación de BD) — no se usa en este parche. |

Si preferís otros nombres o niveles, decímelo y ajusto la tabla antes de pasarlo a Claude Code — es la única parte de este plan que no sale directamente de un documento existente.

---

## 3. Capa de entidades (BE)

### 3.1 Enums nuevos

`Operativ.BE/Enums/TipoAccionBitacora.cs`:

```csharp
namespace Operativ.BE.Enums
{
    public enum TipoAccionBitacora
    {
        LoginExitoso,
        LoginBloqueado,
        RecuperacionContrasena,
        CierreSesion
    }
}
```

`Operativ.BE/Enums/CriticidadBitacora.cs`:

```csharp
namespace Operativ.BE.Enums
{
    public enum CriticidadBitacora
    {
        Informativo,
        Advertencia,
        Critico,
        Grave
    }
}
```

### 3.2 Entidad nueva

`Operativ.BE/Entidades/Bitacora.cs`:

```csharp
using System;
using Operativ.BE.Enums;

namespace Operativ.BE.Entidades
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }

        public int IdUsuario { get; set; }

        public DateTime FechaHora { get; set; }

        public TipoAccionBitacora Accion { get; set; }

        public CriticidadBitacora Criticidad { get; set; }

        public string Descripcion { get; set; }
    }
}
```

---

## 4. Capa de acceso a datos (DAL)

Sigue el mismo patrón que `UsuarioRepositorio`/`IUsuarioRepositorio` ya existentes.

### 4.1 Contrato

`Operativ.DAL/Contratos/IBitacoraRepositorio.cs`:

```csharp
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos
{
    public interface IBitacoraRepositorio
    {
        void Registrar(Bitacora entrada);
    }
}
```

### 4.2 Implementación

`Operativ.DAL/Implementaciones/BitacoraRepositorio.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Conexion;

namespace Operativ.DAL.Implementaciones
{
    public class BitacoraRepositorio : IBitacoraRepositorio
    {
        private readonly AccesoDatos accesoDatos;

        public BitacoraRepositorio()
        {
            accesoDatos = new AccesoDatos();
        }

        public void Registrar(Bitacora entrada)
        {
            string consulta = "INSERT INTO Bitacora (IdUsuario, Accion, Criticidad, Descripcion) "
                + "VALUES (@IdUsuario, @Accion, @Criticidad, @Descripcion)";

            object descripcion = entrada.Descripcion ?? (object)DBNull.Value;

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", entrada.IdUsuario),
                new SqlParameter("@Accion", entrada.Accion.ToString()),
                new SqlParameter("@Criticidad", entrada.Criticidad.ToString()),
                new SqlParameter("@Descripcion", descripcion)
            };

            accesoDatos.EjecutarConsulta(consulta, parametros);
        }
    }
}
```

`FechaHora` no se manda como parámetro: la completa el `DEFAULT (GETDATE())` de la tabla.

No hace falta `Convertidores/BitacoraConvertidor` todavía porque este parche no lee bitácora, solo escribe (eso llega con la futura pantalla de Consultar Bitácora — ítem 6).

### 4.3 Fábrica

Agregar a `Operativ.DAL/Fabricas/FabricaRepositorio.cs` (no tocar los métodos existentes):

```csharp
public IBitacoraRepositorio CrearBitacoraRepositorio()
{
    return new BitacoraRepositorio();
}
```

---

## 5. Capa de negocio (BLL) y web (UI)

### 5.1 Dónde vive la lógica de registro

Los 4 puntos de registro caen en flujos que ya pasan por `UsuarioNegocio` (Login, Recuperar Contraseña) o que hoy resuelven todo en el code-behind sin pasar por BLL (Logout, en `ResumenUsuario.ascx.cs`). Este parche:

- Agrega el registro de bitácora **dentro de `UsuarioNegocio`** para los 3 puntos que ya están ahí (login exitoso, login bloqueado, recuperar contraseña) — consistente con que en los casos de uso el registro en bitácora es parte de la respuesta del sistema junto con la operación de negocio, no de la UI.
- Agrega un método nuevo `RegistrarCierreSesion(int idUsuario)` a `IUsuarioNegocio`/`UsuarioNegocio` para que Logout también pase por BLL en vez de escribir a bitácora directo desde el code-behind.
- **No crea** una `IBitacoraNegocio` separada: no hay ninguna regla de negocio propia sobre bitácora todavía (solo un insert), así que agregar una capa BLL extra sería sobre-ingeniería para esta entrega. Cuando llegue Consultar Bitácora con sus filtros, ahí sí va a tener sentido.

### 5.2 Cambios en `Operativ.BLL/Contratos/IUsuarioNegocio.cs`

Agregar una firma nueva:

```csharp
void RegistrarCierreSesion(int idUsuario);
```

### 5.3 Cambios en `Operativ.BLL/Implementaciones/UsuarioNegocio.cs`

- Agregar `using Operativ.BE.Enums;` (ya está, se usa `TipoError`) y el campo `private readonly IBitacoraRepositorio bitacoraRepositorio;`, inicializado en el constructor igual que `usuarioRepositorio`:

```csharp
public UsuarioNegocio()
{
    FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
    usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();
    bitacoraRepositorio = fabricaRepositorio.CrearBitacoraRepositorio();
}
```

- Agregar un método privado para no repetir la construcción de la entidad en los 4 puntos:

```csharp
private void RegistrarBitacora(int idUsuario, TipoAccionBitacora accion, CriticidadBitacora criticidad, string descripcion)
{
    Bitacora entrada = new Bitacora
    {
        IdUsuario = idUsuario,
        Accion = accion,
        Criticidad = criticidad,
        Descripcion = descripcion
    };

    bitacoraRepositorio.Registrar(entrada);
}
```

- En `ValidarCredenciales`, después de `usuarioRepositorio.ResetearIntentosFallidos(usuario.IdUsuario);` y antes del `return usuario;`:

```csharp
RegistrarBitacora(usuario.IdUsuario, TipoAccionBitacora.LoginExitoso, CriticidadBitacora.Informativo, "Inicio de sesión exitoso");
```

- En `RegistrarIntentoFallido`, dentro del `if (bloqueado)`, después de `usuarioRepositorio.ActualizarIntentosFallidos(...)` y **antes** del `throw new OperativException(TipoError.ErrorUsuarioBloqueado, ...)`:

```csharp
if (bloqueado)
{
    RegistrarBitacora(usuario.IdUsuario, TipoAccionBitacora.LoginBloqueado, CriticidadBitacora.Critico,
        string.Format("Usuario bloqueado tras {0} intentos fallidos", ConfiguracionAplicacion.IntentosMaximosLogin));

    throw new OperativException(TipoError.ErrorUsuarioBloqueado, new string[] { usuario.NombreUsuario });
}
```

- En `RecuperarContrasena`, después de `usuarioRepositorio.ActualizarContrasena(...)` y antes de que el método termine:

```csharp
RegistrarBitacora(usuario.IdUsuario, TipoAccionBitacora.RecuperacionContrasena, CriticidadBitacora.Advertencia, "Contraseña restablecida por recuperación");
```

- Método público nuevo:

```csharp
public void RegistrarCierreSesion(int idUsuario)
{
    RegistrarBitacora(idUsuario, TipoAccionBitacora.CierreSesion, CriticidadBitacora.Informativo, "Cierre de sesión");
}
```

### 5.4 Cambios en `Operativ.Web/Controles/ResumenUsuario.ascx.cs`

En `lnkCerrarSesion_Click`, registrar en bitácora **antes** de limpiar la sesión (después de `CerrarSesion()` ya no hay forma de obtener el `IdUsuario`). El registro de bitácora no debe impedir el logout si falla (por ejemplo, sin conexión a la base): envolver solo esa llamada en un `try/catch` que no se muestre al usuario, para que cerrar sesión funcione siempre.

```csharp
protected void lnkCerrarSesion_Click(object sender, EventArgs e)
{
    Usuario usuario = sesionHandler.GetUsuario();

    if (usuario != null)
    {
        try
        {
            FabricaNegocio fabricaNegocio = new FabricaNegocio();
            IUsuarioNegocio usuarioNegocio = fabricaNegocio.CrearUsuarioNegocio();
            usuarioNegocio.RegistrarCierreSesion(usuario.IdUsuario);
        }
        catch (Exception)
        {
        }
    }

    sesionHandler.CerrarSesion();
    Response.Redirect("~/Login.aspx");
}
```

Agregar los `using` correspondientes (`Operativ.BLL.Contratos`, `Operativ.BLL.Fabricas`) al archivo.

**Nota de estándares:** el `catch (Exception) { }` vacío es la única excepción deliberada a "todo error se canaliza por `ErroresHandler`" — es una decisión funcional explícita de este parche (una falla al auditar el logout no debe bloquear el logout), no un olvido. Si preferís que sí se muestre un error en este caso puntual, decímelo y lo ajusto.

---

## 6. Explícitamente fuera de alcance de este parche

- **CU-003-011 Consultar Bitácora**: la pantalla de listado/filtrado (`8.GestionBitacora.aspx` según la carpeta STF), reservada a Web Master. Requiere validación de patente individual, que en esta entrega todavía no existe (checklist ítem 10 de `Plan_Entregable_1_Operativ.md` sigue sin marcar). Cuando se implemente, va a necesitar `IBitacoraRepositorio.Listar(filtros)` y un `BitacoraConvertidor`.
- **DVH/DVV en `Bitacora`**: mencionado en la sección 10.8.3 de la carpeta STF, pero fuera de esta entrega (ver 2.1).
- Registro de bitácora en cualquier otro caso de uso (ABM de usuarios, familias, patentes, clientes, etc.): no existe código para esos módulos todavía.

---

## 7. Actualización de documentación

- `Plan_Entregable_1_Operativ.md`, checklist (sección 1): marcar el ítem **4. Bitácora / Auditoría** como incluido, aclarando entre paréntesis "(solo registro; consulta queda para una entrega futura)".
- `Plan_Entregable_1_Operativ.md`, sección 13 ("Explícitamente FUERA de esta entrega"): quitar "Bitácora" de esa lista, ya que a partir de este parche sí está incluida (parcialmente).

Estos dos archivos no están commiteados en el repo (viven como documentos locales tuyos junto con `Estandares_Codigo_y_Estilo_Operativ.md`); actualizalos vos o pedime que te devuelva las versiones actualizadas.

---

## 8. Criterio de "terminado"

- La solución sigue compilando con los 5 proyectos.
- `Scripts/CrearBaseDatos.sql` crea la tabla `Bitacora` con la FK a `Usuario`, sin datos semilla.
- Flujo verificable en SQL Server Management Studio (o similar), consultando `SELECT * FROM Bitacora ORDER BY IdBitacora DESC` entre cada paso:
  - Login exitoso con cualquier usuario semilla → aparece una fila `LoginExitoso` / `Informativo` con su `IdUsuario`.
  - Fallar el login 3 veces seguidas con el mismo usuario → aparece **una sola** fila `LoginBloqueado` / `Critico` (no una por cada intento fallido).
  - Recuperar contraseña de un usuario existente → aparece una fila `RecuperacionContrasena` / `Advertencia`.
  - Logout desde `ResumenUsuario.ascx` → aparece una fila `CierreSesion` / `Informativo`.
- El código respeta `Estandares_Codigo_y_Estilo_Operativ.md` (español, sin comentarios, sin `var`, sin tuplas, sin delegados en acceso a datos, interfaces en `Contratos/`, instanciación solo vía fábricas, `EjecutarConsulta` para el insert).

---

*Este parche complementa, no reemplaza, a `Plan_Entregable_1_Operativ.md`, `Estandares_Codigo_y_Estilo_Operativ.md`, `Parche_Entrega_1.1_Operativ.md` y `Parche_Entrega_1.2_Operativ.md`. Pasar estos documentos junto con `CLAUDE.md` a Claude Code para aplicar esta iteración (Entrega 1.3).*
