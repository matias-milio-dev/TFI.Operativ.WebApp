# Plan: Backup y restore de la base de datos bajo demanda — Operativ (2.1)

**Repo:** `matias-milio-dev/TFI.Operativ.WebApp`, base `main` (HEAD auditado: `2978dfeb6f0eb0838a1d4200425937028c757342` — "usa int[] en vez de List&lt;int&gt; para los lotes de patentes"; el parche 2.0 ya está mergeado y con varias iteraciones propias encima: `PatentesPermitidas` en `PaginaSeguraBase`, `CategoriaPatente`/`TipoCategoriaPatente` como smart enum, `PermisosUsuario` reescrita, asignación de patentes en lote).
**Rama a crear:** `feature/parche-2.1-backup-restore-base-datos`.
**No hacer merge ni push a `main`.** Dejar los commits listos en la rama local para que el dueño del repo los revise y los suba él mismo.

Recordatorio de estándares (`Estandares_Codigo_y_Estilo_Operativ.md`): sin comentarios en C#, sin `var`, sin tuplas/record/LINQ/lambdas (nada de `Action`/`Func`/expresiones lambda — ni siquiera para ordenar una lista, ver sección 5.4), `namespace X;` scoped, ifs con llaves, orden público→privado, repositorios/servicios solo vía fábricas, errores vía `OperativException`/`TipoError`/`DefinicionError`, auditoría vía `IBitacoraService`.

**Excepción explícita y acotada a este parche:** el estándar dice "sin Stored Procedures" para el acceso a datos habitual (`AccesoDatos` con SQL parametrizado). Este parche es una excepción deliberada, pedida explícitamente: el backup y el restore de la base **sí** se implementan como Stored Procedures. Ver sección 1.1 para la justificación técnica de por qué además tienen que vivir en `master`, no en `OperativDb`.

---

## 0. Corrección a implementar

Funcionalidad de backup y restore de la base de datos **bajo demanda** (no programado, un WebMaster lo dispara manualmente desde una página nueva):
1. Implementarlo con Stored Procedures.
2. Agregar los permisos correspondientes: a la familia que corresponda según el caso de uso, y patentes individuales para poder asignarlas una por una (ya existe hace varios parches una patente `RealizarBackup` sembrada y asociada a WebMaster, con la descripción "Permite realizar backup y restore de la base de datos" — sin ninguna funcionalidad real detrás todavía; este parche la separa en dos y construye la funcionalidad real).

---

## 1. Diseño y decisiones

### 1.1 Por qué los Stored Procedures viven en `master`, no en `OperativDb`

`BACKUP DATABASE` puede correr en caliente, con la base en uso normal — no hay problema en tenerlo como Stored Procedure dentro de `OperativDb`. `RESTORE DATABASE`, en cambio, necesita **acceso exclusivo**: no puede haber ninguna sesión conectada a `OperativDb` en el momento de restaurarla, ni siquiera la sesión que ejecuta el propio `RESTORE`. Si el Stored Procedure de restore viviera en `OperativDb`, la sesión que lo está ejecutando ya estaría "usando" esa base por el solo hecho de haberse conectado a ella para llamarlo — un auto-bloqueo.

La solución, y el mismo patrón que ya usa `Scripts/CrearBaseDatos.sql` en su primera línea útil (`ALTER DATABASE OperativDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE OperativDb;`, para poder recrearla), es que la sesión que ejecuta el restore esté conectada a **otra** base — `master` es la elección estándar de SQL Server para esto. Por eso:

- Los dos Stored Procedures (`uspBackupOperativDb`, `uspRestoreOperativDb`) se crean en `master`.
- El `RESTORE` empieza poniendo a `OperativDb` en `SINGLE_USER WITH ROLLBACK IMMEDIATE` (mata cualquier otra sesión conectada, incluidas las que estén en el pool de conexiones de ASP.NET) antes de restaurar, y la vuelve a `MULTI_USER` al final — envuelto en `TRY/CATCH` para que, si el restore falla a mitad de camino, la base no quede trabada en modo mono-usuario para siempre.
- Como beneficio adicional, los procedures quedan en `master`, así que un restore de `OperativDb` nunca se borra a sí mismo ni a la infraestructura que lo ejecuta.

`AccesoDatos`/`ConexionDB` hoy están atados exclusivamente a la connection string `OperativDb` de `Web.config`. Se agrega una segunda cadena de conexión derivada en runtime (mismo servidor y credenciales, `Initial Catalog=master`) en vez de hardcodear una segunda entrada en `Web.config` — así no hay dos connection strings que puedan desincronizarse si el día de mañana cambia el servidor.

### 1.2 Dos patentes, no una

La patente `RealizarBackup` ya sembrada cubría backup **y** restore en un solo permiso. Se separa en `RealizarBackup` (generar un backup — de bajo riesgo, no destructivo) y una nueva `RestaurarBackup` (restaurar — de alto riesgo, reemplaza **todos** los datos actuales sin posibilidad de deshacer). Es el mismo criterio que ya se usó en el parche 2.0 para separar `AsignarPatente`/`RemoverPatente`: dos acciones con perfiles de riesgo muy distintos no deberían compartir un único permiso todo-o-nada. Ambas quedan en la categoría `Sistema` que ya agrupaba a `RealizarBackup` junto con `RepararBaseDatos`.

No hay un CU numerado para "Backup"/"Restore" en la carpeta de TFI (sí hay uno para "Reparar Base de Datos", CU-002-002, que es una funcionalidad distinta ya implementada vía el acceso de emergencia). Se sigue el mismo criterio que ya se usó para `BloqueoUsuario` en el parche 1.9: una patente para una capacidad real del sistema, aunque no tenga un número de CU formal en la documentación.

Familia: se le asignan las dos a `WebMaster`, la misma familia que ya tenía `RealizarBackup` y `RepararBaseDatos` desde el seed original — es la única familia pensada para mantenimiento técnico de la plataforma.

### 1.3 Dónde vive la página y cómo se accede

