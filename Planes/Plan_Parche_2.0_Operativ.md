# Plan: Gestión completa de Usuario, Familia y Patente — familia opcional, permisos individuales y visibilidad por patente — Operativ (2.0)

**Repo:** `matias-milio-dev/TFI.Operativ.WebApp`, base `main` (HEAD auditado: `1f7f32f1e5ae5dd612e1cad10bb8ddbcf8248430` — "no permite dar de baja al ultimo usuario activo de una familia", con 1.9 y 1.9.1 ya mergeados).
**Rama a crear:** `feature/parche-2.0-gestion-usuario-familia-patente`.
**No hacer merge ni push a `main`.** Dejar los commits listos en la rama local para que el dueño del repo los revise y los suba él mismo.

Este es el parche más grande hasta ahora — toca las 4 capas (BE/DAL/SEC/Web) y agrega 2 páginas nuevas. Se armó auditando el código real de `main` en su estado actual (no un plan anterior), incluyendo piezas que ya estaban construidas pero sin usar (ver sección 1.1). Recordatorio de estándares (`Estandares_Codigo_y_Estilo_Operativ.md`): sin comentarios, sin `var`, sin tuplas/record/LINQ (nada de `.Where`/`.Select`/lambdas — los `List<T>.Exists`/`.Find` tampoco se usan acá, se prefiere `foreach` explícito), `namespace X;` scoped, ifs con llaves, orden público→privado, repositorios/servicios solo vía fábricas, acceso a datos vía `AccesoDatos` parametrizado, errores vía `OperativException`/`TipoError`/`DefinicionError`, auditoría vía `IBitacoraService`.

---

## 0. Correcciones a implementar

1. Se puede crear un usuario **sin familia** (dejando el combo de familia sin seleccionar).
2. Nuevo botón **"Permisos"** en la fila de cada usuario de `GestionUsuarios`, al lado de Editar/Dar de baja, que lleva a una página nueva para asignar/retirar patentes individuales a ese usuario, con estilos consistentes con el resto del sitio.
   - 2.1 Las patentes disponibles ya están precargadas en `Scripts/CrearBaseDatos.sql`; se reemplaza la patente única y grosera `GestionarUsuarios` por una fina por cada CU de usuario (Consultar, Alta, Baja, Modificación, Desbloqueo, Bloqueo) más `AsignarPatente`/`RemoverPatente` (son CU-003-007/008 reales de tu propia carpeta de TFI).
3. Se usa el patrón Composite que ya estaba a medio construir (`ComponentePermiso`/`FamiliaCompuesto`/`UsuarioPatenteHoja`) para representar el árbol de permisos completo de un usuario.
4. Los controles que dependen de una patente puntual usan el mismo enfoque de template method que ya usa `PaginaSeguraBase.PerfilesPermitidos` para familia, pero aplicado a nivel control (mostrar/ocultar), no a nivel página.
5. El árbol de permisos combina family + individuales: un usuario puede tener una patente por su familia, la misma patente asignada individualmente, o ambas al mismo tiempo, y el sistema lo resuelve como una sola unión.

---

## 1. Diseño y decisiones

### 1.1 Lo que ya estaba construido en el repo y no se usaba

Auditando `main` aparecieron piezas ya armadas, presumiblemente dejadas listas a propósito para este parche:

- **Composite**: `ComponentePermiso` (abstracto, `Id`/`Nombre`/`ObtenerNombresPatentes()`), `FamiliaCompuesto` (composite: `Agregar`/`Quitar`/`ObtenerComponentes`/`ObtenerNombresPatentes` recursivo) y `UsuarioPatenteHoja` (hoja). Hoy `FamiliaService.ArmarArbolPermisos` arma un `FamiliaCompuesto` que representa la familia y le cuelga una `UsuarioPatenteHoja` por cada patente de esa familia — pero el árbol se guarda en sesión (`SesionHandler.GetArbolPermisos()`) y **nada lo lee todavía**.
- **Base de datos**: las tablas `Patente`, `UsuarioPatente`, `FamiliaPatente` y hasta `FamiliaFamilia` ya existen en `CrearBaseDatos.sql`, con FKs y con la integridad (DVH) ya registrada en `TablasVerificables`. **No hace falta ninguna migración de esquema en este parche** — solo cambia el dato semilla de `Patente`/`FamiliaPatente` y se agrega el código que lee/escribe `UsuarioPatente` (hoy esa tabla existe pero nunca se escribe).
- El catálogo de patentes hoy es grueso (una por módulo: `GestionarUsuarios`, `GestionarFamilias`, `GestionarClientes`, etc.), no una por caso de uso. Se reemplaza únicamente `GestionarUsuarios` por el set fino de usuario; el resto de las patentes de otros módulos no se toca (están para funcionalidad que todavía no existe, igual que antes).

### 1.2 Catálogo de patentes de usuario

Se reemplaza `GestionarUsuarios` por 8 patentes finas, nombradas igual que ya nombra el propio código a las mismas acciones en `TipoAccionBitacora` (`AltaUsuario`, `BajaUsuario`, `ModificacionUsuario`, `DesbloqueoUsuario`) para no introducir una segunda convención de nombres en el mismo proyecto, y dos nuevas para el CU-003-007/008 de tu carpeta (Asignar/Remover Patente):

| Patente (`Nombre`, sin espacios ni tildes — mismo estilo que `RepararBaseDatos`/`RealizarBackup` ya sembrados) | Habilita |
| --- | --- |
| `ConsultarUsuario` | Ver el listado de usuarios (sembrada, no se ata todavía a un control puntual — ver 1.8) |
| `AltaUsuario` | Botón "Nuevo usuario" y Guardar en modo alta |
| `BajaUsuario` | Botón "Dar de baja" en la grilla |
| `ModificacionUsuario` | Botón "Editar" en la grilla y Guardar en modo edición |
| `DesbloqueoUsuario` | Botón "Desbloquear usuario" |
| `BloqueoUsuario` | Botón "Bloquear usuario" (parche 1.9) |
| `AsignarPatente` | Tildar una patente nueva en la página de permisos individuales |
| `RemoverPatente` | Destildar una patente ya asignada individualmente |

Las 8 se le asignan a la familia `Administrador` en el seed (además de `GestionarFamilias`, que ya tenía y no se toca). WebMaster/Comercial/Cliente no se tocan.

### 1.3 Árbol de permisos combinado (Composite)

`FamiliaService.ArmarArbolPermisos` pasa de armar un único `FamiliaCompuesto` (la familia) a armar un `FamiliaCompuesto` **raíz** que representa al usuario, con:

- Un hijo `FamiliaCompuesto` con las patentes de la familia (solo si el usuario tiene familia).
- Una `UsuarioPatenteHoja` por cada patente asignada **individualmente** (tabla `UsuarioPatente`), colgada directo de la raíz.

`ObtenerNombresPatentes()` ya recorre recursivamente los hijos (heredado de cómo está escrito `FamiliaCompuesto` hoy), así que la unión familia+individual sale gratis sin tocar una línea de las clases Composite — solo cambia cómo `FamiliaService` arma el árbol. Si una patente está tanto en la familia como asignada individualmente, aparece dos veces en la lista de nombres; no es un bug — `TienePatente` solo hace un chequeo de pertenencia, así que la duplicación es inofensiva.

### 1.4 Usuarios sin familia: qué se rompe si no se arregla en cadena

`FamiliaService.GetPerfilDeUsuario` hoy **tira `ErrorUsuarioNoExiste` si el usuario tiene cero familias** (lo trata como proxy de "no existe", porque hasta ahora todo usuario tenía exactamente una). Si un usuario puede quedar sin familia, hay que:

1. Que `GetPerfilDeUsuario` devuelva `null` en vez de tirar excepción cuando no hay familia (la validación de existencia real ya la hace `ValidarCredenciales` antes, en el login).
2. `LoginNormalStrategy`/`ArmarArbolPermisos` tienen que tolerar perfil `null`.
3. `NavegacionHelper.ObtenerUrlHome(null)` no puede devolver `Login.aspx` (generaría un loop: usuario logueado → redirige a Login → Login ve sesión activa → intenta `perfil.Nombre` → `NullReferenceException`). Se agrega una página nueva `SinFamilia.aspx` para este caso puntual.
4. `Login.aspx.cs` tiene **dos** lugares que hacen `.Perfil.Nombre`/`.GetPerfil().Nombre` sin chequeo de null.
5. `NoAutorizado.aspx.cs` tiene el mismo problema (un usuario sin familia que intenta entrar a una página seria redirigido ahí por `ValidarAcceso`, y esa página también rompía).
6. `ResumenUsuario.ascx.cs` (el encabezado de todas las páginas seguras) hoy **oculta todo el control** si `perfil == null` — eso incluye el enlace de "Cerrar sesión", dejando a un usuario sin familia sin forma de salir. Se cambia para que muestre un texto genérico en vez del nombre de familia, pero mantenga visible "Cerrar sesión".

Con esto, un usuario sin familia puede loguearse, ve una página clara explicando que no tiene perfil asignado, y puede cerrar sesión con normalidad. Ninguna página protegida por familia lo deja entrar (sigue sin poder pasar `PerfilesPermitidos`), lo cual es correcto: crear el usuario sin familia es un estado transitorio hasta que un Administrador le asigne una (ver 1.8, este parche también permite reasignar la familia desde Modificación, no solo en el alta).

### 1.5 Visibilidad por patente: mismo enfoque de template method que Familia

El template method que ya usa Familia es `PaginaSeguraBase.PerfilesPermitidos`: una propiedad abstracta que cada página sobrescribe, consumida por el método plantilla `ValidarAcceso()` (no overridable), llamado desde `OnInit`. Se agrega el equivalente para controles:

```csharp
protected override void OnPreRender(System.EventArgs e)
{
    base.OnPreRender(e);
    AbrirCambioClaveSiEsProvisoria();
    AplicarVisibilidadPorPatentes();
}

protected virtual void AplicarVisibilidadPorPatentes()
{
}
```

`AplicarVisibilidadPorPatentes()` es virtual con cuerpo vacío en la base (no rompe ninguna página existente) y `GestionUsuarios` lo sobrescribe para ocultar sus botones de acción según patente. Se ejecuta en `OnPreRender`, después de que cualquier handler de click ya decidió el modo del formulario (alta/edición/desbloqueo) — así que las decisiones de patente **restringen** (nunca amplían) lo que el modo del formulario ya decidió mostrar.

Para los controles **dentro de las filas de la grilla** (Editar/Baja/Permisos) no aplica este mismo hook: un `GridView` decide la visibilidad de sus controles por fila en `RowDataBound`, que es el mecanismo nativo de Web Forms para esto — se usa `AutorizacionHandler.TienePatente(...)` ahí también, así que el chequeo de fondo es el mismo, solo cambia el evento que lo dispara.

### 1.6 Defensa en profundidad: ocultar un botón no es autorizar

Ocultar un control en el cliente no impide que alguien dispare el postback igual (inspeccionando el HTML o repitiendo la request). Por eso, además de ocultar, cada handler que modifica datos (`btnGuardar_Click`, `DarDeBaja`, `btnDesbloquear_Click`, `btnBloquear_Click`, y el `Page_Load` de la página de permisos) vuelve a validar del lado servidor con `AutorizacionHandler.TienePatente(...)` antes de ejecutar la acción, devolviendo un error nuevo (`ErrorSinPermiso`, `ERR20`) si no corresponde. Esto es el mismo criterio que ya usa la página completa (`ValidarAcceso` redirige si no sos del perfil correcto) pero a nivel acción individual.

### 1.7 Página nueva de permisos individuales

