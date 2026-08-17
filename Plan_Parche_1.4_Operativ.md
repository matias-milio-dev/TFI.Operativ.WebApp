|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.4 — ABM de Usuarios — Plataforma Operativ

Parche de **implementación** (agrega funcionalidad nueva, a diferencia del 1.3.2 que fue puramente estructural). Se apoya en la reorganización del parche 1.3.2: como `UsuarioService` ya vive en `Operativ.SEC`, el ABM de Usuarios entra ahí directamente, sin volver a tocar el BLL.

---

## 0. Objetivo y alcance

Agregar Alta, Baja (lógica) y Modificación de usuarios, con su pantalla en `Operativ.Web`, exclusiva para el perfil **Administrador**.

**Incluido:**
1. `UsuarioService` pasa a ser `partial class`: una parte con lo que ya existe (credenciales, intentos fallidos, recuperar contraseña — del parche 1.3.2) y otra parte nueva con el ABM.
2. Pantalla `GestionUsuarios.aspx`, con la hoja de estilos y Master Page ya establecidas, validadores de formato y obligatoriedad.
3. Validación de unicidad de nombre de usuario **y** de email, cada una con su propio mensaje de error.
4. Listado de usuarios (necesario para que el ABM tenga sobre qué operar — no estaba enumerado explícitamente pero es la "R" implícita de cualquier ABM).

**Fuera de alcance de este parche** (para no mezclar con el pedido): desbloqueo manual de usuarios, cambio de contraseña por el propio usuario, validación de patentes individuales, internacionalización. Si se quiere alguno de estos, mejor como parche aparte.

> **Nota de verificación importante**: en el contexto del proyecto conviven fragmentos de código con dos convenciones distintas. Una coincide con `Estandares_Codigo_y_Estilo_Operativ.md` (`UsuarioService`, `OperativException`, `TipoError`, `ErroresHandler`, namespace `Operativ.BE`/`Operativ.SEC`). Otra usa nombres distintos (`ExcepcionNegocio`, `ManejadorErrores`, `Operativ.Comun`, `Master.SiteMaster`, `Administracion.aspx`). Este plan está escrito con la convención vigente en los estándares. Si al abrir el repo real la convención instalada es la segunda, la tabla de equivalencias del punto 6 sirve para adaptar los nombres sin cambiar el diseño.

## 1. `UsuarioService` como partial class

- `SEC/Implementaciones/UsuarioService.cs` (ya existe desde el 1.3.2): mantiene `ValidarCredenciales`, manejo de intentos fallidos/bloqueo, `RecuperarClave`, `ObtenerUsuario` (el que trae usuario + familia/perfil para la sesión).
- `SEC/Implementaciones/UsuarioService.Abm.cs` (nuevo): contiene los métodos de ABM. Mismo namespace `Operativ.SEC`, misma clase `public partial class UsuarioService : IUsuarioService`, `private readonly` con las mismas dependencias que ya tenga la clase (no se duplican los campos, van una sola vez en cualquiera de los dos archivos — sugerido: quedan en `UsuarioService.cs` porque ya están ahí desde el 1.3.2).
- `IUsuarioService` (en `SEC/Contratos/`) no se parte: es una sola interfaz, se le agregan las firmas nuevas. La interfaz no sabe ni le importa que la implementación esté partida en dos archivos.

Métodos nuevos en `IUsuarioService` / `UsuarioService.Abm.cs`:

```csharp
int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int idFamilia);
void ModificarUsuario(Usuario usuario);
void BajaUsuario(int idUsuario);
Usuario ObtenerUsuarioPorId(int idUsuario);
List<Usuario> ListarUsuarios(string filtro, int numeroPagina, int tamanioPagina);
```