Página nueva `Operativ.Web/Paginas/Sistema/BackupRestore.aspx` (carpeta `Sistema/` nueva, mismo criterio de organización que `Usuarios/`, `Comun/`, `Home/`). Se accede con `PatentesPermitidas` (el mecanismo que ya trajo el parche 2.0: alcanza con tener alguna de las patentes declaradas, sin importar si viene de la familia o individual) declarando `RealizarBackup`/`RestaurarBackup` — mismo patrón exacto que ya usa `PermisosUsuario.aspx` con `AsignarPatente`/`RemoverPatente`. Dentro de la página, el botón "Crear backup" y el botón "Restaurar" de cada fila se muestran y se revalidan del lado servidor cada uno contra su propia patente (no alcanza con la del otro) — mismo criterio de visibilidad + revalidación de `GestionUsuarios` del parche 2.0.

Se agrega un enlace "Backup" al navbar, visible solo si el usuario tiene alguna de las dos patentes — mismo patrón que ya usa `lnkUsuarios` con `CategoriaPatente.Usuarios.NombresPatente`.

### 1.4 Nombre y ubicación de los backups

Cada backup se guarda como `OperativDb_yyyyMMdd_HHmmss.bak` (nombre con fecha, nunca se pisan entre sí) en una carpeta configurable por `Web.config` (`Operativ.Backups.Carpeta`, mismo patrón que ya usa `Operativ.Emergencia.RutaXml` en `ConfiguracionAplicacion`), con default `C:\OperativBackups\`. A diferencia de la ruta de emergencia (que es relativa a la app, `~/App_Data/...`), esta es una ruta de **filesystem absoluta**, a propósito: `BACKUP DATABASE`/`RESTORE DATABASE` corren del lado del **servicio de SQL Server**, no del proceso de IIS Express, así que necesitan una carpeta que la cuenta del servicio de SQL Server pueda escribir/leer — meterla dentro de la carpeta del sitio web no tiene sentido y podría ni siquiera ser accesible para ese servicio.

**Requisito operativo (igual naturaleza que el de SMTP ya documentado en `CLAUDE.md`):** la carpeta configurada tiene que ser escribible por la cuenta del servicio de SQL Server Express **y** legible por la cuenta que corre IIS Express (para poder listar los archivos desde la página). En una instalación de desarrollo local típica, donde la propia cuenta de Windows del desarrollador suele tener rol sysadmin sobre su instancia local de `.\SQLEXPRESS`, esto no debería requerir configuración adicional — se deja como nota para no asumir que "simplemente funciona" en cualquier entorno.

El listado de backups se arma leyendo el directorio con `System.IO.Directory.GetFiles` (no hay una tabla en la base para esto — son archivos, no filas). El orden (más reciente primero) se resuelve con un ordenamiento explícito por `FechaCreacion` (burbuja simple, sin `List<T>.Sort` con delegado y sin LINQ), no confiando en el orden en que el sistema operativo devuelve los archivos.

### 1.5 Manejo de errores: por qué no alcanza con el mapeo genérico existente

`ErroresHandler.TraducirExcepcion` hoy mapea **cualquier** `SqlException` no reconocida a `TipoError.ErrorConexionBaseDatos` ("No se puede conectar a la base de datos") — un mensaje engañoso para un backup/restore que sí conectó bien pero falló por otro motivo (ruta inaccesible, disco lleno, archivo de backup corrupto). Por eso `BackupService` atrapa `SqlException` específicamente en cada operación y la re-lanza como `OperativException(TipoError.ErrorOperacionBackupFallida, ...)` con el mensaje real de SQL Server como parámetro, así el WebMaster ve el motivo real del fallo. Se agrega también `ErrorArchivoBackupNoExiste` para el caso borde de que el archivo elegido para restaurar haya sido borrado del disco entre que se cargó la página y que se confirmó el restore.

### 1.6 Después de un restore exitoso, se fuerza el cierre de sesión

Un restore reemplaza toda la base, incluida la tabla `Usuario` — los datos de sesión en memoria (`Usuario`, `Familia`, árbol de permisos) podrían quedar desincronizados de lo que hay ahora en la base (usuario borrado en el backup restaurado, contraseña distinta, etc.). Igual que ya hace el flujo de reparación de emergencia (`HomeWebMaster.btnAceptarReparacion_Click`: cierra sesión y manda a Login tras una reparación), tras un restore exitoso se cierra la sesión y se redirige a `Login.aspx?restaurado=1`, que muestra un mensaje de éxito y pide loguearse de nuevo. No se inventa un mecanismo nuevo — se reutiliza el mismo criterio ya establecido para "acabamos de tocar la base a bajo nivel, hay que volver a autenticar".

### 1.7 Qué NO se toca (fuera de alcance)

- No hay backups programados/automáticos — es explícitamente "bajo demanda", tal como se pidió.
- No se agrega una forma de **borrar** backups viejos desde la UI (limpieza manual por ahora vía filesystem). Se puede agregar en un parche futuro si hace falta.
- El `RESTORE` asume que se restaura sobre el mismo servidor/misma ubicación de archivos de datos donde se hizo el backup (no lleva `WITH MOVE`) — es el escenario real de este proyecto (un único `.\SQLEXPRESS` local). Restaurar en otro servidor con otra estructura de carpetas queda fuera de alcance.
- No se le agrega esta funcionalidad a WebMaster desde `HomeWebMaster.aspx` con una tarjeta de acceso rápido — el punto de entrada es el enlace nuevo del navbar, igual que "Usuarios" no tiene una tarjeta propia en ninguna Home.

---

## 2. Base de datos

### 2.1 `Scripts/CrearBaseDatos.sql` — actualizar el seed de `Patente` y `FamiliaPatente`

Reemplazar la fila de `RealizarBackup` y agregar `RestaurarBackup` en el bloque `INSERT INTO Patente`:

```sql
INSERT INTO Patente (Nombre, Descripcion) VALUES
    ('RepararBaseDatos', 'Permite ejecutar el modulo de reparacion de la base de datos.'),
    ('RealizarBackup', 'Permite generar backups de la base de datos.'),
    ('RestaurarBackup', 'Permite restaurar la base de datos desde un backup existente.'),
    ('ConsultarUsuario', 'Permite ver el listado de usuarios de la plataforma.'),
    ('AltaUsuario', 'Permite crear nuevos usuarios en la plataforma.'),
    ('BajaUsuario', 'Permite dar de baja usuarios.'),
    ('ModificacionUsuario', 'Permite modificar los datos de un usuario.'),
    ('DesbloqueoUsuario', 'Permite desbloquear usuarios bloqueados.'),
    ('BloqueoUsuario', 'Permite bloquear usuarios.'),
    ('AsignarPatente', 'Permite asignar patentes a usuarios.'),
    ('RemoverPatente', 'Permite quitar patentes asignadas a un usuario.'),
    ('GestionarFamilias', 'Permite dar de alta, baja y modificar familias y sus patentes.'),
    ('GestionarClientes', 'Permite dar de alta, baja y modificar clientes.'),
    ('GestionarCatalogo', 'Permite administrar el catalogo de paquetes.'),
    ('GestionarSuscripciones', 'Permite contratar y administrar suscripciones.'),
    ('ConsultarFacturas', 'Permite consultar las facturas emitidas.'),
    ('ReportarIncidentes', 'Permite reportar incidentes sobre activos.');