`PermisosUsuario.aspx?idUsuario={id}`: una `CheckBoxList` con **todas** las patentes del catálogo, tildada cada una si el usuario la tiene asignada **individualmente** (tabla `UsuarioPatente`); si además la tiene por su familia, el texto del ítem lo aclara entre paréntesis, pero el tilde solo representa la asignación individual — tildar una patente que ya viene por familia es válido (queda asignada por los dos caminos a la vez, tal como pide el punto 5).

Se usa `CheckBoxList` (no un `Repeater` con checkboxes sueltos) porque `CheckBoxList.Items` persiste su estado de tildado en ViewState automáticamente entre posts — con un `Repeater`, el `DataItem` de cada fila se pierde en el postback y hay que pelear con eso a mano; para una lista chata de patentes, `CheckBoxList` es la herramienta correcta y evita ese problema de raíz.

Cada ítem se deshabilita (`Enabled = false`) si el Administrador logueado no tiene el permiso necesario para cambiar ese estado puntual: sin `AsignarPatente` no puede tildar una que no tenía, sin `RemoverPatente` no puede destildar una que sí tenía — aplicando el punto 4 dentro de la página nueva también, no solo en la grilla de `GestionUsuarios`.

### 1.8 Qué NO se toca (fuera de alcance)

- `ConsultarUsuario` se siembra en la base pero no se ata a un control puntual en este parche — la grilla y la búsqueda de `GestionUsuarios` ya están protegidas a nivel página por familia (`PerfilesPermitidos`), y ninguno de los ejemplos concretos que se pidieron ocultar es la grilla en sí. Queda sembrada para cuando haga falta.
- Este parche **sí** extiende `ModificarUsuario` para poder reasignar o quitar la familia de un usuario ya existente (no solo en el alta) — sin esto, un usuario creado sin familia quedaría huérfano para siempre, lo cual contradice el espíritu de "gestión completa". No se agregó ningún control nuevo para esto: el combo de familia ya se mostraba en el formulario de edición, solo faltaba que `btnGuardar_Click` lo leyera también en esa rama.
- No se gestiona qué patentes tiene asignada una **familia** (eso son los CU-003-009/010 Alta/Baja de Familia y la asignación de patentes a familias — un módulo de administración de familias que no existe todavía en la UI). Este parche es específicamente sobre patentes **individuales** de un usuario.
- WebMaster no gana acceso a `GestionUsuarios` aunque la carpeta de TFI lo liste como actor de Desbloquear/Asignar/Remover Patente — `PerfilesPermitidos` de esa página ya excluía a WebMaster desde antes de este parche y ampliarlo es un cambio de alcance de página, no de patente, que no fue pedido.

---

## 2. Base de datos

### 2.1 `Scripts/CrearBaseDatos.sql` — reemplazar el bloque de seed de `Patente` y `FamiliaPatente`

No hay cambios de esquema (las tablas `Patente`/`UsuarioPatente`/`FamiliaPatente` ya existen tal cual se necesitan). Reemplazar estos dos bloques `INSERT` (los que arrancan en `INSERT INTO Patente` y `INSERT INTO FamiliaPatente`) por:

```sql
INSERT INTO Patente (Nombre, Descripcion) VALUES
    ('RepararBaseDatos', 'Ejecutar el modulo de reparacion de base de datos'),
    ('RealizarBackup', 'Realizar backup y restore de la base de datos'),
    ('ConsultarUsuario', 'Consultar el listado de usuarios'),
    ('AltaUsuario', 'Dar de alta usuarios nuevos'),
    ('BajaUsuario', 'Dar de baja usuarios existentes'),
    ('ModificacionUsuario', 'Modificar los datos de un usuario'),
    ('DesbloqueoUsuario', 'Desbloquear usuarios bloqueados'),
    ('BloqueoUsuario', 'Bloquear usuarios manualmente'),
    ('AsignarPatente', 'Asignar patentes individuales a un usuario'),
    ('RemoverPatente', 'Quitar patentes individuales de un usuario'),
    ('GestionarFamilias', 'Alta, baja y modificacion de familias y patentes'),
    ('GestionarClientes', 'Alta, baja y modificacion de clientes'),
    ('GestionarCatalogo', 'Administrar el catalogo de paquetes'),
    ('GestionarSuscripciones', 'Contratar y administrar suscripciones'),
    ('ConsultarFacturas', 'Consultar facturas emitidas'),
    ('ReportarIncidentes', 'Reportar incidentes sobre activos');
GO

INSERT INTO FamiliaPatente (IdFamilia, IdPatente)
SELECT F.IdFamilia, P.IdPatente
FROM Familia F, Patente P
WHERE (F.Nombre = 'WebMaster' AND P.Nombre IN ('RepararBaseDatos', 'RealizarBackup'))
   OR (F.Nombre = 'Administrador' AND P.Nombre IN ('ConsultarUsuario', 'AltaUsuario', 'BajaUsuario', 'ModificacionUsuario', 'DesbloqueoUsuario', 'BloqueoUsuario', 'AsignarPatente', 'RemoverPatente', 'GestionarFamilias'))
   OR (F.Nombre = 'Comercial' AND P.Nombre IN ('GestionarClientes', 'GestionarCatalogo'))
   OR (F.Nombre = 'Cliente' AND P.Nombre IN ('GestionarSuscripciones', 'ConsultarFacturas', 'ReportarIncidentes'));
GO
```

Nada más del script cambia (la tabla `UsuarioPatente` ya existe vacía y se va a ir llenando desde la UI, no desde el seed).

---

## 3. Capa BE

### 3.1 `Operativ.BE/Enums/TipoError.cs` — contenido completo

```csharp
namespace Operativ.BE.Enums;
public enum TipoError
{
    ErrorUsuarioNoExiste,
    ErrorContrasenaIncorrecta,
    ErrorUsuarioBloqueado,
    ErrorConexionBaseDatos,
    ErrorSesionExpirada,
    ErrorEnvioEmail,
    ErrorUsuarioYaExiste,
    ErrorEmailYaRegistrado,
    ErrorContrasenaActualIncorrecta,
    ErrorClaveNoCumpleComplejidad,
    ErrorIntegridadCorrupta,
    ErrorCredencialesEmergenciaInvalidas,
    ErrorArchivoEmergenciaNoDisponible,
    ErrorUltimoUsuarioDeFamilia,
    ErrorPatenteYaAsignada,
    ErrorPatenteNoAsignada,
    ErrorSinPermiso,
    FalloNoManejadoGenerico
}
```

### 3.2 `Operativ.BE/Modelos/DefinicionError.cs` — contenido completo

Se agregan `ERR18`/`ERR19`/`ERR20`. De paso se corrige un bug preexistente que no era parte de ningún pedido: `ObtenerTodas()` no incluía `FalloNoManejadoGenerico`, así que `ObtenerPorTipo(TipoError.FalloNoManejadoGenerico)` caía en el `Desconocido` (`ERR00`) en vez de en `ERR16`. Se arregla de paso porque ya se está tocando esta misma lista.

```csharp
using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Modelos;

public class DefinicionError
{
    public TipoError Tipo { get; }

    public string Codigo { get; }

    public string ClaveRecurso { get; }

    public DefinicionError(TipoError tipo, string codigo, string claveRecurso)
    {
        Tipo = tipo;
        Codigo = codigo;
        ClaveRecurso = claveRecurso;
    }

    public static readonly DefinicionError ErrorUsuarioNoExiste =
        new(TipoError.ErrorUsuarioNoExiste, "ERR01", "MensajeErrorUsuarioNoExiste");
    public static readonly DefinicionError ErrorContrasenaIncorrecta =
        new(TipoError.ErrorContrasenaIncorrecta, "ERR02", "MensajeErrorContrasenaIncorrecta");
    public static readonly DefinicionError ErrorUsuarioBloqueado =
        new(TipoError.ErrorUsuarioBloqueado, "ERR03", "MensajeErrorUsuarioBloqueado");
    public static readonly DefinicionError ErrorIntegridadCorrupta =
        new(TipoError.ErrorIntegridadCorrupta, "ERR04", "MensajeErrorIntegridadCorrupta");
    public static readonly DefinicionError ErrorConexionBaseDatos =
        new(TipoError.ErrorConexionBaseDatos, "ERR05", "MensajeErrorConexionBaseDatos");
    public static readonly DefinicionError ErrorEnvioEmail =
        new(TipoError.ErrorEnvioEmail, "ERR06", "MensajeErrorEnvioEmail");
    public static readonly DefinicionError ErrorCredencialesEmergenciaInvalidas =
        new(TipoError.ErrorCredencialesEmergenciaInvalidas, "ERR07", "MensajeErrorCredencialesEmergenciaInvalidas");
    public static readonly DefinicionError ErrorArchivoEmergenciaNoDisponible =
        new(TipoError.ErrorArchivoEmergenciaNoDisponible, "ERR08", "MensajeErrorArchivoEmergenciaNoDisponible");
    public static readonly DefinicionError ErrorSesionExpirada =
        new(TipoError.ErrorSesionExpirada, "ERR11", "MensajeErrorSesionExpirada");
    public static readonly DefinicionError ErrorUsuarioYaExiste =
        new(TipoError.ErrorUsuarioYaExiste, "ERR12", "MensajeErrorUsuarioYaExiste");
    public static readonly DefinicionError ErrorEmailYaRegistrado =
        new(TipoError.ErrorEmailYaRegistrado, "ERR13", "MensajeErrorEmailYaRegistrado");
    public static readonly DefinicionError ErrorContrasenaActualIncorrecta =
        new(TipoError.ErrorContrasenaActualIncorrecta, "ERR14", "MensajeErrorContrasenaActualIncorrecta");
    public static readonly DefinicionError ErrorClaveNoCumpleComplejidad =
        new(TipoError.ErrorClaveNoCumpleComplejidad, "ERR15", "MensajeErrorClaveNoCumpleComplejidad");
    public static readonly DefinicionError FalloNoManejadoGenerico =
        new(TipoError.FalloNoManejadoGenerico, "ERR16", "MensajeErrorFalloNoManejadoGenerico");
    public static readonly DefinicionError ErrorUltimoUsuarioDeFamilia =
        new(TipoError.ErrorUltimoUsuarioDeFamilia, "ERR17", "MensajeErrorUltimoUsuarioDeFamilia");
    public static readonly DefinicionError ErrorPatenteYaAsignada =
        new(TipoError.ErrorPatenteYaAsignada, "ERR18", "MensajeErrorPatenteYaAsignada");
    public static readonly DefinicionError ErrorPatenteNoAsignada =
        new(TipoError.ErrorPatenteNoAsignada, "ERR19", "MensajeErrorPatenteNoAsignada");
    public static readonly DefinicionError ErrorSinPermiso =
        new(TipoError.ErrorSinPermiso, "ERR20", "MensajeErrorSinPermiso");
    private static readonly DefinicionError Desconocido =
        new(TipoError.ErrorUsuarioNoExiste, "ERR00", "MensajeErrorDesconocido");

    public static List<DefinicionError> ObtenerTodas()
    {
        return new List<DefinicionError>
        {
            ErrorUsuarioNoExiste,
            ErrorContrasenaIncorrecta,
            ErrorUsuarioBloqueado,
            ErrorIntegridadCorrupta,
            ErrorConexionBaseDatos,
            ErrorEnvioEmail,
            ErrorCredencialesEmergenciaInvalidas,
            ErrorArchivoEmergenciaNoDisponible,
            ErrorSesionExpirada,
            ErrorUsuarioYaExiste,
            ErrorEmailYaRegistrado,
            ErrorContrasenaActualIncorrecta,
            ErrorClaveNoCumpleComplejidad,
            ErrorUltimoUsuarioDeFamilia,
            ErrorPatenteYaAsignada,
            ErrorPatenteNoAsignada,
            ErrorSinPermiso,
            FalloNoManejadoGenerico
        };
    }

    public static DefinicionError ObtenerPorTipo(TipoError tipo)
    {
        foreach (DefinicionError definicion in ObtenerTodas())
        {
            if (definicion.Tipo == tipo)
            {
                return definicion;
            }
        }

        return Desconocido;
    }
}
```

