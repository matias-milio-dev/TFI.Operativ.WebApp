|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.3.2 — Refactor de Seguridad — Plataforma Operativ

Este documento describe **qué hay que tocar en el código ya existente** (repo de GitHub) para aplicar el parche 1.3.2. Es un documento de parche, no de entrega: no agrega funcionalidad nueva, reubica y renombra código existente para que quede alineado con `Estandares_Codigo_y_Estilo_Operativ.md` ya actualizado con este mismo criterio. Aplicable indistintamente desde esta PC o la otra, ya que ambas comparten este contexto de proyecto.

---

## 0. Objetivo del parche

Las cuestiones de **seguridad** (`Usuario`, `Familia`, `Bitácora`) estaban modeladas como lógica de **negocio** (`Operativ.BLL`), lo cual mezclaba autenticación/autorización/auditoría con las reglas de negocio específicas del dominio (Clientes, Paquetes, Suscripciones, etc.). El parche saca esas tres clases (y sus interfaces) del BLL y las lleva a `Operativ.SEC`, y reubica dos piezas transversales (`ErroresHandler` y `EmailHelper`) en las capas que les corresponden por naturaleza.

Con esto, `Operativ.BLL` queda reservado **exclusivamente** para lógica de negocio específica del dominio.

## 1. Alcance

**Incluido en este parche:**

1. `FamiliaNegocio`, `UsuarioNegocio`, `BitacoraNegocio` → `Operativ.SEC`, renombradas `FamiliaService`, `UsuarioService`, `BitacoraService`.
2. Instanciación de las tres vía una nueva `FabricaSeguridad` (Abstract Factory, análoga a `FabricaNegocio`/`FabricaRepositorio`), en `SEC/Fabricas/`.
3. Sus interfaces (`IFamiliaNegocio`, `IUsuarioNegocio`, `IBitacoraNegocio`) migran y se adaptan a `IFamiliaService`, `IUsuarioService`, `IBitacoraService`, en `SEC/Contratos/`.
4. `ErroresHandler` + constantes de error: `BLL/Errores/` → `BE/Errores/`.
5. `EmailHelper`: `BLL/Helpers/` → `SEC/Helpers/`.

**Explícitamente fuera de este parche:** no cambia ningún comportamiento funcional (mismo flujo de login, bloqueo, recuperación de contraseña, registro de bitácora); es un refactor puro de ubicación/nombre. No se tocan DAL, BE/Entidades, Composite ni la UI más allá de los `using`/llamadas a las clases movidas.

> Nota de verificación: al preparar este plan encontré en el contexto del proyecto fragmentos de código con una convención distinta (`UsuarioBLL`, `BitacoraBLL`, namespace `Operativ.DAL` con sufijo `*DAL`) que no coincide con `Estandares_Codigo_y_Estilo_Operativ.md`. Puede tratarse de una iteración vieja indexada. **Antes de aplicar los pasos de la sección 3, confirmá contra el repo real los nombres de archivo y clase vigentes** (puede que ya sean `UsuarioNegocio`/`UsuarioRepositorio` como dice el estándar, o puede que todavía tengan el sufijo viejo — en ese caso el mapeo de la sección 2 es el mismo, solo cambia el nombre de origen).

## 2. Mapeo de movimientos