GO
```

Y agregar `RestaurarBackup` a la lista de `WebMaster` en el `INSERT INTO FamiliaPatente`:

```sql
INSERT INTO FamiliaPatente (IdFamilia, IdPatente)
SELECT F.IdFamilia, P.IdPatente
FROM Familia F, Patente P
WHERE (F.Nombre = 'WebMaster' AND P.Nombre IN ('RepararBaseDatos', 'RealizarBackup', 'RestaurarBackup'))
   OR (F.Nombre = 'Administrador' AND P.Nombre IN ('ConsultarUsuario', 'AltaUsuario', 'BajaUsuario', 'ModificacionUsuario', 'DesbloqueoUsuario', 'BloqueoUsuario', 'AsignarPatente', 'RemoverPatente', 'GestionarFamilias'))
   OR (F.Nombre = 'Comercial' AND P.Nombre IN ('GestionarClientes', 'GestionarCatalogo'))
   OR (F.Nombre = 'Cliente' AND P.Nombre IN ('GestionarSuscripciones', 'ConsultarFacturas', 'ReportarIncidentes'));
GO
```

### 2.2 `Scripts/CrearBaseDatos.sql` — agregar los Stored Procedures al final del script

Van en `master`, con guarda de idempotencia (`DROP PROCEDURE` si ya existen) porque el script recrea `OperativDb` en cada corrida pero nunca toca `master`. El restore está envuelto en `TRY/CATCH` para garantizar que `OperativDb` vuelva a `MULTI_USER` incluso si el `RESTORE` falla a mitad de camino.

```sql
-- Los Stored Procedures de backup/restore viven en master, no en OperativDb: RESTORE DATABASE
-- necesita acceso exclusivo a OperativDb, y la sesion que lo ejecuta no puede estar conectada
-- a la base que esta restaurando. Ver Plan_Parche_2.1_Operativ.md seccion 1.1.
USE master;
GO

IF OBJECT_ID('dbo.uspBackupOperativDb', 'P') IS NOT NULL
    DROP PROCEDURE dbo.uspBackupOperativDb;
GO

CREATE PROCEDURE dbo.uspBackupOperativDb
    @RutaArchivo NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BACKUP DATABASE OperativDb TO DISK = @RutaArchivo WITH INIT, STATS = 10;
END
GO

IF OBJECT_ID('dbo.uspRestoreOperativDb', 'P') IS NOT NULL
    DROP PROCEDURE dbo.uspRestoreOperativDb;
GO

CREATE PROCEDURE dbo.uspRestoreOperativDb
    @RutaArchivo NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    ALTER DATABASE OperativDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

    BEGIN TRY
        RESTORE DATABASE OperativDb FROM DISK = @RutaArchivo WITH REPLACE, STATS = 10;
    END TRY
    BEGIN CATCH
        ALTER DATABASE OperativDb SET MULTI_USER;
        THROW;
    END CATCH

    ALTER DATABASE OperativDb SET MULTI_USER;
END
GO

USE OperativDb;
GO
```

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
    ErrorOperacionBackupFallida,
    ErrorArchivoBackupNoExiste,
    FalloNoManejadoGenerico
}
```

### 3.2 `Operativ.BE/Modelos/DefinicionError.cs` — contenido completo

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
    public static readonly DefinicionError ErrorOperacionBackupFallida =
        new(TipoError.ErrorOperacionBackupFallida, "ERR21", "MensajeErrorOperacionBackupFallida");
    public static readonly DefinicionError ErrorArchivoBackupNoExiste =
        new(TipoError.ErrorArchivoBackupNoExiste, "ERR22", "MensajeErrorArchivoBackupNoExiste");
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
            ErrorOperacionBackupFallida,
            ErrorArchivoBackupNoExiste,
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
    RemocionPatente,
    BackupBaseDatos,
    RestoreBaseDatos
}
```

### 3.4 `Operativ.BE/Modelos/AccionBitacora.cs` — contenido completo

`RestoreBaseDatos` queda con criticidad `Critico`, igual que `ReparacionEmergenciaBaseDatos` (mismo nivel de impacto: reemplaza toda la base). `BackupBaseDatos` queda como `Advertencia`, igual que `AsignacionPatente`/`RemocionPatente` (una acción administrativa notable pero no destructiva).

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
    public static readonly AccionBitacora BackupBaseDatos =
        new(TipoAccionBitacora.BackupBaseDatos, CriticidadBitacora.Advertencia, "Backup de la base de datos generado bajo demanda");
    public static readonly AccionBitacora RestoreBaseDatos =
        new(TipoAccionBitacora.RestoreBaseDatos, CriticidadBitacora.Critico, "Restore de la base de datos ejecutado bajo demanda");

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
            RemocionPatente,
            BackupBaseDatos,
            RestoreBaseDatos
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

### 3.5 `Operativ.BE/Modelos/NombrePatente.cs` — contenido completo

```csharp
namespace Operativ.BE.Modelos;

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
    public const string GestionarFamilias = "GestionarFamilias";
    public const string GestionarClientes = "GestionarClientes";
    public const string GestionarCatalogo = "GestionarCatalogo";
    public const string GestionarSuscripciones = "GestionarSuscripciones";
    public const string ConsultarFacturas = "ConsultarFacturas";
    public const string ReportarIncidentes = "ReportarIncidentes";
    public const string RealizarBackup = "RealizarBackup";
    public const string RestaurarBackup = "RestaurarBackup";
    public const string RepararBaseDatos = "RepararBaseDatos";
}
```

### 3.6 `Operativ.BE/Modelos/CategoriaPatente.cs` — contenido completo

Solo cambia la categoría `Sistema`, que ahora agrupa las tres patentes de mantenimiento técnico.

```csharp
using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Modelos;