### 3.3 `Operativ.BE/Enums/TipoAccionBitacora.cs` — contenido completo

```csharp
namespace Operativ.BE.Enums;
public enum TipoAccionBitacora
{
    LoginExitoso,
    LoginBloqueado,
    RecuperacionContrasena,
    CierreSesion,
    IntentoLoginFallido,
    AltaUsuario,
    BajaUsuario,
    ModificacionUsuario,
    DesbloqueoUsuario,
    CambioClave,
    ReparacionEmergenciaBaseDatos,
    IntegridadCorrupta,
    BloqueoManualUsuario,
    AsignacionPatente,
    RemocionPatente
}
```

### 3.4 `Operativ.BE/Modelos/AccionBitacora.cs` — contenido completo

```csharp
using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Modelos;

public class AccionBitacora
{
    public TipoAccionBitacora Tipo { get; }

    public CriticidadBitacora Criticidad { get; }

    public string Descripcion { get; }

    public AccionBitacora(TipoAccionBitacora tipo, CriticidadBitacora criticidad, string descripcion)
    {
        Tipo = tipo;
        Criticidad = criticidad;
        Descripcion = descripcion;
    }

    public static readonly AccionBitacora LoginExitoso =
        new(TipoAccionBitacora.LoginExitoso, CriticidadBitacora.Informativo, "Inicio de sesión exitoso");
    public static readonly AccionBitacora LoginBloqueado =
        new(TipoAccionBitacora.LoginBloqueado, CriticidadBitacora.Critico, "Usuario bloqueado tras {0} intentos fallidos");
    public static readonly AccionBitacora RecuperacionContrasena =
        new(TipoAccionBitacora.RecuperacionContrasena, CriticidadBitacora.Advertencia, "Contraseña restablecida por recuperación");
    public static readonly AccionBitacora CierreSesion =
        new(TipoAccionBitacora.CierreSesion, CriticidadBitacora.Informativo, "Cierre de sesión");
    public static readonly AccionBitacora IntentoLoginFallido =
        new(TipoAccionBitacora.IntentoLoginFallido, CriticidadBitacora.Critico, "Login con credenciales invalidas");
    public static readonly AccionBitacora AltaUsuario =
        new(TipoAccionBitacora.AltaUsuario, CriticidadBitacora.Informativo, "Alta de usuario");
    public static readonly AccionBitacora BajaUsuario =
        new(TipoAccionBitacora.BajaUsuario, CriticidadBitacora.Advertencia, "Baja lógica de usuario");
    public static readonly AccionBitacora ModificacionUsuario =
        new(TipoAccionBitacora.ModificacionUsuario, CriticidadBitacora.Informativo, "Modificación de datos de usuario");
    public static readonly AccionBitacora DesbloqueoUsuario =
        new(TipoAccionBitacora.DesbloqueoUsuario, CriticidadBitacora.Advertencia, "Desbloqueo de usuario");
    public static readonly AccionBitacora CambioClave =
        new(TipoAccionBitacora.CambioClave, CriticidadBitacora.Informativo, "Cambio de contraseña por autogestión");
    public static readonly AccionBitacora ReparacionEmergenciaBaseDatos =
        new(TipoAccionBitacora.ReparacionEmergenciaBaseDatos, CriticidadBitacora.Critico, "Base de datos reparada mediante acceso de emergencia del Web Master");
    public static readonly AccionBitacora IntegridadCorrupta =
        new(TipoAccionBitacora.IntegridadCorrupta, CriticidadBitacora.Critico, "Se detectó una alteración en la integridad de los datos del sistema");
    public static readonly AccionBitacora BloqueoManualUsuario =
        new(TipoAccionBitacora.BloqueoManualUsuario, CriticidadBitacora.Advertencia, "Bloqueo manual de usuario por administrador");
    public static readonly AccionBitacora AsignacionPatente =
        new(TipoAccionBitacora.AsignacionPatente, CriticidadBitacora.Advertencia, "Asignación de patente individual a un usuario");
    public static readonly AccionBitacora RemocionPatente =
        new(TipoAccionBitacora.RemocionPatente, CriticidadBitacora.Advertencia, "Remoción de patente individual de un usuario");

    public static List<AccionBitacora> ObtenerTodas()
    {
        return new List<AccionBitacora>
        {
            LoginExitoso,
            LoginBloqueado,
            RecuperacionContrasena,
            CierreSesion,
            IntentoLoginFallido,
            AltaUsuario,
            BajaUsuario,
            ModificacionUsuario,
            DesbloqueoUsuario,
            CambioClave,
            ReparacionEmergenciaBaseDatos,
            IntegridadCorrupta,
            BloqueoManualUsuario,
            AsignacionPatente,
            RemocionPatente
        };
    }

    public static AccionBitacora ObtenerPorTipo(TipoAccionBitacora tipo)
    {
        foreach (AccionBitacora accion in ObtenerTodas())
        {
            if (accion.Tipo == tipo)
            {
                return accion;
            }
        }

        return null;
    }
}
```

Las clases Composite (`ComponentePermiso.cs`, `FamiliaCompuesto.cs`, `UsuarioPatenteHoja.cs`) **no se tocan** — se reutilizan tal cual están.

---

## 4. Capa DAL

### 4.1 `Operativ.DAL/Contratos/IPatenteRepositorio.cs` — contenido completo (archivo nuevo)

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IPatenteRepositorio
{
    List<Patente> ListarTodas();

    List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario);

    void AsignarPatenteAUsuario(int idUsuario, int idPatente);

    void QuitarPatenteDeUsuario(int idUsuario, int idPatente);

    bool ExistePatenteIndividual(int idUsuario, int idPatente);
}
```

### 4.2 `Operativ.DAL/Implementaciones/PatenteRepositorio.cs` — contenido completo (archivo nuevo)

`ToListaPatentes()` ya existe (lo usa hoy `FamiliaRepositorio.GetPatentesDeFamilia`), así que no hace falta un convertidor nuevo. Tras el `DELETE` de `QuitarPatenteDeUsuario` se recalcula el DVV de `UsuarioPatente` a mano (`IntegridadHelper.ActualizarDvvTabla`, método `internal` visible dentro de `Operativ.DAL`): a diferencia de un `UPDATE`, un `DELETE` cambia la cantidad de filas, así que si no se recalcula el DVV queda desactualizado y una verificación de integridad futura marcaría una alteración falsa.

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Conexion;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class PatenteRepositorio : IPatenteRepositorio
{
    private readonly AccesoDatos accesoDatos;

    public PatenteRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public List<Patente> ListarTodas()
    {
        string consulta = "SELECT IdPatente, Nombre, Descripcion FROM Patente ORDER BY Nombre";

        DataTable tabla = accesoDatos.EjecutarReader(consulta, null);

        return tabla.ToListaPatentes();
    }

    public List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario)
    {
        string consulta = "SELECT P.IdPatente, P.Nombre, P.Descripcion "
            + "FROM Patente P "
            + "INNER JOIN UsuarioPatente UP ON UP.IdPatente = P.IdPatente "
            + "WHERE UP.IdUsuario = @IdUsuario "
            + "ORDER BY P.Nombre";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaPatentes();
    }

    public void AsignarPatenteAUsuario(int idUsuario, int idPatente)
    {
        string consulta = "INSERT INTO UsuarioPatente (IdUsuario, IdPatente) VALUES (@IdUsuario, @IdPatente)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);

        List<SqlParameter> clavesFila = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        IntegridadHelper.ActualizarIntegridadClaveCompuesta("UsuarioPatente", clavesFila);
    }

    public void QuitarPatenteDeUsuario(int idUsuario, int idPatente)
    {
        string consulta = "DELETE FROM UsuarioPatente WHERE IdUsuario = @IdUsuario AND IdPatente = @IdPatente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        IntegridadHelper.ActualizarDvvTabla("UsuarioPatente");
    }

    public bool ExistePatenteIndividual(int idUsuario, int idPatente)
    {
        string consulta = "SELECT COUNT(*) FROM UsuarioPatente WHERE IdUsuario = @IdUsuario AND IdPatente = @IdPatente";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdPatente", idPatente)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }
}
```

### 4.3 `Operativ.DAL/Contratos/IUsuarioRepositorio.cs` — contenido completo

Se agrega `QuitarFamilias`, simétrico a `AsignarFamilia`, para poder reasignar la familia de un usuario existente desde `ModificarUsuario` (ver sección 5.4).

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Contratos;
public interface IUsuarioRepositorio
{
    Usuario GetPorNombreUsuario(string nombreUsuario);

    Usuario GetPorId(int idUsuario);

    void ActualizarIntentosFallidos(int idUsuario, int intentosFallidos, bool bloqueado);

    void ActualizarContrasena(int idUsuario, string contrasena, string salt);

    void ActualizarContrasenaProvisoria(int idUsuario, string contrasena, string salt);

    void ResetearIntentosFallidos(int idUsuario);

    void Desbloquear(int idUsuario);

    void Bloquear(int idUsuario);

    int Insertar(Usuario usuario);

    void Modificar(Usuario usuario);

    void BajaLogica(int idUsuario);

    void AsignarFamilia(int idUsuario, int idFamilia);

    void QuitarFamilias(int idUsuario);

    List<Usuario> Listar(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);

    int ContarUsuarios(string filtro, int? idFamilia);

    bool ExisteNombreUsuario(string nombreUsuario, int? idUsuarioExcluir);

    bool ExisteEmail(string correoElectronico, int? idUsuarioExcluir);

    bool ExisteOtroUsuarioActivoEnFamilia(int idFamilia, int idUsuarioExcluir);
}
```

### 4.4 `Operativ.DAL/Implementaciones/UsuarioRepositorio.cs` — agregar `QuitarFamilias`

Agregar este método público, junto a `AsignarFamilia` (antes de `ActualizarDVH`), en el archivo que ya existe (el resto del archivo no cambia respecto al parche 1.9.1):

```csharp
    public void QuitarFamilias(int idUsuario)
    {
        string consulta = "DELETE FROM UsuarioFamilia WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        IntegridadHelper.ActualizarDvvTabla("UsuarioFamilia");
    }
```

Mismo motivo que en `PatenteRepositorio`: es un `DELETE`, así que hay que recalcular el DVV de `UsuarioFamilia` a mano.

### 4.5 `Operativ.DAL/Fabricas/FabricaRepositorio.cs` — contenido completo

```csharp
using Operativ.DAL.Contratos;
using Operativ.DAL.Implementaciones;

namespace Operativ.DAL.Fabricas;
public class FabricaRepositorio
{
    public IUsuarioRepositorio CrearUsuarioRepositorio()
    {
        return new UsuarioRepositorio();
    }

    public IFamiliaRepositorio CrearFamiliaRepositorio()
    {
        return new FamiliaRepositorio();
    }

    public IPatenteRepositorio CrearPatenteRepositorio()
    {
        return new PatenteRepositorio();
    }

    public IBitacoraRepositorio CrearBitacoraRepositorio()
    {
        return new BitacoraRepositorio();
    }