| # | Elemento | Ubicación / nombre actual | Ubicación / nombre nuevo |
| --- | --- | --- | --- |
| 1 | Interfaz Usuario | `Operativ.BLL/Contratos/IUsuarioNegocio.cs` | `Operativ.SEC/Contratos/IUsuarioService.cs` |
| 2 | Implementación Usuario | `Operativ.BLL/Implementaciones/UsuarioNegocio.cs` | `Operativ.SEC/Implementaciones/UsuarioService.cs` |
| 3 | Interfaz Familia | `Operativ.BLL/Contratos/IFamiliaNegocio.cs` | `Operativ.SEC/Contratos/IFamiliaService.cs` |
| 4 | Implementación Familia | `Operativ.BLL/Implementaciones/FamiliaNegocio.cs` | `Operativ.SEC/Implementaciones/FamiliaService.cs` |
| 5 | Interfaz Bitácora | `Operativ.BLL/Contratos/IBitacoraNegocio.cs` | `Operativ.SEC/Contratos/IBitacoraService.cs` |
| 6 | Implementación Bitácora | `Operativ.BLL/Implementaciones/BitacoraNegocio.cs` | `Operativ.SEC/Implementaciones/BitacoraService.cs` |
| 7 | Fábrica | `Operativ.BLL/Fabricas/FabricaNegocio.cs` (métodos `CrearUsuarioNegocio`, `CrearFamiliaNegocio`, `CrearBitacoraNegocio` se dan de baja de acá) | `Operativ.SEC/Fabricas/FabricaSeguridad.cs` (nueva, con `CrearUsuarioService`, `CrearFamiliaService`, `CrearBitacoraService`) |
| 8 | Errores | `Operativ.BLL/Errores/ErroresHandler.cs` + constantes | `Operativ.BE/Errores/ErroresHandler.cs` + constantes |
| 9 | Email | `Operativ.BLL/Helpers/EmailHelper.cs` | `Operativ.SEC/Helpers/EmailHelper.cs` |

## 3. Pasos de implementación (orden sugerido)

1. **Crear carpetas destino** si no existen: `SEC/Contratos/`, `SEC/Implementaciones/`, `SEC/Fabricas/`, `BE/Errores/`.
2. **Mover y renombrar las 3 interfaces** (filas 1, 3, 5 de la tabla): cambiar el nombre del archivo y del tipo (`IUsuarioNegocio` → `IUsuarioService`, etc.), cambiar el namespace de `Operativ.BLL` a `Operativ.SEC`. Revisar cada firma de método: si algún nombre quedó con vocabulario de negocio en vez de seguridad, ajustarlo (ej. si hubiera algo como `IUsuarioNegocio.ValidarYRegistrar(...)`, evaluar si conviene dividir la parte de validación de la de registro).
3. **Mover y renombrar las 3 implementaciones** (filas 2, 4, 6): mismo cambio de nombre/namespace, implementar la interfaz ya renombrada, y actualizar sus `using` internos (van a dejar de necesitar `using Operativ.BLL;` para las cosas que se movieron, pero van a necesitar `using Operativ.BE.Errores;` para `ErroresHandler`, y quizás `using Operativ.SEC.Helpers;` para `EmailHelper` si antes lo referenciaban con `using Operativ.BLL.Helpers;`).
4. **Crear `FabricaSeguridad`** en `SEC/Fabricas/`, siguiendo el mismo patrón (Abstract Factory, misma forma de instanciación — Singleton o estática según cómo esté hecha `FabricaNegocio`/`FabricaRepositorio` hoy en el repo) con `CrearUsuarioService()`, `CrearFamiliaService()`, `CrearBitacoraService()`. Dar de baja los métodos equivalentes en `FabricaNegocio`.
5. **Mover `ErroresHandler` y las constantes de error** (fila 8) a `BE/Errores/`, namespace `Operativ.BE.Errores` (o el que use hoy `Operativ.BE`, manteniendo consistencia con el resto de esa capa).
6. **Mover `EmailHelper`** (fila 9) a `SEC/Helpers/`, namespace `Operativ.SEC`.
7. **Actualizar todos los call-sites**, principalmente en `Operativ.Web`:
   - `Login.aspx.cs`: reemplazar el uso de `FabricaNegocio.Instancia.CrearUsuarioNegocio()` (o como esté instanciado hoy) por `FabricaSeguridad.Instancia.CrearUsuarioService()`; actualizar `using`.
   - `RecuperarContrasena.aspx.cs`: ídem para `UsuarioService`, y verificar el `using` de `EmailHelper` si se lo referencia directo desde ahí.
   - Cualquier página de administración de usuarios/familias (`GestionUsuarios`, `GestionFamiliaPatente` si ya existen en el repo) y cualquier punto donde se registre bitácora (`BitacoraService.Registrar(...)`).
   - Buscar en toda la solución referencias a `IUsuarioNegocio`, `IFamiliaNegocio`, `IBitacoraNegocio`, `UsuarioNegocio`, `FamiliaNegocio`, `BitacoraNegocio` y a `using Operativ.BLL` donde ya no corresponda, y corregir.