public class CategoriaPatente
{
    public TipoCategoriaPatente Tipo { get; }

    public string ClaveRecurso { get; }

    public string[] NombresPatente { get; }

    public CategoriaPatente(TipoCategoriaPatente tipo, string claveRecurso, string[] nombresPatente)
    {
        Tipo = tipo;
        ClaveRecurso = claveRecurso;
        NombresPatente = nombresPatente;
    }

    public static readonly CategoriaPatente Usuarios =
        new(TipoCategoriaPatente.Usuarios, "CategoriaUsuarios", new[]
        {
            NombrePatente.ConsultarUsuario,
            NombrePatente.AltaUsuario,
            NombrePatente.BajaUsuario,
            NombrePatente.ModificacionUsuario,
            NombrePatente.DesbloqueoUsuario,
            NombrePatente.BloqueoUsuario,
            NombrePatente.AsignarPatente,
            NombrePatente.RemoverPatente
        });
    public static readonly CategoriaPatente Familias =
        new(TipoCategoriaPatente.Familias, "CategoriaFamilias", new[] { NombrePatente.GestionarFamilias });
    public static readonly CategoriaPatente Clientes =
        new(TipoCategoriaPatente.Clientes, "CategoriaClientes", new[] { NombrePatente.GestionarClientes });
    public static readonly CategoriaPatente Catalogo =
        new(TipoCategoriaPatente.Catalogo, "CategoriaCatalogo", new[] { NombrePatente.GestionarCatalogo });
    public static readonly CategoriaPatente Suscripciones =
        new(TipoCategoriaPatente.Suscripciones, "CategoriaSuscripciones", new[] { NombrePatente.GestionarSuscripciones });
    public static readonly CategoriaPatente Facturacion =
        new(TipoCategoriaPatente.Facturacion, "CategoriaFacturacion", new[] { NombrePatente.ConsultarFacturas });
    public static readonly CategoriaPatente Incidentes =
        new(TipoCategoriaPatente.Incidentes, "CategoriaIncidentes", new[] { NombrePatente.ReportarIncidentes });
    public static readonly CategoriaPatente Sistema =
        new(TipoCategoriaPatente.Sistema, "CategoriaSistema", new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup, NombrePatente.RepararBaseDatos });

    public static List<CategoriaPatente> ObtenerTodas()
    {
        return new List<CategoriaPatente>
        {
            Usuarios,
            Familias,
            Clientes,
            Catalogo,
            Suscripciones,
            Facturacion,
            Incidentes,
            Sistema
        };
    }

    public static CategoriaPatente ObtenerPorTipo(TipoCategoriaPatente tipo)
    {
        foreach (CategoriaPatente categoria in ObtenerTodas())
        {
            if (categoria.Tipo == tipo)
            {
                return categoria;
            }
        }

        return null;
    }

    public static CategoriaPatente ObtenerPorPatente(string nombrePatente)
    {
        foreach (CategoriaPatente categoria in ObtenerTodas())
        {
            foreach (string nombre in categoria.NombresPatente)
            {
                if (nombre == nombrePatente)
                {
                    return categoria;
                }
            }
        }

        return Sistema;
    }
}
```

### 3.7 `Operativ.BE/Entidades/ArchivoBackup.cs` — contenido completo (archivo nuevo)

Representa un archivo `.bak` en disco (no es una fila de una tabla — se arma leyendo el filesystem, ver sección 5.4).

```csharp
using System;

namespace Operativ.BE.Entidades;
public class ArchivoBackup
{
    public string NombreArchivo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public long TamanioBytes { get; set; }
}
```

---

## 4. Capa DAL

### 4.1 `Operativ.DAL/Conexion/ConexionDB.cs` — contenido completo

Se agrega `GetCadenaConexionMaster()`, derivada en runtime de la cadena de `OperativDb` cambiando el catálogo inicial — no se agrega una segunda entrada en `Web.config` a propósito (ver sección 1.1).

```csharp
using System.Configuration;
using System.Data.SqlClient;

namespace Operativ.DAL.Conexion;
public class ConexionDB
{
    private static ConexionDB instancia;
    private static readonly object bloqueo = new object();
    private readonly string cadenaConexion;
    private readonly string cadenaConexionMaster;

    private ConexionDB()
    {
        cadenaConexion = ConfigurationManager.ConnectionStrings["OperativDb"].ConnectionString;
        cadenaConexionMaster = ConstruirCadenaConexionMaster(cadenaConexion);
    }
    public static ConexionDB Instancia
    {
        get
        {
            lock (bloqueo)
            {
                if (instancia == null)
                {
                    instancia = new ConexionDB();
                }
            }
            return instancia;
        }
    }

    public string GetCadenaConexion()
    {
        return cadenaConexion;
    }

    public string GetCadenaConexionMaster()
    {
        return cadenaConexionMaster;
    }