    public IIntegridadRepositorio CrearIntegridadRepositorio()
    {
        return new IntegridadRepositorio();
    }
}
```

---

## 5. Capa SEC

### 5.1 `Operativ.SEC/Contratos/IFamiliaService.cs` — contenido completo

Se agrega `GetPatentesDeFamilia`, para que la página de permisos individuales pueda mostrar qué patentes vienen "de regalo" por la familia sin que la capa Web tenga que hablarle directo al DAL.

```csharp
using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IFamiliaService
{
    Familia GetPerfilDeUsuario(int idUsuario);

    FamiliaCompuesto ArmarArbolPermisos(int idUsuario);

    List<Familia> ListarFamilias();

    List<Patente> GetPatentesDeFamilia(int idFamilia);
}
```

### 5.2 `Operativ.SEC/Implementaciones/FamiliaService.cs` — contenido completo

`GetPerfilDeUsuario` devuelve `null` (no tira excepción) cuando el usuario no tiene familia. `ArmarArbolPermisos` arma la raíz que representa al usuario, con la familia (si tiene) como rama hija y las patentes individuales colgadas directo de la raíz.

```csharp
using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones;
public class FamiliaService : IFamiliaService
{
    private readonly IFamiliaRepositorio familiaRepositorio;
    private readonly IPatenteRepositorio patenteRepositorio;

    public FamiliaService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        familiaRepositorio = fabricaRepositorio.CrearFamiliaRepositorio();
        patenteRepositorio = fabricaRepositorio.CrearPatenteRepositorio();
    }

    public Familia GetPerfilDeUsuario(int idUsuario)
    {
        List<Familia> familias = familiaRepositorio.GetFamiliasDeUsuario(idUsuario);

        if (familias.Count == 0)
        {
            return null;
        }

        return familias[0];
    }

    public FamiliaCompuesto ArmarArbolPermisos(int idUsuario)
    {
        FamiliaCompuesto raiz = new FamiliaCompuesto
        {
            Id = idUsuario,
            Nombre = "PermisosDeUsuario"
        };

        Familia perfil = GetPerfilDeUsuario(idUsuario);

        if (perfil != null)
        {
            raiz.Agregar(ArmarRamaFamilia(perfil));
        }

        List<Patente> patentesIndividuales = patenteRepositorio.GetPatentesIndividualesDeUsuario(idUsuario);

        foreach (Patente patente in patentesIndividuales)
        {
            raiz.Agregar(new UsuarioPatenteHoja { Id = patente.IdPatente, Nombre = patente.Nombre });
        }

        return raiz;
    }

    public List<Familia> ListarFamilias()
    {
        return familiaRepositorio.ListarTodas();
    }

    public List<Patente> GetPatentesDeFamilia(int idFamilia)
    {
        return familiaRepositorio.GetPatentesDeFamilia(idFamilia);
    }

    private FamiliaCompuesto ArmarRamaFamilia(Familia perfil)
    {
        FamiliaCompuesto ramaFamilia = new FamiliaCompuesto
        {
            Id = perfil.IdFamilia,
            Nombre = perfil.Nombre
        };

        List<Patente> patentesFamilia = familiaRepositorio.GetPatentesDeFamilia(perfil.IdFamilia);

        foreach (Patente patente in patentesFamilia)
        {
            ramaFamilia.Agregar(new UsuarioPatenteHoja { Id = patente.IdPatente, Nombre = patente.Nombre });
        }

        return ramaFamilia;
    }
}
```

### 5.3 `Operativ.SEC/Contratos/IUsuarioService.cs` — contenido completo

`AltaUsuario`/`ModificarUsuario` pasan a recibir la familia como `int?`.

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IUsuarioService
{
    void RecuperarContrasena(string nombreUsuario);

    void CambiarClave(int idUsuario, string claveActual, string claveNueva);

    int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int? idFamilia);

    void ModificarUsuario(Usuario usuario, int? idFamilia);

    void BajaUsuario(int idUsuario);

    void DesbloquearUsuario(int idUsuario);

    void BloquearUsuario(int idUsuario);

    Usuario ObtenerUsuarioPorId(int idUsuario);

    List<Usuario> ListarUsuarios(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);

    int ContarUsuarios(string filtro, int? idFamilia);
}
```

### 5.4 `Operativ.SEC/Implementaciones/UsuarioService.Abm.cs` — contenido completo

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones;
public partial class UsuarioService
{
    public int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int? idFamilia)
    {
        ValidarUnicidad(nombreUsuario, correoElectronico, null);

        string contrasenaTemporal = ClaveHelper.GenerarContrasenaTemporal();
        string salt = HashHelper.GenerarSalt();
        string hash = HashHelper.GenerarHash(contrasenaTemporal, salt);

        EmailHelper.EnviarBienvenida(correoElectronico, nombreUsuario, contrasenaTemporal);

        Usuario usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Email = correoElectronico,
            Contrasena = hash,
            Salt = salt
        };

        int idUsuario = usuarioRepositorio.Insertar(usuario);

        if (idFamilia.HasValue)
        {
            usuarioRepositorio.AsignarFamilia(idUsuario, idFamilia.Value);
        }

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.AltaUsuario);

        return idUsuario;
    }

    public void ModificarUsuario(Usuario usuario, int? idFamilia)
    {
        ValidarUnicidad(usuario.NombreUsuario, usuario.Email, usuario.IdUsuario);

        usuarioRepositorio.Modificar(usuario);

        usuarioRepositorio.QuitarFamilias(usuario.IdUsuario);

        if (idFamilia.HasValue)
        {
            usuarioRepositorio.AsignarFamilia(usuario.IdUsuario, idFamilia.Value);
        }

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.ModificacionUsuario);
    }

    public void BajaUsuario(int idUsuario)
    {
        ValidarNoEsUltimoUsuarioDeFamilia(idUsuario);

        usuarioRepositorio.BajaLogica(idUsuario);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.BajaUsuario);
    }

    public Usuario ObtenerUsuarioPorId(int idUsuario)
    {
        Usuario usuario = usuarioRepositorio.GetPorId(idUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);

        usuario.Familias = familiaRepositorio.GetFamiliasDeUsuario(idUsuario);

        return usuario;
    }

    public List<Usuario> ListarUsuarios(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina)
    {
        return usuarioRepositorio.Listar(filtro, idFamilia, numeroPagina, tamanioPagina);
    }

    public int ContarUsuarios(string filtro, int? idFamilia)
    {
        return usuarioRepositorio.ContarUsuarios(filtro, idFamilia);
    }

    private void ValidarUnicidad(string nombreUsuario, string correoElectronico, int? idUsuarioExcluir)
    {
        if (usuarioRepositorio.ExisteNombreUsuario(nombreUsuario, idUsuarioExcluir))
        {
            throw new OperativException(TipoError.ErrorUsuarioYaExiste);
        }

        if (usuarioRepositorio.ExisteEmail(correoElectronico, idUsuarioExcluir))
        {
            throw new OperativException(TipoError.ErrorEmailYaRegistrado);
        }
    }

    private void ValidarNoEsUltimoUsuarioDeFamilia(int idUsuario)
    {
        List<Familia> familias = familiaRepositorio.GetFamiliasDeUsuario(idUsuario);

        foreach (Familia familia in familias)
        {
            if (!usuarioRepositorio.ExisteOtroUsuarioActivoEnFamilia(familia.IdFamilia, idUsuario))
            {
                throw new OperativException(TipoError.ErrorUltimoUsuarioDeFamilia, new string[] { familia.Nombre });
            }
        }
    }
}
```

### 5.5 `Operativ.SEC/Contratos/IPatenteService.cs` — contenido completo (archivo nuevo)

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IPatenteService
{
    List<Patente> ListarTodas();

    List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario);

    void AsignarPatente(int idUsuario, int idPatente);

    void QuitarPatente(int idUsuario, int idPatente);
}
```

### 5.6 `Operativ.SEC/Implementaciones/PatenteService.cs` — contenido completo (archivo nuevo)

Reglas de negocio tal como las narra CU-003-007/008 de tu carpeta: no asignar una patente ya asignada, no quitar una que no está asignada.

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.SEC.Implementaciones;
public class PatenteService : IPatenteService
{
    private readonly IPatenteRepositorio patenteRepositorio;
    private readonly IUsuarioRepositorio usuarioRepositorio;
    private readonly IBitacoraService bitacoraService;

    public PatenteService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        patenteRepositorio = fabricaRepositorio.CrearPatenteRepositorio();
        usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<Patente> ListarTodas()
    {
        return patenteRepositorio.ListarTodas();
    }

    public List<Patente> GetPatentesIndividualesDeUsuario(int idUsuario)
    {
        return patenteRepositorio.GetPatentesIndividualesDeUsuario(idUsuario);
    }

    public void AsignarPatente(int idUsuario, int idPatente)
    {
        ValidarUsuarioExistente(idUsuario);

        if (patenteRepositorio.ExistePatenteIndividual(idUsuario, idPatente))
        {
            throw new OperativException(TipoError.ErrorPatenteYaAsignada);
        }

        patenteRepositorio.AsignarPatenteAUsuario(idUsuario, idPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.AsignacionPatente);
    }

    public void QuitarPatente(int idUsuario, int idPatente)
    {
        ValidarUsuarioExistente(idUsuario);

        if (!patenteRepositorio.ExistePatenteIndividual(idUsuario, idPatente))
        {
            throw new OperativException(TipoError.ErrorPatenteNoAsignada);
        }

        patenteRepositorio.QuitarPatenteDeUsuario(idUsuario, idPatente);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.RemocionPatente);
    }

    private void ValidarUsuarioExistente(int idUsuario)
    {
        if (usuarioRepositorio.GetPorId(idUsuario) == null)
        {
            throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        }
    }
}
```

### 5.7 `Operativ.SEC/Handlers/AutorizacionHandler.cs` — contenido completo

Se agrega `TienePatente`, mismo criterio que `EsAlgunPerfil` pero contra el árbol de permisos en sesión. `List<string>.Contains` es un método propio de `List<T>` (no una extensión de LINQ), así que no viola "sin LINQ".

```csharp
using System;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos.Composite;

namespace Operativ.SEC.Handlers;
public class AutorizacionHandler
{
    private readonly SesionHandler sesionHandler;
    public AutorizacionHandler()
    {
        sesionHandler = new SesionHandler();
    }