8. **Verificar referencias de proyecto** (`.csproj`): en principio no hace falta agregar ninguna, porque la cascada de referencias ya vigente (`UI → BLL / SEC → DAL → BE`) contempla que `SEC` referencie `DAL` y `BE` igual que `BLL`. Si `Operativ.SEC` todavía no referencia `Operativ.DAL` (porque antes no lo necesitaba), agregar la referencia.
9. **Compilar la solución completa** y resolver los errores de referencia rota uno por uno (van a aparecer principalmente en `Operativ.Web`).
10. **Probar el flujo funcional sin cambios de comportamiento**: login con los 4 usuarios semilla, bloqueo al 3er intento fallido, recuperación de contraseña con envío de email, y si Bitácora ya está en uso, que el registro de eventos siga funcionando igual.

## 4. Definición de "terminado" para este parche

- La solución compila sin referencias rotas a `Operativ.BLL` desde código que ya no debería depender de él.
- `Operativ.BLL/Contratos/` y `Operativ.BLL/Implementaciones/` no contienen ninguna clase relacionada a Usuario, Familia o Bitácora.
- `IUsuarioService`, `IFamiliaService`, `IBitacoraService` están en `SEC/Contratos/`; sus implementaciones en `SEC/Implementaciones/`; se instancian únicamente vía `FabricaSeguridad`.
- `ErroresHandler` y las constantes de error viven en `Operativ.BE/Errores/` y son accesibles sin referencia circular desde BLL, SEC y Web.
- `EmailHelper` vive en `Operativ.SEC/Helpers/`.
- El comportamiento observable de la aplicación (login, bloqueo, recuperación de contraseña, bitácora) es idéntico al previo al parche.
- `Estandares_Codigo_y_Estilo_Operativ.md` refleja esta convención (✅ ya actualizado en este contexto, ver su historial de cambios, entrada 1.3.2).

## 5. Riesgos y puntos a confirmar contra el repo real

- Si alguna clase de negocio genuina (por ejemplo, facturación) llegara a depender de `EmailHelper` para notificaciones que no son de seguridad, no hay problema: `BLL` puede referenciar `SEC` igual que hoy referencia `DAL`, así que `EmailHelper` sigue siendo alcanzable desde el BLL sin romper la cascada de capas.
- Confirmar si `BitacoraNegocio`/`IBitacoraNegocio` ya existen en el repo actual (Bitácora estaba fuera del alcance de la Entrega Oficial 1) o si Bitácora se agregó en una entrega/mejora posterior no reflejada en los documentos de plan que tengo en este contexto. Si todavía no existe, este parche solo aplica a los puntos de Usuario y Familia, y deja `IBitacoraService`/`BitacoraService`/`FabricaSeguridad.CrearBitacoraService()` preparados para cuando se implemente.
- Revisar que no queden `using Operativ.BLL;` colgando en archivos que solo lo necesitaban por las clases movidas.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.3.2 | Plan inicial: refactor de seguridad — `FamiliaNegocio`/`UsuarioNegocio`/`BitacoraNegocio` → `FamiliaService`/`UsuarioService`/`BitacoraService` en SEC vía `FabricaSeguridad`; `ErroresHandler` → BE; `EmailHelper` → SEC. |