    private string ConstruirCadenaConexionMaster(string cadenaOriginal)
    {
        SqlConnectionStringBuilder constructor = new SqlConnectionStringBuilder(cadenaOriginal);
        constructor.InitialCatalog = "master";
        return constructor.ConnectionString;
    }
}
```

### 4.2 `Operativ.DAL/Conexion/AccesoDatos.cs` — contenido completo

Se agrega `EjecutarConsultaEnMaster`, gemelo de `EjecutarConsulta` pero contra la cadena de `master` y con un `CommandTimeout` más generoso (120 segundos en vez del default de 30): `ALTER DATABASE ... SET SINGLE_USER` más `BACKUP`/`RESTORE` pueden tardar más que una consulta parametrizada común, aunque la base de este proyecto sea chica. El resto de la clase no cambia.

```csharp
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Operativ.DAL.Conexion;
public class AccesoDatos
{
    public DataTable EjecutarReader(string consulta, List<SqlParameter> parametros)
    {
        DataTable tabla = new DataTable();
        using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
        {
            using (SqlCommand comando = new SqlCommand(consulta, conexion))
            {
                AgregarParametros(comando, parametros);
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }
            }
        }
        return tabla;
    }

    public int EjecutarConsulta(string consulta, List<SqlParameter> parametros)
    {
        int filasAfectadas;
        using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
        {
            using (SqlCommand comando = new SqlCommand(consulta, conexion))
            {
                AgregarParametros(comando, parametros);
                conexion.Open();
                filasAfectadas = comando.ExecuteNonQuery();
            }
        }
        return filasAfectadas;
    }

    public int EjecutarConsultaEnMaster(string consulta, List<SqlParameter> parametros)
    {
        int filasAfectadas;
        using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexionMaster()))
        {
            using (SqlCommand comando = new SqlCommand(consulta, conexion))
            {
                comando.CommandTimeout = 120;
                AgregarParametros(comando, parametros);
                conexion.Open();
                filasAfectadas = comando.ExecuteNonQuery();
            }
        }
        return filasAfectadas;
    }

    public object EjecutarEscalar(string consulta, List<SqlParameter> parametros)
    {
        object resultado;
        using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
        {
            using (SqlCommand comando = new SqlCommand(consulta, conexion))
            {
                AgregarParametros(comando, parametros);
                conexion.Open();
                resultado = comando.ExecuteScalar();
            }
        }
        return resultado;
    }

    private void AgregarParametros(SqlCommand comando, List<SqlParameter> parametros)
    {
        if (parametros != null)
        {
            foreach (SqlParameter parametro in parametros)
            {
                comando.Parameters.Add(parametro);
            }
        }
    }
}
```

### 4.3 `Operativ.DAL/Contratos/IBackupRepositorio.cs` — contenido completo (archivo nuevo)

```csharp
namespace Operativ.DAL.Contratos;
public interface IBackupRepositorio
{
    void EjecutarBackup(string rutaArchivo);

    void EjecutarRestore(string rutaArchivo);
}
```

### 4.4 `Operativ.DAL/Implementaciones/BackupRepositorio.cs` — contenido completo (archivo nuevo)

Único lugar de todo el DAL que arma un `EXEC` de Stored Procedure en vez de SQL de tabla — es la excepción de este parche (sección 1.1).

```csharp
using System.Collections.Generic;
using System.Data.SqlClient;
using Operativ.DAL.Conexion;
using Operativ.DAL.Contratos;

namespace Operativ.DAL.Implementaciones;
public class BackupRepositorio : IBackupRepositorio
{
    private readonly AccesoDatos accesoDatos;

    public BackupRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public void EjecutarBackup(string rutaArchivo)
    {
        string consulta = "EXEC master.dbo.uspBackupOperativDb @RutaArchivo";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@RutaArchivo", rutaArchivo)
        };

        accesoDatos.EjecutarConsultaEnMaster(consulta, parametros);
    }

    public void EjecutarRestore(string rutaArchivo)
    {
        string consulta = "EXEC master.dbo.uspRestoreOperativDb @RutaArchivo";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@RutaArchivo", rutaArchivo)
        };

        accesoDatos.EjecutarConsultaEnMaster(consulta, parametros);
    }
}
```

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

    public IBackupRepositorio CrearBackupRepositorio()
    {
        return new BackupRepositorio();
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

### 5.1 `Operativ.SEC/Configuracion/ConfiguracionAplicacion.cs` — contenido completo

Se agrega `CarpetaBackups`, mismo patrón que `RutaXmlEmergencia`.

```csharp
using System.Configuration;

namespace Operativ.SEC.Configuracion;
public static class ConfiguracionAplicacion
{
    public static int IntentosMaximosLogin
    {
        get { return int.Parse(GetConfiguracion("Operativ.IntentosMaximosLogin", "3")); }
    }

    public static int LongitudContrasenaTemporal
    {
        get { return int.Parse(GetConfiguracion("Operativ.LongitudContrasenaTemporal", "10")); }
    }

    public static int TamanoPredeterminadoGrillaUsuarios
    {
        get { return int.Parse(GetConfiguracion("Operativ.TamanoPredeterminadoGrillaUsuarios", "10")); }
    }