- `AltaUsuario`: valida obligatoriedad y unicidad (punto 3), genera contraseña temporal (`HashHelper.GenerarClaveTemporal()` + `GenerarSalt()` + `CalcularHash()`, ya usado en `RecuperarClave`), inserta el usuario vía `usuarioRepositorio.Insertar(...)`, asocia la familia elegida vía `usuarioRepositorio.AsignarFamilia(idUsuario, idFamilia)`, y envía el email de bienvenida con `EmailHelper` (mismo mecanismo que la recuperación de contraseña — es la razón por la que tiene sentido que todo esto viva en SEC).
- `ModificarUsuario`: valida obligatoriedad y unicidad **excluyendo el propio registro** (si el usuario no cambió su email, no se debe marcar como duplicado contra sí mismo).
- `BajaUsuario`: baja lógica — `Activo = false` (la tabla `Usuario` ya tiene esa columna, no hace falta tocar el modelo de datos), nunca `DELETE`.
- `ListarUsuarios`: para la grilla de la pantalla, con filtro de texto y paginación (mismo patrón `numeroPagina`/`tamanioPagina` usado en otros lados del proyecto).

## 2. Validación de unicidad (punto 3 del pedido)

Se valida en `UsuarioService.Abm.cs`, contra el DAL, antes de escribir en base:

- Nombre de usuario duplicado → reutiliza el error ya existente `ErrorUsuarioYaExiste` (**ERR06** — "El usuario ya existe en el sistema", ya está en el anexo de errores).
- Email duplicado → **no hay código de error para esto en el anexo actual** (que llega hasta ERR26). Se agrega uno nuevo:

| Constante | Código | Mensaje |
| --- | --- | --- |
| `ErrorEmailYaRegistrado` | **ERR27** (nuevo) | El correo electrónico ya se encuentra registrado |

Ambas constantes se dan de alta en `BE/Errores/` (ya reubicado ahí por el parche 1.3.2). Falta actualizar el anexo 12.1 de `Carpeta_Tecnologia_STF_-Matias_Milio.docx` con la fila de ERR27 — no lo edito acá porque es el documento formal de la carpeta de tecnología; avisame si querés que te arme esa fila para pegarla.

Para que `ModificarUsuario` no se dispare a sí mismo, el chequeo de unicidad necesita excluir el propio `IdUsuario`:

```csharp
bool ExisteNombreUsuario(string nombreUsuario, int? idUsuarioExcluir);
bool ExisteEmail(string correoElectronico, int? idUsuarioExcluir);
```

Estos dos métodos van en `IUsuarioRepositorio`/`UsuarioRepositorio` (DAL), implementados con `EjecutarEscalar` (`SELECT COUNT(*) ... WHERE ... AND IdUsuario <> @IdUsuarioExcluir` cuando corresponde). No hace falta exponerlos en `IUsuarioService`: son detalle de implementación del ABM, no algo que la UI necesite llamar directo.

`AltaUsuario`/`ModificarUsuario` lanzan `OperativException` con el `TipoError` correspondiente si alguno de los dos chequeos da positivo — mismo patrón `?? throw new OperativException(...)` o `if` explícito con llaves, según corresponda, ya establecido en los estándares.

## 3. DAL — cambios en `Operativ.DAL`

`IUsuarioRepositorio` / `UsuarioRepositorio` suman:

| Método | Devuelve | Notas |
| --- | --- | --- |
| `Insertar(Usuario usuario)` | `int` (nuevo `IdUsuario`, vía `EjecutarEscalar` + `SCOPE_IDENTITY`) | |
| `Modificar(Usuario usuario)` | `void`/`int` filas afectadas | |
| `BajaLogica(int idUsuario)` | `void`/`int` | `UPDATE Usuario SET Activo = 0` |
| `AsignarFamilia(int idUsuario, int idFamilia)` | `void`/`int` | `INSERT` en `UsuarioFamilia` |
| `Listar(string filtro, int numeroPagina, int tamanioPagina)` | `List<Usuario>` | pagina con `OFFSET`/`FETCH NEXT`, filtro por nombre de usuario o email vía `LIKE` parametrizado |
| `ExisteNombreUsuario(string nombreUsuario, int? idUsuarioExcluir)` | `bool` | `EjecutarEscalar` |
| `ExisteEmail(string correoElectronico, int? idUsuarioExcluir)` | `bool` | `EjecutarEscalar` |