    public bool EsAlgunPerfil(string[] nombresFamilia)
    {
        Familia perfil = sesionHandler.GetPerfil();

        if (perfil == null)
        {
            return false;
        }

        foreach (string nombreFamilia in nombresFamilia)
        {
            if (string.Equals(perfil.Nombre, nombreFamilia, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool TienePatente(string nombrePatente)
    {
        FamiliaCompuesto arbolPermisos = sesionHandler.GetArbolPermisos();

        if (arbolPermisos == null)
        {
            return false;
        }

        return arbolPermisos.ObtenerNombresPatentes().Contains(nombrePatente);
    }

    public string GetNombrePerfil()
    {
        Familia perfil = sesionHandler.GetPerfil();

        if (perfil == null)
        {
            return null;
        }

        return perfil.Nombre;
    }
}
```

### 5.8 `Operativ.SEC/Fabricas/FabricaSeguridad.cs` — contenido completo

```csharp
using Operativ.SEC.Contratos;
using Operativ.SEC.Implementaciones;
using Operativ.SEC.Implementaciones.Estrategias;

namespace Operativ.SEC.Fabricas;
public class FabricaSeguridad
{
    public ILoginStrategy CrearLoginStrategy(bool modoEmergencia = false)
    {
        return modoEmergencia ? new LoginEmergenciaStrategy() : new LoginNormalStrategy();
    }

    public IUsuarioService CrearUsuarioService()
    {
        return new UsuarioService();
    }

    public IFamiliaService CrearFamiliaService()
    {
        return new FamiliaService();
    }

    public IPatenteService CrearPatenteService()
    {
        return new PatenteService();
    }

    public IBitacoraService CrearBitacoraService()
    {
        return new BitacoraService();
    }

    public IIntegridadService CrearIntegridadService()
    {
        return new IntegridadService();
    }
}
```

`LoginNormalStrategy.cs` **no cambia**: ya llama a `familiaService.GetPerfilDeUsuario`/`ArmarArbolPermisos` y guarda el resultado en `ResultadoAutenticacion.Perfil`/`ArbolPermisos` sin asumir que no sean null en ningún punto propio — el `Perfil` nulo simplemente viaja como tal hasta `Login.aspx.cs`. `LoginEmergenciaStrategy.cs` tampoco cambia: siempre arma un `Perfil` no nulo ("WebMaster").

---

## 6. Capa Web

### 6.1 `Operativ.Web/Paginas/NavegacionHelper.cs` — contenido completo

```csharp
namespace Operativ.Web.Paginas;
public static class NavegacionHelper
{
    public const string PerfilWebMaster = "WebMaster";
    public const string PerfilAdministrador = "Administrador";
    public const string PerfilComercial = "Comercial";
    public const string PerfilCliente = "Cliente";

    public static string ObtenerUrlHome(string nombrePerfil)
    {
        if (nombrePerfil == null)
        {
            return "~/Paginas/Comun/SinFamilia.aspx";
        }

        return nombrePerfil switch
        {
            PerfilWebMaster => "~/Paginas/Home/HomeWebMaster.aspx",
            PerfilAdministrador => "~/Paginas/Home/HomeAdministrador.aspx",
            PerfilComercial => "~/Paginas/Home/HomeComercial.aspx",
            PerfilCliente => "~/Paginas/Home/HomeCliente.aspx",
            _ => "~/Paginas/Usuarios/Login.aspx",
        };
    }
}
```

### 6.2 `Operativ.Web/Paginas/NombrePatente.cs` — contenido completo (archivo nuevo)

Mismo patrón que `NavegacionHelper`: constantes de solo lectura en la capa Web, para no tipear los nombres de patente a mano en cada page y tener autocompletado/refactor seguro. Tienen que coincidir exactamente con la columna `Nombre` sembrada en `Patente` (sección 2.1).

```csharp
namespace Operativ.Web.Paginas;
public static class NombrePatente
{
    public const string ConsultarUsuario = "ConsultarUsuario";
    public const string AltaUsuario = "AltaUsuario";
    public const string BajaUsuario = "BajaUsuario";
    public const string ModificacionUsuario = "ModificacionUsuario";
    public const string DesbloqueoUsuario = "DesbloqueoUsuario";
    public const string BloqueoUsuario = "BloqueoUsuario";
    public const string AsignarPatente = "AsignarPatente";
    public const string RemoverPatente = "RemoverPatente";
}
```

### 6.3 `Operativ.Web/Paginas/PaginaSeguraBase.cs` — contenido completo

```csharp
using Operativ.BE.Entidades;
using Operativ.SEC.Handlers;
using Operativ.Web.Controles;
using Operativ.Web.Master;

namespace Operativ.Web.Paginas;
public abstract class PaginaSeguraBase : PaginaBase
{
    protected SesionHandler SesionHandler { get; private set; }
    protected AutorizacionHandler AutorizacionHandler { get; private set; }
    protected abstract string[] PerfilesPermitidos { get; }

    protected Notificaciones ControlNotificaciones
    {
        get { return ((Principal)Master).ControlNotificaciones; }
    }

    protected override void OnInit(System.EventArgs e)
    {
        base.OnInit(e);

        SesionHandler = new SesionHandler();
        AutorizacionHandler = new AutorizacionHandler();

        ValidarAcceso();
    }

    protected override void OnPreRender(System.EventArgs e)
    {
        base.OnPreRender(e);

        AbrirCambioClaveSiEsProvisoria();
        AplicarVisibilidadPorPatentes();
    }

    protected virtual void AplicarVisibilidadPorPatentes()
    {
    }

    private void ValidarAcceso()
    {
        if (!SesionHandler.HaySesionActiva())
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?err=sesion");
        }

        if (SesionHandler.GetPerfil() == null)
        {
            Response.Redirect("~/Paginas/Comun/SinFamilia.aspx");
        }

        if (!AutorizacionHandler.EsAlgunPerfil(PerfilesPermitidos))
        {
            Response.Redirect("~/Paginas/Comun/NoAutorizado.aspx");
        }
    }

    private void AbrirCambioClaveSiEsProvisoria()
    {
        Usuario usuario = SesionHandler.GetUsuario();

        if (usuario != null && usuario.ContrasenaProvisoria)
        {
            ClientScript.RegisterStartupScript(GetType(), "AbrirModalCambiarClave", "Operativ.abrirModalCambiarClave();", true);
        }
    }
}
```

### 6.4 `Operativ.Web/Paginas/Usuarios/Login.aspx.cs` — contenido completo

Dos cambios de una línea: `perfilActivo?.Nombre` y `resultado.Perfil?.Nombre`, para tolerar un usuario sin familia sin romper.

```csharp
using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class Login : PaginaBase
{
    private readonly FabricaSeguridad fabricaSeguridad;
    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;
    private readonly SesionHandler sesionHandler;
    private bool modoEmergencia;

    public Login()
    {
        fabricaSeguridad = new FabricaSeguridad();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo?.Nombre));
        }
        //VerificarIntegridadSistema();
        if (!IsPostBack && Request.QueryString["err"] == "sesion")
        {
            ucNotificaciones.MostrarMensaje(TipoError.ErrorSesionExpirada);
        }
    }

    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        if (modoEmergencia || !Page.IsValid)
        {
            return;
        }

        ProcesarLogin(
            fabricaSeguridad.CrearLoginStrategy(),
            txtNombreUsuario.Text.Trim(),
            txtContrasena.Text);
    }

    protected void btnIngresoEmergencia_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        ProcesarLogin(
            fabricaSeguridad.CrearLoginStrategy(modoEmergencia: true),
            txtUsuarioEmergencia.Text.Trim(),
            txtContrasenaEmergencia.Text);
    }

    private void ProcesarLogin(ILoginStrategy estrategia, string nombreUsuario, string contrasena)
    {
        try
        {
            ResultadoAutenticacion resultado = estrategia.Autenticar(nombreUsuario, contrasena);
            sesionHandler.IniciarSesion(resultado.Usuario, resultado.Perfil, resultado.ArbolPermisos);
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(resultado.Perfil?.Nombre) + resultado.SufijoRedireccion, false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void VerificarIntegridadSistema()
    {
        try
        {
            List<ResultadoVerificacionTabla> resultadosInvalidos = integridadService.VerificarIntegridad();
            modoEmergencia = resultadosInvalidos.Count > 0;
            if (modoEmergencia)
            {
                string detalle = integridadService.FormatearResumenFallas(resultadosInvalidos);
                ucNotificaciones.MostrarMensaje(TipoError.ErrorIntegridadCorrupta, new string[] { detalle });
                bitacoraService.Registrar(null, TipoAccionBitacora.IntegridadCorrupta, detalle);
                pnlLoginNormal.Visible = false;
                pnlAccesoEmergencia.Visible = true;
            }
        }
        catch (Exception excepcion)
        {
            bitacoraService.Registrar(null, TipoAccionBitacora.IntegridadCorrupta, excepcion.Message);
            modoEmergencia = true;
            pnlLoginNormal.Visible = false;
            pnlAccesoEmergencia.Visible = true;
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }
}
```

### 6.5 `Operativ.Web/Paginas/Comun/NoAutorizado.aspx.cs` — contenido completo

```csharp
using System;
using Operativ.SEC.Handlers;

namespace Operativ.Web.Paginas;
public partial class NoAutorizado : PaginaBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
        SesionHandler sesionHandler = new SesionHandler();

        if (sesionHandler.HaySesionActiva())
        {
            string nombrePerfil = sesionHandler.GetPerfil()?.Nombre;
            lnkVolverHome.NavigateUrl = ResolveUrl(NavegacionHelper.ObtenerUrlHome(nombrePerfil));
        }
        else
        {
            lnkVolverHome.NavigateUrl = ResolveUrl("~/Paginas/Usuarios/Login.aspx");
        }
    }
}
```

### 6.6 `Operativ.Web/Paginas/Controles/ResumenUsuario.ascx.cs` — contenido completo

Antes ocultaba **todo** el control (incluido "Cerrar sesión") si no había perfil. Ahora solo se oculta si no hay usuario logueado; sin perfil, muestra un texto genérico en su lugar y mantiene "Cerrar sesión" visible.

```csharp
using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;

namespace Operativ.Web.Controles;
public partial class ResumenUsuario : UserControl
{
    private readonly IBitacoraService bitacoraService;

    private SesionHandler sesionHandler;

    public ResumenUsuario()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        sesionHandler = new SesionHandler();

        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario == null)
        {
            Visible = false;
            return;
        }

        Familia perfil = sesionHandler.GetPerfil();
        string nombrePerfil = perfil != null
            ? "<strong>" + perfil.Nombre + "</strong>"
            : (string)GetGlobalResourceObject("Textos", "EtiquetaSinFamilia");

        string formatoBienvenida = (string)GetGlobalResourceObject("Textos", "MensajeBienvenida");
        lblBienvenida.Text = string.Format(formatoBienvenida, usuario.NombreUsuario, nombrePerfil);
    }

    protected void lnkCerrarSesion_Click(object sender, EventArgs e)
    {
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario != null)
        {
            try
            {
                bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.CierreSesion);
            }
            catch (Exception)
            {
            }
        }

        sesionHandler.CerrarSesion();
        Response.Redirect("~/Paginas/Usuarios/Login.aspx");
    }
}
```

### 6.7 `Operativ.Web/Paginas/Comun/SinFamilia.aspx` — contenido completo (archivo nuevo)

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SinFamilia.aspx.cs" Inherits="Operativ.Web.Paginas.SinFamilia" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentSinFamilia" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloSinFamilia">Cuenta sin familia asignada</h1>
        <p runat="server" meta:resourcekey="DescripcionSinFamilia">Tu cuenta todavía no tiene una familia (perfil) asignada. Contactá a un Administrador para que te asigne una y puedas acceder al sistema.</p>
    </div>
</asp:Content>
```

### 6.8 `Operativ.Web/Paginas/Comun/SinFamilia.aspx.cs` — contenido completo (archivo nuevo)

```csharp
using System;
using Operativ.SEC.Handlers;

namespace Operativ.Web.Paginas;
public partial class SinFamilia : PaginaBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
        SesionHandler sesionHandler = new SesionHandler();

        if (!sesionHandler.HaySesionActiva())
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx");
        }
    }
}
```

### 6.9 `Operativ.Web/Paginas/Comun/SinFamilia.aspx.designer.cs` — contenido completo (archivo nuevo)

El `h1`/`p` no llevan `ID`, así que no generan campo (mismo criterio que ya sigue `NoAutorizado.aspx.designer.cs` con sus textos fijos):

```csharp
namespace Operativ.Web.Paginas;
public partial class SinFamilia
{
}
```

### 6.10 `Operativ.Web/Paginas/Usuarios/PermisosUsuario.aspx` — contenido completo (archivo nuevo)

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PermisosUsuario.aspx.cs" Inherits="Operativ.Web.Paginas.PermisosUsuario" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentPermisosUsuario" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
            </span>
            <div>
                <h1 id="tituloPermisos" runat="server"></h1>
                <p runat="server" meta:resourcekey="DescripcionPermisosUsuario">Marcá las patentes que querés asignarle individualmente a este usuario, además de las que ya tiene por su familia.</p>
            </div>
        </div>

        <asp:CheckBoxList ID="chkPatentes" runat="server" CssClass="lista-patentes" RepeatDirection="Vertical" RepeatLayout="Flow" />

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnGuardar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
            </asp:LinkButton>
            <asp:HyperLink ID="lnkVolver" runat="server" CssClass="btn-outline" Text="<%$ Resources:Textos, EnlaceVolverGestionUsuarios %>" />
        </div>
    </div>