    public static string ServidorSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Servidor", "localhost"); }
    }

    public static int PuertoSmtp
    {
        get { return int.Parse(GetConfiguracion("Operativ.Smtp.Puerto", "25")); }
    }

    public static string UsuarioSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Usuario", string.Empty); }
    }

    public static string ContrasenaSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Contrasena", string.Empty); }
    }

    public static bool UsarSslSmtp
    {
        get { return bool.Parse(GetConfiguracion("Operativ.Smtp.UsarSsl", "false")); }
    }

    public static string EmailRemitente
    {
        get { return GetConfiguracion("Operativ.Smtp.EmailRemitente", "no-responder@operativ.com"); }
    }

    public static bool HabilitarEnvioEmail
    {
        get { return bool.Parse(GetConfiguracion("HabilitarEnvioEmail", "false")); }
    }

    public static string RutaXmlEmergencia
    {
        get { return GetConfiguracion("Operativ.Emergencia.RutaXml", "~/App_Data/AccesoEmergencia.xml"); }
    }

    public static string CarpetaBackups
    {
        get { return GetConfiguracion("Operativ.Backups.Carpeta", @"C:\OperativBackups\"); }
    }

    private static string GetConfiguracion(string clave, string valorPorDefecto)
    {
        string valor = ConfigurationManager.AppSettings[clave];

        if (string.IsNullOrEmpty(valor))
        {
            valor = valorPorDefecto;
        }

        return valor;
    }
}
```

### 5.2 `Operativ.Web/Web.config` — agregar una clave a `<appSettings>`

```xml
<add key="Operativ.Backups.Carpeta" value="C:\OperativBackups\" />
```

(Se puede ajustar el valor según la carpeta real que se vaya a usar en cada máquina — ver el requisito operativo de la sección 1.4.)

### 5.3 `Operativ.SEC/Contratos/IBackupService.cs` — contenido completo (archivo nuevo)

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IBackupService
{
    List<ArchivoBackup> ListarBackups();

    string CrearBackup();

    void RestaurarBackup(string nombreArchivo);
}
```

### 5.4 `Operativ.SEC/Implementaciones/BackupService.cs` — contenido completo (archivo nuevo)

El ordenamiento por fecha descendente se hace con un bubble sort explícito, no con `List<T>.Sort(Comparison<T>)`: ese overload recibe un delegado, y el proyecto no usa delegados/lambdas en ningún lado — es la misma razón por la que el parche 2.0 resolvió la pertenencia a una lista con `foreach` en vez de `List<T>.Exists(Predicate<T>)`.

```csharp
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.SEC.Implementaciones;
public class BackupService : IBackupService
{
    private readonly IBackupRepositorio backupRepositorio;
    private readonly IBitacoraService bitacoraService;

    public BackupService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        backupRepositorio = fabricaRepositorio.CrearBackupRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public List<ArchivoBackup> ListarBackups()
    {
        List<ArchivoBackup> backups = new List<ArchivoBackup>();
        string carpeta = ConfiguracionAplicacion.CarpetaBackups;

        if (!Directory.Exists(carpeta))
        {
            return backups;
        }

        foreach (string ruta in Directory.GetFiles(carpeta, "*.bak"))
        {
            FileInfo info = new FileInfo(ruta);
            backups.Add(new ArchivoBackup
            {
                NombreArchivo = info.Name,
                FechaCreacion = info.CreationTime,
                TamanioBytes = info.Length
            });
        }

        OrdenarPorFechaDescendente(backups);

        return backups;
    }

    public string CrearBackup()
    {
        string carpeta = ConfiguracionAplicacion.CarpetaBackups;

        if (!Directory.Exists(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        string nombreArchivo = "OperativDb_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
        string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

        try
        {
            backupRepositorio.EjecutarBackup(rutaCompleta);
        }
        catch (SqlException excepcion)
        {
            throw new OperativException(TipoError.ErrorOperacionBackupFallida, new string[] { excepcion.Message });
        }

        bitacoraService.Registrar(null, TipoAccionBitacora.BackupBaseDatos, nombreArchivo);

        return nombreArchivo;
    }

    public void RestaurarBackup(string nombreArchivo)
    {
        string rutaCompleta = Path.Combine(ConfiguracionAplicacion.CarpetaBackups, nombreArchivo);

        if (!File.Exists(rutaCompleta))
        {
            throw new OperativException(TipoError.ErrorArchivoBackupNoExiste, new string[] { nombreArchivo });
        }

        try
        {
            backupRepositorio.EjecutarRestore(rutaCompleta);
        }
        catch (SqlException excepcion)
        {
            throw new OperativException(TipoError.ErrorOperacionBackupFallida, new string[] { excepcion.Message });
        }

        bitacoraService.Registrar(null, TipoAccionBitacora.RestoreBaseDatos, nombreArchivo);
    }

    private void OrdenarPorFechaDescendente(List<ArchivoBackup> backups)
    {
        for (int i = 0; i < backups.Count - 1; i++)
        {
            for (int j = 0; j < backups.Count - 1 - i; j++)
            {
                if (backups[j].FechaCreacion < backups[j + 1].FechaCreacion)
                {
                    ArchivoBackup temporal = backups[j];
                    backups[j] = backups[j + 1];
                    backups[j + 1] = temporal;
                }
            }
        }
    }
}
```

`Registrar(null, ...)` sigue el mismo criterio que ya usa `ReparacionEmergenciaBaseDatos`/`IntegridadCorrupta`: son operaciones a nivel sistema, no dirigidas a un usuario puntual, así que no llevan `IdUsuario` (la bitácora de este proyecto registra "qué pasó", no "quién lo hizo").

### 5.5 `Operativ.SEC/Fabricas/FabricaSeguridad.cs` — contenido completo

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

    public IBackupService CrearBackupService()
    {
        return new BackupService();
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

No hace falta tocar `AutorizacionHandler.cs`: `TienePatente`/`TieneAlgunaPatente` ya existen desde el parche 2.0 y son genéricos (reciben cualquier nombre de patente).

---

## 6. Capa Web

### 6.1 `Operativ.Web/Paginas/Sistema/BackupRestore.aspx` — contenido completo (archivo nuevo)

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BackupRestore.aspx.cs" Inherits="Operativ.Web.Paginas.BackupRestore" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentBackupRestore" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><ellipse cx="12" cy="5" rx="9" ry="3"></ellipse><path d="M21 12c0 1.66-4 3-9 3s-9-1.34-9-3"></path><path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"></path></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloBackupRestore">Backup y restore de la base de datos</h1>
                    <p runat="server" meta:resourcekey="DescripcionBackupRestore">Generá un backup de la base bajo demanda o restaurala desde uno existente.</p>
                </div>
            </div>
            <asp:LinkButton ID="btnCrearBackup" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnCrearBackup_Click">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonCrearBackup %>" />
            </asp:LinkButton>
        </div>

        <div class="tabla-contenedor">
        <asp:GridView ID="gvBackups" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
            DataKeyNames="NombreArchivo" OnRowCommand="gvBackups_RowCommand" OnRowDataBound="gvBackups_RowDataBound" GridLines="None">
            <Columns>
                <asp:BoundField DataField="NombreArchivo" HeaderText="<%$ Resources:Textos, EtiquetaArchivoBackup %>" />
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFechaBackup %>">
                    <ItemTemplate>
                        <%# ((DateTime)Eval("FechaCreacion")).ToString("dd/MM/yyyy HH:mm") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaTamanioBackup %>">
                    <ItemTemplate>
                        <%# ((long)Eval("TamanioBytes") / 1024) %> KB
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkRestaurar" runat="server" CommandName="Restaurar" CommandArgument='<%# Eval("NombreArchivo") %>'
                            CssClass="btn-outline-peligro" CausesValidation="false"
                            OnClientClick='<%# "return confirm(\"¿Confirma que desea restaurar la base de datos desde " + Eval("NombreArchivo") + "? Esto reemplaza TODOS los datos actuales y no se puede deshacer.\");" %>'>
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="1 4 1 10 7 10"></polyline><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10"></path></svg>
                            <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonRestaurarBackup %>" />
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinBackups %>" />
            </EmptyDataTemplate>
        </asp:GridView>
        </div>
    </div>
</asp:Content>
```

### 6.2 `Operativ.Web/Paginas/Sistema/BackupRestore.aspx.designer.cs` — contenido completo (archivo nuevo)

```csharp
namespace Operativ.Web.Paginas;
public partial class BackupRestore
{
    protected global::System.Web.UI.WebControls.LinkButton btnCrearBackup;

    protected global::System.Web.UI.WebControls.GridView gvBackups;
}
```

### 6.3 `Operativ.Web/Paginas/Sistema/BackupRestore.aspx.cs` — contenido completo (archivo nuevo)

`PatentesPermitidas` con las dos patentes (alcanza con tener una para entrar a la página), y dentro de la página cada acción se muestra y se revalida contra su propia patente — mismo criterio de `GestionUsuarios`/`PermisosUsuario` del parche 2.0.

```csharp
using System;
using System.Web.UI.WebControls;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class BackupRestore : PaginaSeguraBase
{
    private readonly IBackupService backupService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup }; }
    }

    public BackupRestore()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        backupService = fabricaSeguridad.CrearBackupService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarGrilla();
        }
    }

    protected override void AplicarVisibilidadPorPatentes()
    {
        btnCrearBackup.Visible = AutorizacionHandler.TienePatente(NombrePatente.RealizarBackup);
    }

    protected void btnCrearBackup_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.RealizarBackup))
        {
            return;
        }

        try
        {
            string nombreArchivo = backupService.CrearBackup();
            string formato = (string)GetGlobalResourceObject("Textos", "MensajeExitoCrearBackup");
            ControlNotificaciones.MostrarMensaje(string.Format(formato, nombreArchivo), true);
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void gvBackups_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Restaurar")
        {
            return;
        }

        if (!ValidarPatente(NombrePatente.RestaurarBackup))
        {
            return;
        }

        string nombreArchivo = e.CommandArgument.ToString();

        try
        {
            backupService.RestaurarBackup(nombreArchivo);

            SesionHandler.CerrarSesion();
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?restaurado=1");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void gvBackups_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
        {
            return;
        }

        LinkButton lnkRestaurar = (LinkButton)e.Row.FindControl("lnkRestaurar");
        lnkRestaurar.Visible = AutorizacionHandler.TienePatente(NombrePatente.RestaurarBackup);
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

    private void CargarGrilla()
    {
        gvBackups.DataSource = backupService.ListarBackups();
        gvBackups.DataBind();
    }
}
```

### 6.4 `Operativ.Web/Paginas/Controles/Navbar.ascx` — contenido completo

```html
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navbar.ascx.cs" Inherits="Operativ.Web.Controles.Navbar" %>
<div class="navbar">
    <span class="navbar-marca">Operativ<span class="navbar-marca-acento">.</span></span>
    <asp:HyperLink ID="lnkHome" runat="server" CssClass="navbar-link navbar-link-activo" Text="<%$ Resources:Textos, EnlaceInicio %>" />
    <asp:HyperLink ID="lnkUsuarios" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Usuarios/GestionUsuarios.aspx" Text="<%$ Resources:Textos, EnlaceUsuarios %>" Visible="false" />
    <asp:HyperLink ID="lnkBackup" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Sistema/BackupRestore.aspx" Text="<%$ Resources:Textos, EnlaceBackup %>" Visible="false" />
</div>
```

### 6.5 `Operativ.Web/Paginas/Controles/Navbar.ascx.designer.cs` — contenido completo

```csharp
namespace Operativ.Web.Controles;
public partial class Navbar
{
    protected global::System.Web.UI.WebControls.HyperLink lnkHome;

    protected global::System.Web.UI.WebControls.HyperLink lnkUsuarios;

    protected global::System.Web.UI.WebControls.HyperLink lnkBackup;
}
```

### 6.6 `Operativ.Web/Paginas/Controles/Navbar.ascx.cs` — contenido completo

```csharp
using System;
using System.Web.UI;
using Operativ.BE.Modelos;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web.Controles;
public partial class Navbar : UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        AutorizacionHandler autorizacionHandler = new AutorizacionHandler();
        string nombrePerfil = autorizacionHandler.GetNombrePerfil();

        lnkHome.Visible = !string.IsNullOrEmpty(nombrePerfil);

        if (lnkHome.Visible)
        {
            lnkHome.NavigateUrl = ResolveUrl(NavegacionHelper.ObtenerUrlHome(nombrePerfil));
        }

        lnkUsuarios.Visible = autorizacionHandler.TieneAlgunaPatente(CategoriaPatente.Usuarios.NombresPatente);
        lnkBackup.Visible = autorizacionHandler.TieneAlgunaPatente(new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup });
    }
}
```

### 6.7 `Operativ.Web/Paginas/Usuarios/Login.aspx.cs` — agregar el manejo de `?restaurado=1`

Agregar este bloque en `Page_Load`, junto al que ya maneja `?err=sesion` (no se muestra el archivo completo porque el resto no cambia — es una única adición):

```csharp
        if (!IsPostBack && Request.QueryString["restaurado"] == "1")
        {
            string mensaje = (string)GetGlobalResourceObject("Textos", "MensajeExitoRestaurarBackup");
            ucNotificaciones.MostrarMensaje(mensaje, true);
        }