`UsuarioConvertidor` (en `DAL/Convertidores/`) ya tiene `ToUsuario`/`ToListaUsuarios`; no necesita cambios salvo que el `SELECT` de `Listar` traiga alguna columna nueva.

No hay cambios de esquema: `Usuario` (`IdUsuario`, `NombreUsuario` unique, `ClaveHash`, `Salt`, `Email`, `NombreCompleto`, `Bloqueado`, `IntentosFallidos`, `Activo`) y `UsuarioFamilia` ya contemplan todo lo que hace falta.

## 4. UI — `GestionUsuarios.aspx`

- Ubicación: `Operativ.Web/Paginas/GestionUsuarios.aspx`, usa la Master Page (`Principal.Master`) igual que el resto — hereda automáticamente la hoja de estilos, `Navbar`, `ResumenUsuario` y `Footer` ya establecidos. No se crea CSS nuevo ni se aparta de la paleta existente.
- **Autorización** (punto 2 del pedido, "seguir los estilos establecidos" incluye también el patrón de autorización ya fijado en el estándar §9): `Page_Load` valida sesión activa (si no, `Login.aspx`) y perfil Administrador (`AutorizacionHandler.EsPerfil("Administrador")`; si no, `NoAutorizado.aspx`).
- **Navbar.ascx**: se agrega el link "Usuarios" → `GestionUsuarios.aspx`, visible solo para el perfil Administrador (mismo mecanismo condicional que ya usa el Navbar para mostrar opciones por perfil).
- **Estructura de la pantalla**: grilla con los usuarios existentes (`ListarUsuarios`) + panel de formulario para Alta/Modificación (mismo panel, cambia de modo según si se entra por "Nuevo Usuario" o por "Editar" de una fila — evita duplicar el formulario en dos vistas).
- **Campos y validaciones** (punto 2 del pedido):

| Campo | Control | Validadores |
| --- | --- | --- |
| Nombre de usuario | `TextBox` | `RequiredFieldValidator`; solo editable en Alta, deshabilitado en Modificación (es la clave de login, no se cambia desde acá) |
| Nombre completo | `TextBox` | `RequiredFieldValidator` |
| Correo electrónico | `TextBox` | `RequiredFieldValidator` + `RegularExpressionValidator` con patrón de email estándar |
| Familia / Perfil | `DropDownList` (WebMaster, Administrador, Comercial, Cliente) | `RequiredFieldValidator` (con `InitialValue` en el ítem placeholder para que cuente como "sin elegir") |

  Todos los validadores reforzados del lado servidor antes de llamar a `UsuarioService` (server no confía en que el cliente haya corrido el validador — estándar §9).
- **Mensajes**: éxito y error (`ERRXX - {mensaje}`, incluidos `ERR06` y el nuevo `ERR27`) se muestran únicamente vía `Notificaciones.ascx`, igual que en Login y Recuperar Contraseña. Nada de `Label` sueltos con texto de error hardcodeado.
- **Baja**: botón "Dar de baja" por fila de la grilla, con confirmación simple (`OnClientClick="return confirm(...)"` es aceptable acá, no hace falta más), llama a `BajaUsuario` y refresca la grilla (que deja de listar los usuarios con `Activo = 0`, salvo que se agregue un filtro de "ver inactivos" — no incluido en este parche).

## 5. Pasos de implementación (orden sugerido)