</asp:Content>
```

### 6.11 `Operativ.Web/Paginas/Usuarios/PermisosUsuario.aspx.designer.cs` — contenido completo (archivo nuevo)

```csharp
namespace Operativ.Web.Paginas;
public partial class PermisosUsuario
{
    protected global::System.Web.UI.HtmlControls.HtmlGenericControl tituloPermisos;

    protected global::System.Web.UI.WebControls.CheckBoxList chkPatentes;

    protected global::System.Web.UI.WebControls.LinkButton btnGuardar;

    protected global::System.Web.UI.WebControls.HyperLink lnkVolver;
}
```

### 6.12 `Operativ.Web/Paginas/Usuarios/PermisosUsuario.aspx.cs` — contenido completo (archivo nuevo)

Página gateada por `PerfilesPermitidos` (familia Administrador, igual que `GestionUsuarios`) **y además** por tener al menos una de las dos patentes nuevas — si alguien llega acá directo por URL sin ninguna de las dos, se lo manda a `NoAutorizado.aspx` aunque sea del perfil correcto.

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class PermisosUsuario : PaginaSeguraBase
{
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;
    private readonly IPatenteService patenteService;
    private int idUsuario;

    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilAdministrador }; }
    }

    public PermisosUsuario()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
        patenteService = fabricaSeguridad.CrearPatenteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        idUsuario = Convert.ToInt32(Request.QueryString["idUsuario"]);

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

        if (!puedeAsignar && !puedeRemover)
        {
            Response.Redirect("~/Paginas/Comun/NoAutorizado.aspx");
            return;
        }

        lnkVolver.NavigateUrl = "~/Paginas/Usuarios/GestionUsuarios.aspx";

        if (!IsPostBack)
        {
            CargarPagina();
        }
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            List<Patente> patentesIndividuales = patenteService.GetPatentesIndividualesDeUsuario(idUsuario);

            foreach (ListItem item in chkPatentes.Items)
            {
                if (!item.Enabled)
                {
                    continue;
                }

                int idPatente = Convert.ToInt32(item.Value);
                bool yaAsignada = TienePatenteIndividual(patentesIndividuales, idPatente);

                if (item.Selected && !yaAsignada)
                {
                    patenteService.AsignarPatente(idUsuario, idPatente);
                }
                else if (!item.Selected && yaAsignada)
                {
                    patenteService.QuitarPatente(idUsuario, idPatente);
                }
            }

            ControlNotificaciones.MostrarExito("MensajeExitoPermisosUsuario");
            CargarPagina();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void CargarPagina()
    {
        Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

        string formato = (string)GetGlobalResourceObject("Textos", "TituloPermisosUsuario");
        tituloPermisos.InnerText = string.Format(formato, usuario.NombreUsuario);

        List<int> idsPatentesFamilia = ObtenerIdsPatentesFamilia(usuario);
        List<Patente> patentesIndividuales = patenteService.GetPatentesIndividualesDeUsuario(idUsuario);

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

        string sufijoFamilia = (string)GetGlobalResourceObject("Textos", "EtiquetaYaPorFamilia");

        chkPatentes.Items.Clear();

        foreach (Patente patente in patenteService.ListarTodas())
        {
            string texto = patente.Nombre;

            if (idsPatentesFamilia.Contains(patente.IdPatente))
            {
                texto = texto + " " + sufijoFamilia;
            }

            ListItem item = new ListItem(texto, patente.IdPatente.ToString());
            item.Selected = TienePatenteIndividual(patentesIndividuales, patente.IdPatente);

            if (item.Selected && !puedeRemover)
            {
                item.Enabled = false;
            }
            else if (!item.Selected && !puedeAsignar)
            {
                item.Enabled = false;
            }

            chkPatentes.Items.Add(item);
        }
    }

    private List<int> ObtenerIdsPatentesFamilia(Usuario usuario)
    {
        List<int> ids = new List<int>();

        if (usuario.Familias.Count == 0)
        {
            return ids;
        }

        List<Patente> patentesFamilia = familiaService.GetPatentesDeFamilia(usuario.Familias[0].IdFamilia);

        foreach (Patente patente in patentesFamilia)
        {
            ids.Add(patente.IdPatente);
        }

        return ids;
    }

    private bool TienePatenteIndividual(List<Patente> patentes, int idPatente)
    {
        foreach (Patente patente in patentes)
        {
            if (patente.IdPatente == idPatente)
            {
                return true;
            }
        }

        return false;
    }
}
```

### 6.13 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx` — contenido completo

Cambios respecto al estado actual: se quita `rfvFamilia` (la familia deja de ser obligatoria), la columna de familia en la grilla muestra "Sin familia" cuando corresponde, y se agrega `lnkPermisos` en las acciones de fila.

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GestionUsuarios.aspx.cs" Inherits="Operativ.Web.Paginas.GestionUsuarios" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentGestionUsuarios" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                        <circle cx="9" cy="7" r="4"></circle>
                        <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
                        <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
                    </svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloGestionUsuarios">Gestión de usuarios</h1>
                    <p runat="server" meta:resourcekey="DescripcionGestionUsuarios">Alta, baja y modificación de usuarios de la plataforma Operativ.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <div class="campo-formulario">
                <label for="<%= txtFiltro.ClientID %>"><asp:Literal ID="litEtiquetaFiltro" runat="server" Text="<%$ Resources:Textos, EtiquetaFiltroUsuarios %>" /></label>
                <asp:TextBox ID="txtFiltro" runat="server" />
            </div>
            <div class="campo-formulario">
                <label for="<%= ddlFiltroFamilia.ClientID %>"><asp:Literal ID="litEtiquetaFiltroFamilia" runat="server" Text="<%$ Resources:Textos, EtiquetaFamilia %>" /></label>
                <asp:DropDownList ID="ddlFiltroFamilia" runat="server" />
            </div>
            <asp:LinkButton ID="btnNuevoUsuario" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnNuevoUsuario_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path><circle cx="8.5" cy="7" r="4"></circle><line x1="20" y1="8" x2="20" y2="14"></line><line x1="23" y1="11" x2="17" y2="11"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonNuevoUsuario %>" />
            </asp:LinkButton>
            <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
            </asp:LinkButton>
        </div>

        <div class="tabla-contenedor">
        <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
            DataKeyNames="IdUsuario" OnRowCommand="gvUsuarios_RowCommand" OnRowDataBound="gvUsuarios_RowDataBound" GridLines="None">
            <Columns>
                <asp:BoundField DataField="NombreUsuario" HeaderText="<%$ Resources:Textos, EtiquetaNombreUsuario %>" />
                <asp:BoundField DataField="NombreCompleto" HeaderText="<%$ Resources:Textos, EtiquetaNombreCompleto %>" />
                <asp:BoundField DataField="Email" HeaderText="<%$ Resources:Textos, EtiquetaCorreoElectronico %>" />
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFamilia %>">
                    <ItemTemplate>
                        <%# string.IsNullOrEmpty((string)Eval("NombreFamilia"))
                            ? (string)GetGlobalResourceObject("Textos", "EtiquetaSinFamiliaAsignada")
                            : Eval("NombreFamilia") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaEstado %>">
                    <ItemTemplate>
                        <span class='badge <%# (bool)Eval("Bloqueado") ? "badge-bloqueado" : "badge-activo" %>'>
                            <%# (bool)Eval("Bloqueado")
                                ? (string)GetGlobalResourceObject("Textos", "EstadoBloqueado")
                                : (string)GetGlobalResourceObject("Textos", "EstadoActivo") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <div class="acciones-fila">
                            <asp:LinkButton ID="lnkEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IdUsuario") %>'
                                CssClass="btn-outline" CausesValidation="false">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 3a2.828 2.828 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"></path></svg>
                                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonEditar %>" />
                            </asp:LinkButton>
                            <asp:HyperLink ID="lnkPermisos" runat="server" CssClass="btn-outline"
                                NavigateUrl='<%# "~/Paginas/Usuarios/PermisosUsuario.aspx?idUsuario=" + Eval("IdUsuario") %>'>
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"></path><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"></path></svg>
                                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonPermisos %>" />
                            </asp:HyperLink>
                            <asp:LinkButton ID="lnkBaja" runat="server" CommandName="Baja" CommandArgument='<%# Eval("IdUsuario") %>'
                                CssClass="btn-outline-peligro" CausesValidation="false"
                                OnClientClick="return confirm('¿Confirma que desea dar de baja al usuario?');">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDarBaja %>" />
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        </div>

        <div class="paginado">
            <asp:Literal ID="litResumenPaginado" runat="server" />
            <div class="paginado-controles">
                <asp:Button ID="btnPaginaAnterior" runat="server" Text="<%$ Resources:Textos, BotonPaginaAnterior %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnPaginaAnterior_Click" />
                <span class="paginado-numero"><asp:Literal ID="litNumeroPagina" runat="server" /></span>
                <asp:Button ID="btnPaginaSiguiente" runat="server" Text="<%$ Resources:Textos, BotonPaginaSiguiente %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnPaginaSiguiente_Click" />
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlFormularioUsuario" runat="server" CssClass="tarjeta">
        <div class="tarjeta-encabezado-titulo">
            <span class="icono-circulo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                    <circle cx="8.5" cy="7" r="4"></circle>
                    <line x1="20" y1="8" x2="20" y2="14"></line>
                    <line x1="23" y1="11" x2="17" y2="11"></line>
                </svg>
            </span>
            <h2 id="tituloFormulario" runat="server"></h2>
        </div>
        <asp:HiddenField ID="hidIdUsuario" runat="server" Value="0" />

        <asp:Panel ID="pnlDesbloqueo" runat="server" Visible="false">
            <p><asp:Literal ID="litMensajeBloqueado" runat="server" /></p>
            <div class="acciones-formulario">
                <asp:LinkButton ID="btnDesbloquear" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnDesbloquear_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 9.9-1"></path></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonDesbloquearUsuario %>" />
                </asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCamposEdicion" runat="server">
            <div class="fila-formulario">
                <div class="campo-formulario">
                    <label for="<%= txtNombreUsuarioAlta.ClientID %>"><asp:Literal ID="litEtiquetaUsuario" runat="server" Text="<%$ Resources:Textos, EtiquetaNombreUsuario %>" /></label>
                    <asp:TextBox ID="txtNombreUsuarioAlta" runat="server" autocomplete="off" />
                    <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuarioAlta"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionUsuarioObligatorio %>" CssClass="texto-validacion" Display="Dynamic" />
                </div>

                <div class="campo-formulario">
                    <label for="<%= txtNombreCompleto.ClientID %>"><asp:Literal ID="litEtiquetaNombreCompleto" runat="server" Text="<%$ Resources:Textos, EtiquetaNombreCompleto %>" /></label>
                    <asp:TextBox ID="txtNombreCompleto" runat="server" />
                    <asp:RequiredFieldValidator ID="rfvNombreCompleto" runat="server" ControlToValidate="txtNombreCompleto"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionNombreCompletoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" />
                </div>

                <div class="campo-formulario">
                    <label for="<%= txtEmail.ClientID %>"><asp:Literal ID="litEtiquetaEmail" runat="server" Text="<%$ Resources:Textos, EtiquetaCorreoElectronico %>" /></label>
                    <asp:TextBox ID="txtEmail" runat="server" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoObligatorio %>" CssClass="texto-validacion" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                        ValidationExpression="^[\w\.\-]+@[\w\-]+\.[\w\.\-]+$"
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionCorreoFormato %>" CssClass="texto-validacion" Display="Dynamic" />
                </div>

                <div class="campo-formulario">
                    <label for="<%= ddlFamilia.ClientID %>"><asp:Literal ID="litEtiquetaFamilia" runat="server" Text="<%$ Resources:Textos, EtiquetaFamilia %>" /></label>
                    <asp:DropDownList ID="ddlFamilia" runat="server" />
                </div>
            </div>

            <div class="acciones-formulario">
                <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn-primario" OnClick="btnGuardar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path><polyline points="17 21 17 13 7 13 7 21"></polyline><polyline points="7 3 7 8 15 8"></polyline></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonGuardar %>" />
                </asp:LinkButton>
                <asp:LinkButton ID="btnBloquear" runat="server" CssClass="btn-peligro" CausesValidation="false" Visible="false" OnClick="btnBloquear_Click"
                    OnClientClick="return confirm('¿Confirma que desea bloquear a este usuario?');">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBloquearUsuario %>" />
                </asp:LinkButton>
            </div>
        </asp:Panel>

        <div class="acciones-formulario">
            <asp:LinkButton ID="btnCancelar" runat="server" CssClass="btn-outline" CausesValidation="false" OnClick="btnCancelar_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCancelar %>" />
            </asp:LinkButton>
        </div>
        <asp:ValidationSummary ID="vsGestionUsuarios" runat="server" CssClass="texto-validacion" />
    </asp:Panel>