```

---

## 7. Recursos (`Textos.resx` / `Textos.en.resx`)

Agregar en `Textos.resx`:

```xml
<data name="TituloBackupRestore" xml:space="preserve">
  <value>Backup y restore de la base de datos</value>
</data>
<data name="DescripcionBackupRestore" xml:space="preserve">
  <value>Generá un backup de la base bajo demanda o restaurala desde uno existente.</value>
</data>
<data name="BotonCrearBackup" xml:space="preserve">
  <value>Crear backup</value>
</data>
<data name="BotonRestaurarBackup" xml:space="preserve">
  <value>Restaurar</value>
</data>
<data name="EtiquetaArchivoBackup" xml:space="preserve">
  <value>Archivo</value>
</data>
<data name="EtiquetaFechaBackup" xml:space="preserve">
  <value>Fecha</value>
</data>
<data name="EtiquetaTamanioBackup" xml:space="preserve">
  <value>Tamaño</value>
</data>
<data name="MensajeSinBackups" xml:space="preserve">
  <value>Todavía no se generó ningún backup.</value>
</data>
<data name="MensajeExitoCrearBackup" xml:space="preserve">
  <value>Backup {0} creado correctamente.</value>
</data>
<data name="MensajeExitoRestaurarBackup" xml:space="preserve">
  <value>La base de datos fue restaurada correctamente. Iniciá sesión nuevamente.</value>