1. Agregar `ErrorUsuarioYaExiste` (si no está ya como constante, ya que el código ERR06 existe en el anexo pero puede no estar todavía materializado como constante) y `ErrorEmailYaRegistrado`/ERR27 en `BE/Errores/`.
2. Agregar a `IUsuarioRepositorio`: `Insertar`, `Modificar`, `BajaLogica`, `AsignarFamilia`, `Listar`, `ExisteNombreUsuario`, `ExisteEmail`. Implementarlos en `UsuarioRepositorio` (DAL).
3. Agregar a `IUsuarioService`: `AltaUsuario`, `ModificarUsuario`, `BajaUsuario`, `ObtenerUsuarioPorId`, `ListarUsuarios`.
4. Crear `SEC/Implementaciones/UsuarioService.Abm.cs` con la segunda mitad de la partial class, implementando los métodos nuevos con las validaciones de obligatoriedad/unicidad y la reutilización de `HashHelper`/`EmailHelper` para el alta.
5. Crear `Operativ.Web/Paginas/GestionUsuarios.aspx` + code-behind: autorización por perfil, grilla, panel de formulario con validadores, wiring a `FabricaSeguridad.Instancia.CrearUsuarioService()`.
6. Agregar el link "Usuarios" en `Navbar.ascx`, condicionado al perfil Administrador.
7. Actualizar el anexo de errores (`Carpeta_Tecnologia_STF_-Matias_Milio.docx`, punto 12.1) con ERR27 — pendiente, fuera del alcance de este documento de plan.
8. Compilar y probar manualmente: alta con usuario/email nuevos (éxito + email de bienvenida), alta repitiendo nombre de usuario (ERR06), alta repitiendo email (ERR27), modificación sin tocar el email propio (no debe marcar ERR27 contra sí mismo), modificación repitiendo el email de otro usuario (ERR27), baja lógica (el usuario deja de listarse y no puede loguearse más — confirmar que `ValidarCredenciales` ya contempla `Activo = false` como bloqueo o agregarlo si falta), acceso a `GestionUsuarios.aspx` con perfil distinto de Administrador (→ `NoAutorizado.aspx`) y sin sesión (→ `Login.aspx`).

## 6. Tabla de equivalencias (por si el repo real usa la otra convención)

| Este plan (según estándares vigentes) | Alternativa vista en fragmentos del contexto |
| --- | --- |
| `UsuarioService` | `UsuarioBLL` / clase equivalente |
| `OperativException` | `ExcepcionNegocio` |
| `ErroresHandler` (`BE/Errores/`) | `ManejadorErrores` (`Operativ.Comun/`) |
| `Principal.Master` | `SiteMaster` |
| `Operativ.SEC`/`Operativ.BE` | `Operativ.Comun` (en esa otra convención agrupa lo transversal) |

Si el repo real resulta estar en esta segunda convención, avisame y adapto el plan a esos nombres puntuales — el diseño (partial class, validaciones, campos, pasos) no cambia, solo los nombres de clase/namespace.

## 7. Definición de "terminado"

- `UsuarioService` compila como partial class en dos archivos, implementando `IUsuarioService` completa.
- Alta, Modificación y Baja lógica funcionan end-to-end desde `GestionUsuarios.aspx`.
- Nombre de usuario y email duplicados dan error distinto y específico (ERR06 / ERR27), mostrado vía `Notificaciones.ascx`.
- Todos los campos del formulario tienen `RequiredFieldValidator`; el email además `RegularExpressionValidator`; todo reforzado del lado servidor.
- La pantalla usa la Master Page y la hoja de estilos existentes, sin CSS ni frameworks nuevos.
- Solo el perfil Administrador puede entrar a `GestionUsuarios.aspx` (sesión + perfil validados en `Page_Load`).
- No se agregó ningún `DELETE` físico de usuarios.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.4 | Plan inicial: ABM de Usuarios. `UsuarioService` pasa a partial class (parte de seguridad ya existente + parte de ABM nueva); pantalla `GestionUsuarios.aspx` con validaciones de formato/obligatoriedad; validación de unicidad de nombre de usuario (ERR06) y de email (ERR27, nuevo). |