</asp:Content>
```

### 6.14 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx.designer.cs` — contenido completo

Se quita `rfvFamilia`, se agrega `lnkPermisos`.

```csharp
namespace Operativ.Web.Paginas;
public partial class GestionUsuarios
{
    protected global::System.Web.UI.WebControls.LinkButton btnNuevoUsuario;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaFiltro;

    protected global::System.Web.UI.WebControls.TextBox txtFiltro;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaFiltroFamilia;

    protected global::System.Web.UI.WebControls.DropDownList ddlFiltroFamilia;

    protected global::System.Web.UI.WebControls.LinkButton btnBuscar;

    protected global::System.Web.UI.WebControls.GridView gvUsuarios;

    protected global::System.Web.UI.WebControls.Literal litResumenPaginado;

    protected global::System.Web.UI.WebControls.Button btnPaginaAnterior;

    protected global::System.Web.UI.WebControls.Literal litNumeroPagina;

    protected global::System.Web.UI.WebControls.Button btnPaginaSiguiente;

    protected global::System.Web.UI.WebControls.Panel pnlFormularioUsuario;

    protected global::System.Web.UI.HtmlControls.HtmlGenericControl tituloFormulario;

    protected global::System.Web.UI.WebControls.HiddenField hidIdUsuario;

    protected global::System.Web.UI.WebControls.Panel pnlDesbloqueo;

    protected global::System.Web.UI.WebControls.Literal litMensajeBloqueado;

    protected global::System.Web.UI.WebControls.LinkButton btnDesbloquear;

    protected global::System.Web.UI.WebControls.Panel pnlCamposEdicion;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaUsuario;

    protected global::System.Web.UI.WebControls.TextBox txtNombreUsuarioAlta;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvNombreUsuario;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaNombreCompleto;

    protected global::System.Web.UI.WebControls.TextBox txtNombreCompleto;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvNombreCompleto;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaEmail;

    protected global::System.Web.UI.WebControls.TextBox txtEmail;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvEmail;

    protected global::System.Web.UI.WebControls.RegularExpressionValidator revEmail;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaFamilia;

    protected global::System.Web.UI.WebControls.DropDownList ddlFamilia;

    protected global::System.Web.UI.WebControls.LinkButton btnGuardar;

    protected global::System.Web.UI.WebControls.LinkButton btnBloquear;

    protected global::System.Web.UI.WebControls.LinkButton btnCancelar;

    protected global::System.Web.UI.WebControls.ValidationSummary vsGestionUsuarios;
}
```

### 6.15 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx.cs` — contenido completo