</data>
<data name="MensajeErrorOperacionBackupFallida" xml:space="preserve">
  <value>No se pudo completar la operación: {0}</value>
</data>
<data name="MensajeErrorArchivoBackupNoExiste" xml:space="preserve">
  <value>El archivo de backup '{0}' ya no existe.</value>
</data>
<data name="EnlaceBackup" xml:space="preserve">
  <value>Backup</value>
</data>
```

Agregar en `Textos.en.resx`:

```xml
<data name="TituloBackupRestore" xml:space="preserve">
  <value>Database backup and restore</value>
</data>
<data name="DescripcionBackupRestore" xml:space="preserve">
  <value>Generate an on-demand database backup or restore it from an existing one.</value>
</data>
<data name="BotonCrearBackup" xml:space="preserve">
  <value>Create backup</value>
</data>
<data name="BotonRestaurarBackup" xml:space="preserve">
  <value>Restore</value>
</data>
<data name="EtiquetaArchivoBackup" xml:space="preserve">
  <value>File</value>
</data>
<data name="EtiquetaFechaBackup" xml:space="preserve">
  <value>Date</value>
</data>
<data name="EtiquetaTamanioBackup" xml:space="preserve">
  <value>Size</value>
</data>
<data name="MensajeSinBackups" xml:space="preserve">
  <value>No backups have been generated yet.</value>
</data>
<data name="MensajeExitoCrearBackup" xml:space="preserve">
  <value>Backup {0} created successfully.</value>
</data>
<data name="MensajeExitoRestaurarBackup" xml:space="preserve">
  <value>The database was restored successfully. Please log in again.</value>
</data>
<data name="MensajeErrorOperacionBackupFallida" xml:space="preserve">
  <value>The operation could not be completed: {0}</value>
</data>
<data name="MensajeErrorArchivoBackupNoExiste" xml:space="preserve">
  <value>The backup file '{0}' no longer exists.</value>
</data>
<data name="EnlaceBackup" xml:space="preserve">
  <value>Backup</value>
</data>
```

No hace falta tocar el estilo (`operativ.css`): la página nueva reutiliza `.tarjeta`, `.tabla-operativ`, `.btn-primario`, `.btn-outline-peligro`, `.tarjeta-encabezado` tal como están.

---

## 8. Pasos para Claude Code

1. `git checkout main && git pull`
2. `git checkout -b feature/parche-2.1-backup-restore-base-datos`
3. Aplicar los archivos de las secciones 3 a 6 en orden (BE → DAL → SEC → Web).
4. Aplicar el cambio de seed y los Stored Procedures del script (sección 2), y recrear la base local.
5. Crear a mano la carpeta configurada en `Operativ.Backups.Carpeta` (por defecto `C:\OperativBackups\`) antes de probar, y confirmar que la cuenta que corre SQL Server Express tiene permiso de escritura ahí — si `webmaster` intenta un backup y tira `ERR21` con un mensaje de SQL sobre "Acceso denegado" o "No se puede abrir el dispositivo de backup", es este permiso.
6. Aplicar los recursos (sección 7).
7. Commits sugeridos:
   - `feat(db): agrega los stored procedures de backup y restore en master`
   - `feat(db): separa RealizarBackup en RealizarBackup y RestaurarBackup`
   - `feat(bll): agrega BackupService con manejo especifico de errores de SqlException`
   - `feat(web): agrega la pagina de backup y restore de la base de datos`
8. Build completo: `MSBuild.exe Operativ.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"`.
9. Pruebas manuales (con `webmaster`, que ya tiene `RealizarBackup`/`RestaurarBackup`/`RepararBaseDatos` por familia):
   - Loguearse como `webmaster` → el navbar muestra el enlace "Backup" → entrar a la página, la grilla arranca vacía con el mensaje "Todavía no se generó ningún backup."
   - Click en "Crear backup" → aparece en la grilla con fecha y tamaño; confirmar en el disco que el archivo `.bak` existe en la carpeta configurada.
   - Crear un segundo backup unos segundos después → la grilla lo muestra primero (orden descendente por fecha).
   - Loguearse como `admin` (sin patentes de Sistema) → intentar entrar directo por URL a `BackupRestore.aspx` → redirige a `NoAutorizado.aspx`; el enlace "Backup" no aparece en su navbar.
   - Como `webmaster`, click en "Restaurar" sobre el backup más viejo → confirmar el diálogo → la sesión se cierra y redirige a `Login.aspx` con el mensaje de éxito; loguearse de nuevo funciona normal y los datos de la base corresponden al momento de ese backup (por ejemplo, si se dio de alta un usuario de prueba entre el primer y el segundo backup, y se restaura el primero, ese usuario de prueba ya no debería estar).
   - Restaurar apuntando a un archivo `.bak` renombrado/corrupto a propósito → debe mostrar `ERR21` con el detalle del error de SQL Server, no un mensaje genérico de conexión.
   - Borrar a mano un archivo `.bak` del disco sin refrescar la página, e intentar restaurarlo desde la fila que quedó vieja en la grilla → debe mostrar `ERR22`.
   - En `PermisosUsuario.aspx`, abrir la categoría "Sistema" para cualquier usuario → deben aparecer las tres patentes (`RealizarBackup`, `RestaurarBackup`, `RepararBaseDatos`) con sus descripciones nuevas.
10. No mergear a `main`. Dejar la rama lista con los commits para que el dueño del repo la revise y haga el push.