Cambios: `using Operativ.BE.Enums;` nuevo; `ValidarPatente` privado nuevo; cada handler mutante lo llama primero; `AplicarVisibilidadPorPatentes` override nuevo; `gvUsuarios_RowDataBound` nuevo; `btnGuardar_Click` maneja `idFamilia` nullable; `ddlFamilia.SelectedIndex = 0` en `PrepararAlta` sigue igual (deja seleccionado el placeholder, que ahora es una opción válida en sí misma).

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class GestionUsuarios : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaUsuarios;
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;

    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilAdministrador }; }
    }

    private int NumeroPagina
    {
        get { return ViewState["NumeroPagina"] == null ? 1 : (int)ViewState["NumeroPagina"]; }
        set { ViewState["NumeroPagina"] = value; }
    }

    public GestionUsuarios()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            List<Familia> familias = familiaService.ListarFamilias();
            CargarFamilias(ddlFamilia, familias, "EtiquetaFamiliaPlaceholder");
            CargarFamilias(ddlFiltroFamilia, familias, "EtiquetaTodasLasFamilias");
            PrepararAlta();
            pnlFormularioUsuario.Visible = false;
        }

        CargarGrilla();
    }

    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        NumeroPagina = 1;
        CargarGrilla();
    }

    protected void btnNuevoUsuario_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.AltaUsuario))
        {
            return;
        }

        PrepararAlta();
        MostrarPanelConFoco(txtNombreUsuarioAlta);
    }

    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        PrepararAlta();
        pnlFormularioUsuario.Visible = false;
    }

    protected void btnPaginaAnterior_Click(object sender, EventArgs e)
    {
        if (NumeroPagina > 1)
        {
            NumeroPagina--;
        }

        CargarGrilla();
    }

    protected void btnPaginaSiguiente_Click(object sender, EventArgs e)
    {
        NumeroPagina++;
        CargarGrilla();
    }

    protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int idUsuario = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "Editar")
        {
            CargarUsuarioParaEdicion(idUsuario);
        }
        else if (e.CommandName == "Baja")
        {
            DarDeBaja(idUsuario);
        }
    }

    protected void gvUsuarios_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
        {
            return;
        }

        LinkButton lnkEditar = (LinkButton)e.Row.FindControl("lnkEditar");
        lnkEditar.Visible = AutorizacionHandler.TienePatente(NombrePatente.ModificacionUsuario);

        LinkButton lnkBaja = (LinkButton)e.Row.FindControl("lnkBaja");
        lnkBaja.Visible = AutorizacionHandler.TienePatente(NombrePatente.BajaUsuario);

        HyperLink lnkPermisos = (HyperLink)e.Row.FindControl("lnkPermisos");
        lnkPermisos.Visible = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente)
            || AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);
    }

    protected override void AplicarVisibilidadPorPatentes()
    {
        btnNuevoUsuario.Visible = AutorizacionHandler.TienePatente(NombrePatente.AltaUsuario);

        if (btnGuardar.Visible)
        {
            bool esAlta = hidIdUsuario.Value == "0";
            string patenteRequerida = esAlta ? NombrePatente.AltaUsuario : NombrePatente.ModificacionUsuario;
            btnGuardar.Visible = AutorizacionHandler.TienePatente(patenteRequerida);
        }

        if (btnBloquear.Visible)
        {
            btnBloquear.Visible = AutorizacionHandler.TienePatente(NombrePatente.BloqueoUsuario);
        }

        if (btnDesbloquear.Visible)
        {
            btnDesbloquear.Visible = AutorizacionHandler.TienePatente(NombrePatente.DesbloqueoUsuario);
        }
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        int idUsuario = Convert.ToInt32(hidIdUsuario.Value);
        bool esAlta = idUsuario == 0;
        string patenteRequerida = esAlta ? NombrePatente.AltaUsuario : NombrePatente.ModificacionUsuario;

        if (!ValidarPatente(patenteRequerida))
        {
            return;
        }

        try
        {
            int? idFamilia = string.IsNullOrEmpty(ddlFamilia.SelectedValue) ? (int?)null : Convert.ToInt32(ddlFamilia.SelectedValue);

            if (esAlta)
            {
                usuarioService.AltaUsuario(txtNombreUsuarioAlta.Text.Trim(), txtNombreCompleto.Text.Trim(), txtEmail.Text.Trim(), idFamilia);
                ControlNotificaciones.MostrarExito("MensajeExitoAltaUsuario");
            }
            else
            {
                Usuario usuario = new Usuario
                {
                    IdUsuario = idUsuario,
                    NombreUsuario = txtNombreUsuarioAlta.Text.Trim(),
                    NombreCompleto = txtNombreCompleto.Text.Trim(),
                    Email = txtEmail.Text.Trim()
                };

                usuarioService.ModificarUsuario(usuario, idFamilia);
                ControlNotificaciones.MostrarExito("MensajeExitoModificacionUsuario");
            }

            PrepararAlta();
            pnlFormularioUsuario.Visible = false;
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void btnDesbloquear_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.DesbloqueoUsuario))
        {
            return;
        }

        try
        {
            int idUsuario = Convert.ToInt32(hidIdUsuario.Value);

            usuarioService.DesbloquearUsuario(idUsuario);

            Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);
            MostrarPanelEdicion(usuario);

            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void btnBloquear_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.BloqueoUsuario))
        {
            return;
        }

        try
        {
            int idUsuario = Convert.ToInt32(hidIdUsuario.Value);

            usuarioService.BloquearUsuario(idUsuario);
            ControlNotificaciones.MostrarExito("MensajeExitoBloqueoUsuario");

            Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);
            MostrarPanelDesbloqueo(usuario);

            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private bool ValidarPatente(string nombrePatente)
    {
        if (AutorizacionHandler.TienePatente(nombrePatente))
        {
            return true;
        }

        ControlNotificaciones.MostrarMensaje(TipoError.ErrorSinPermiso, new string[] { nombrePatente });
        return false;
    }

    private void DarDeBaja(int idUsuario)
    {
        if (!ValidarPatente(NombrePatente.BajaUsuario))
        {
            return;
        }

        try
        {
            usuarioService.BajaUsuario(idUsuario);
            ControlNotificaciones.MostrarExito("MensajeExitoBajaUsuario");
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void CargarUsuarioParaEdicion(int idUsuario)
    {
        if (!ValidarPatente(NombrePatente.ModificacionUsuario))
        {
            return;
        }

        try
        {
            Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

            hidIdUsuario.Value = usuario.IdUsuario.ToString();

            if (usuario.Bloqueado)
            {
                MostrarPanelDesbloqueo(usuario);
            }
            else
            {
                MostrarPanelEdicion(usuario);
            }
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void MostrarPanelDesbloqueo(Usuario usuario)
    {
        pnlDesbloqueo.Visible = true;
        pnlCamposEdicion.Visible = false;

        string formato = (string)GetGlobalResourceObject("Textos", "MensajeUsuarioBloqueado");
        litMensajeBloqueado.Text = string.Format(formato, usuario.NombreUsuario);

        tituloFormulario.InnerText = (string)GetGlobalResourceObject("Textos", "TituloFormularioModificacion");

        MostrarPanelConFoco(btnDesbloquear);
    }

    private void MostrarPanelEdicion(Usuario usuario)
    {
        pnlDesbloqueo.Visible = false;
        pnlCamposEdicion.Visible = true;

        hidIdUsuario.Value = usuario.IdUsuario.ToString();
        txtNombreUsuarioAlta.Text = usuario.NombreUsuario;
        txtNombreUsuarioAlta.ReadOnly = true;
        txtNombreCompleto.Text = usuario.NombreCompleto;
        txtEmail.Text = usuario.Email;
        btnBloquear.Visible = true;

        ddlFamilia.SelectedIndex = 0;

        if (usuario.Familias.Count > 0)
        {
            ddlFamilia.SelectedValue = usuario.Familias[0].IdFamilia.ToString();
        }

        tituloFormulario.InnerText = (string)GetGlobalResourceObject("Textos", "TituloFormularioModificacion");

        MostrarPanelConFoco(txtNombreCompleto);
    }

    private void PrepararAlta()
    {
        hidIdUsuario.Value = "0";
        txtNombreUsuarioAlta.Text = string.Empty;
        txtNombreUsuarioAlta.ReadOnly = false;
        txtNombreCompleto.Text = string.Empty;
        txtEmail.Text = string.Empty;
        ddlFamilia.SelectedIndex = 0;
        btnBloquear.Visible = false;

        pnlDesbloqueo.Visible = false;
        pnlCamposEdicion.Visible = true;

        tituloFormulario.InnerText = (string)GetGlobalResourceObject("Textos", "TituloFormularioAlta");
    }

    private void MostrarPanelConFoco(Control campoFoco)
    {
        pnlFormularioUsuario.Visible = true;
        SetFocus(campoFoco);

        string script = "document.getElementById('" + pnlFormularioUsuario.ClientID + "')"
            + ".scrollIntoView({ behavior: 'smooth', block: 'start' });";
        ClientScript.RegisterStartupScript(GetType(), "ScrollFormularioUsuario", script, true);
    }

    private void CargarFamilias(DropDownList ddl, List<Familia> familias, string claveTextoPlaceholder)
    {
        ddl.DataSource = familias;
        ddl.DataTextField = "Nombre";
        ddl.DataValueField = "IdFamilia";
        ddl.DataBind();

        string textoPlaceholder = (string)GetGlobalResourceObject("Textos", claveTextoPlaceholder);
        ddl.Items.Insert(0, new ListItem(textoPlaceholder, string.Empty));
    }

    private void CargarGrilla()
    {
        string filtro = txtFiltro.Text.Trim();
        int? idFamilia = ObtenerIdFamiliaFiltro();

        List<Usuario> usuarios = usuarioService.ListarUsuarios(filtro, idFamilia, NumeroPagina, tamanioPagina);
        int total = usuarioService.ContarUsuarios(filtro, idFamilia);

        gvUsuarios.DataSource = usuarios;
        gvUsuarios.DataBind();

        ActualizarResumenPaginado(total, usuarios.Count);
    }

    private int? ObtenerIdFamiliaFiltro()
    {
        if (string.IsNullOrEmpty(ddlFiltroFamilia.SelectedValue))
        {
            return null;
        }

        return Convert.ToInt32(ddlFiltroFamilia.SelectedValue);
    }

    private void ActualizarResumenPaginado(int total, int cantidadEnPagina)
    {
        int desde = total == 0 ? 0 : ((NumeroPagina - 1) * tamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        string formato = (string)GetGlobalResourceObject("Textos", "MensajeResumenPaginado");
        litResumenPaginado.Text = string.Format(formato, desde, hasta, total);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnPaginaAnterior.Enabled = NumeroPagina > 1;
        btnPaginaSiguiente.Enabled = (NumeroPagina * tamanioPagina) < total;
    }
}
```

Nota sobre `MostrarPanelEdicion`: se agregó `ddlFamilia.SelectedIndex = 0;` antes del `if` que setea el valor real, para que un usuario **sin** familia muestre el placeholder ("Sin familia") en vez de arrastrar la última selección visual de una edición anterior en la misma sesión de postbacks.

---

## 7. Recursos (`Textos.resx` / `Textos.en.resx`)

Se quita el uso de `MensajeValidacionFamiliaObligatoria` (ya no hay validador de familia, pero la clave puede quedar sin usar en el resx, no rompe nada — no hace falta borrarla).

Se actualiza el valor de una clave existente en ambos archivos (la familia pasa de ser un placeholder de "elegí algo" a una opción válida en sí misma):

```xml
<!-- Textos.resx -->
<data name="EtiquetaFamiliaPlaceholder" xml:space="preserve">
  <value>Sin familia</value>
</data>
```
```xml
<!-- Textos.en.resx -->
<data name="EtiquetaFamiliaPlaceholder" xml:space="preserve">
  <value>No family</value>
</data>
```

Agregar en `Textos.resx`:

```xml
<data name="EtiquetaSinFamilia" xml:space="preserve">
  <value>Sin familia asignada</value>
</data>
<data name="EtiquetaSinFamiliaAsignada" xml:space="preserve">
  <value>Sin familia</value>
</data>
<data name="TituloSinFamilia" xml:space="preserve">
  <value>Cuenta sin familia asignada</value>
</data>
<data name="DescripcionSinFamilia" xml:space="preserve">
  <value>Tu cuenta todavía no tiene una familia (perfil) asignada. Contactá a un Administrador para que te asigne una y puedas acceder al sistema.</value>
</data>
<data name="BotonPermisos" xml:space="preserve">
  <value>Permisos</value>
</data>
<data name="TituloPermisosUsuario" xml:space="preserve">
  <value>Permisos individuales de {0}</value>
</data>
<data name="DescripcionPermisosUsuario" xml:space="preserve">
  <value>Marcá las patentes que querés asignarle individualmente a este usuario, además de las que ya tiene por su familia.</value>
</data>
<data name="EtiquetaYaPorFamilia" xml:space="preserve">
  <value>(ya incluida por la familia)</value>
</data>
<data name="MensajeExitoPermisosUsuario" xml:space="preserve">
  <value>Permisos individuales actualizados correctamente.</value>
</data>
<data name="EnlaceVolverGestionUsuarios" xml:space="preserve">
  <value>Volver a gestión de usuarios</value>
</data>
<data name="MensajeErrorPatenteYaAsignada" xml:space="preserve">
  <value>Esa patente ya está asignada individualmente a este usuario.</value>
</data>
<data name="MensajeErrorPatenteNoAsignada" xml:space="preserve">
  <value>Esa patente no está asignada individualmente a este usuario.</value>
</data>
<data name="MensajeErrorSinPermiso" xml:space="preserve">
  <value>No tenés el permiso '{0}' para realizar esta operación.</value>
</data>
```

Agregar en `Textos.en.resx`:

```xml
<data name="EtiquetaSinFamilia" xml:space="preserve">
  <value>No family assigned</value>
</data>
<data name="EtiquetaSinFamiliaAsignada" xml:space="preserve">
  <value>No family</value>
</data>
<data name="TituloSinFamilia" xml:space="preserve">
  <value>Account without an assigned family</value>
</data>
<data name="DescripcionSinFamilia" xml:space="preserve">
  <value>Your account does not have a family (profile) assigned yet. Contact an Administrator to have one assigned so you can access the system.</value>
</data>
<data name="BotonPermisos" xml:space="preserve">
  <value>Permissions</value>
</data>
<data name="TituloPermisosUsuario" xml:space="preserve">
  <value>Individual permissions for {0}</value>
</data>
<data name="DescripcionPermisosUsuario" xml:space="preserve">
  <value>Check the patentes you want to grant individually to this user, in addition to the ones they already have through their family.</value>
</data>
<data name="EtiquetaYaPorFamilia" xml:space="preserve">
  <value>(already included by family)</value>
</data>
<data name="MensajeExitoPermisosUsuario" xml:space="preserve">
  <value>Individual permissions updated successfully.</value>
</data>
<data name="EnlaceVolverGestionUsuarios" xml:space="preserve">
  <value>Back to user management</value>
</data>
<data name="MensajeErrorPatenteYaAsignada" xml:space="preserve">
  <value>That patente is already individually assigned to this user.</value>
</data>
<data name="MensajeErrorPatenteNoAsignada" xml:space="preserve">
  <value>That patente is not individually assigned to this user.</value>
</data>
<data name="MensajeErrorSinPermiso" xml:space="preserve">
  <value>You don't have the '{0}' permission to perform this operation.</value>
</data>
```

---

## 8. Estilos (`Operativ.Web/Estilos/operativ.css`)

Un solo agregado, para que la lista de patentes de `PermisosUsuario.aspx` no se vea como una tira de checkboxes pegados (todo lo demás reutiliza `.tarjeta`/`.btn-primario`/`.btn-outline`/`.acciones-formulario` ya existentes):

```css
.lista-patentes {
    display: block;
    margin: 20px 0;
}

.lista-patentes label {
    display: block;
    padding: 10px 0;
    border-bottom: 1px solid #e5e7eb;
    font-size: 14px;
}

.lista-patentes input[type=checkbox] {
    margin-right: 8px;
}
```

---

## 9. Pasos para Claude Code

1. `git checkout main && git pull`
2. `git checkout -b feature/parche-2.0-gestion-usuario-familia-patente`
3. Aplicar el reemplazo del seed en `Scripts/CrearBaseDatos.sql` (sección 2) y recrear la base local.
4. Aplicar los archivos de las secciones 3 a 6 en orden (BE → DAL → SEC → Web), son varios con dependencias entre sí (por ejemplo `FabricaRepositorio` tiene que existir antes de que `FamiliaService`/`PatenteService` compilen).
5. Aplicar los recursos (sección 7) y el CSS (sección 8).
6. Commits sugeridos:
   - `feat(db): reemplaza GestionarUsuarios por patentes finas de usuario y agrega AsignarPatente/RemoverPatente`
   - `feat(bll): arma el arbol de permisos combinando familia e individuales`
   - `feat(bll): permite usuarios sin familia y reasignar familia en modificacion`
   - `fix(web): evita null reference cuando el usuario logueado no tiene familia`
   - `feat(web): agrega SinFamilia.aspx para usuarios sin perfil asignado`
   - `feat(bll): agrega asignacion y remocion de patentes individuales`
   - `feat(web): agrega pagina de permisos individuales y boton Permisos en GestionUsuarios`
   - `feat(web): oculta y revalida controles de GestionUsuarios segun patente`
7. Build completo: `MSBuild.exe Operativ.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"`.
8. Pruebas manuales (con los 4 usuarios semilla; `admin` ahora tiene las 8 patentes finas de usuario vía la familia Administrador):
   - Crear un usuario nuevo dejando el combo de familia en "Sin familia" → alta exitosa, la grilla lo muestra con "Sin familia" en la columna de familia.
   - Intentar loguearse con ese usuario nuevo (con la clave temporal del mail de bienvenida) → login exitoso, redirige a `SinFamilia.aspx` con el mensaje explicativo; el encabezado sigue mostrando "Bienvenido {usuario}, sin familia asignada" y el enlace de "Cerrar sesión" funciona.
   - Cerrar sesión, volver a entrar como `admin`, editar a ese usuario y asignarle la familia `Cliente` → guardar; loguearse de nuevo con ese usuario → ahora entra directo a `HomeCliente.aspx`.
   - Editar un usuario que sí tiene familia y volver a guardar dejando el combo en "Sin familia" → el usuario pierde la familia (queda como en el primer caso).
   - En la grilla de `GestionUsuarios`, click en "Permisos" de cualquier usuario → abre `PermisosUsuario.aspx` con el listado completo de patentes; las que vienen de la familia del usuario (si tiene) muestran la leyenda "(ya incluida por la familia)".
   - Tildar una patente individual nueva y Guardar → éxito, queda tildada al recargar; en la base, aparece la fila en `UsuarioPatente`.
   - Destildar una patente individual → éxito, desaparece de `UsuarioPatente`.
   - Con un segundo usuario Administrador (creado a mano, sin darle `AsignarPatente` ni `RemoverPatente` individualmente ni por familia — para esta prueba hay que sacárselo a mano de `FamiliaPatente` o crear una familia de prueba sin esas dos), verificar que el botón "Permisos" no aparece en la grilla para ningún usuario, y que entrar directo por URL a `PermisosUsuario.aspx?idUsuario=X` redirige a `NoAutorizado.aspx`.
   - Sacarle a `admin` la patente `BajaUsuario` a mano en la base (`DELETE FROM FamiliaPatente WHERE ...`) y volver a loguearse → el botón "Dar de baja" desaparece de todas las filas de la grilla; si se arma a mano un POST directo al evento de baja, el sistema debe rechazarlo con `ERR20`.
   - Restaurar esa patente y confirmar que el botón vuelve a aparecer.
9. No mergear a `main`. Dejar la rama lista con los commits para que el dueño del repo la revise y haga el push.
