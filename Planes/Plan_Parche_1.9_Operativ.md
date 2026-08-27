# Plan: Desbloqueo de usuario por clave temporal y bloqueo manual desde Gestión de Usuarios — Operativ

**Repo:** `matias-milio-dev/TFI.Operativ.WebApp`, base `main` (HEAD auditado: `615d436` — "Use strategy pattern to handle login use cases").
**Rama a crear:** `feature/parche-1.9-desbloqueo-bloqueo-usuarios`.
**No hacer merge ni push a `main`.** Dejar los commits listos en la rama local para que el dueño del repo los revise y los suba él mismo.

Este documento fue armado por Claude auditando el código real del repo (no el de un plan anterior). Trae el diseño ya decidido y el contenido completo de cada archivo a tocar — no hace falta re-derivar el approach, solo aplicarlo. Donde diga "contenido completo del archivo", reemplazá el archivo entero por ese bloque.

Respetar en todo momento `Estandares_Codigo_y_Estilo_Operativ.md` y en particular:
- Sin comentarios `//` ni `/* */` ni XML docs.
- Sin `var`: tipos explícitos siempre.
- Sin tuplas, sin `record`, sin LINQ.
- `namespace X;` con scoped namespace, nunca con bloque `{ }`.
- Ifs siempre con llaves.
- Orden de miembros: campos/propiedades → constructor(es) → métodos public/protected/internal → métodos private. Nunca un método privado antes que uno público.
- Repositorios y servicios instanciados solo vía fábricas, nunca `new` suelto fuera de ellas.
- Acceso a datos: todo por `AccesoDatos` (`EjecutarReader`/`EjecutarConsulta`/`EjecutarEscalar`), `SqlParameter` parametrizado, sin Stored Procedures.
- Errores canalizados por `ErroresHandler`/`OperativException`/`TipoError`, formato `ERRXX - mensaje`.
- Auditoría de cada operación relevante vía `IBitacoraService.Registrar(...)`.

---

## 0. Correcciones del profesor a implementar

1. Cuando un login falla porque el usuario está bloqueado, el mensaje de error debe mostrar un botón **"Desbloquear usuario"** que lleva al mismo flujo de **Recuperar contraseña**. Al loguearse con la contraseña temporal enviada por mail, el sistema debe **reactivar (desbloquear)** al usuario automáticamente — tanto si se llegó a Recuperar Contraseña haciendo clic en ese botón como si se llegó por el enlace normal "¿Olvidó su contraseña?" del login.
   - **1.1** Inmediatamente después de loguearse con la contraseña temporal, el sistema debe pedir el cambio de contraseña (reutilizando el flujo de cambio de contraseña ya existente — `ModalCambiarClave`). Para saber cuándo pedirlo se agrega un campo nuevo en base de datos, `ContrasenaProvisoria` (bool), que indica si la contraseña actual del usuario es una temporal emitida por el sistema.
2. En el panel de `GestionUsuarios`, en el formulario de modificación de usuario, debe aparecer un botón nuevo **"Bloquear usuario"**, del mismo tamaño que el botón "Guardar" de arriba, que permite a un Administrador bloquear manualmente a un usuario activo (hoy solo se llega a "bloqueado" automáticamente, al tercer intento fallido de login).

---

## 1. Diseño y decisiones

### 1.1 Por qué el desbloqueo se gatea con `ContrasenaProvisoria`, no solo con "la contraseña es correcta"

`LoginNormalStrategy.ValidarCredenciales` hoy rechaza cualquier intento de login de un usuario bloqueado **antes** de validar la contraseña — ni siquiera con la contraseña correcta se puede volver a entrar; la única vía de desbloqueo es la acción manual del Administrador en `GestionUsuarios` (CU 001-006).

La corrección pide que el flujo de recuperación de contraseña también desbloquee. La implementación más simple sería "si la contraseña ingresada valida contra el hash actual, desbloquear" — pero eso tiene un problema de seguridad: si el usuario bloqueado prueba de nuevo con su contraseña **original** (la misma que ya conocía, sin pasar por recuperación) y por algún motivo acierta, quedaría desbloqueado sin haber demostrado su identidad por el canal fuera de banda (el mail). Eso reabre la puerta a probar contraseñas contra un usuario bloqueado.

Por eso el desbloqueo automático en el login se gatea con **ambas** condiciones: la contraseña ingresada tiene que validar contra el hash actual **y** ese hash tiene que estar marcado como `ContrasenaProvisoria = true` (es decir, fue emitido por `RecuperarContrasena`, no es la contraseña "de siempre" del usuario). Esto ata la corrección 1 a la 1.1: el campo nuevo de base de datos no es solo para decidir cuándo pedir el cambio de clave, también es la señal que autoriza el desbloqueo por este canal.

### 1.2 Dónde vive cada pieza

- **`ContrasenaProvisoria`**: columna nueva en `Usuario`, entidad BE, convertidor, y dos escrituras distintas en el repositorio (`ActualizarContrasena` la deja en `false`, para el cambio de clave voluntario vía `ModalCambiarClave`; `ActualizarContrasenaProvisoria` la deja en `true`, para `RecuperarContrasena`).
- **Botón "Desbloquear usuario" en el mensaje de login**: se agrega a `Notificaciones.ascx` (no a `Login.aspx`), porque el control ya recibe el `TipoError` al traducir la excepción — es el lugar correcto para decidir si corresponde mostrar la acción, sin que `Login.aspx.cs` tenga que volver a inspeccionar la excepción (rompería la centralización que ya se hizo en el parche de notificaciones). El link apunta a `RecuperarContrasena.aspx?usuario={nombreUsuario}`, y `RecuperarContrasena.aspx.cs` precarga ese nombre de usuario en el campo.
- **Pedido de cambio de clave post-login**: se agrega en `PaginaSeguraBase.OnInit`, después de `ValidarAcceso()`. Es el único lugar por el que pasan todas las páginas seguras (todas las Home y `GestionUsuarios`), así que alcanza con tocar un solo archivo para que el modal se abra automáticamente apenas el usuario llega a cualquier página protegida con `ContrasenaProvisoria = true` — sin importar si llegó ahí por el camino de "estaba bloqueado y se desbloqueó" o simplemente "recuperó la contraseña sin haber estado bloqueado nunca". Esto no bloquea la navegación (el modal se puede cerrar), pero se va a volver a abrir en cada página mientras la contraseña siga siendo provisoria — no se implementó un bloqueo duro de la UI porque no fue pedido y hubiera significado tocar todas las páginas seguras en vez de una sola.
- **Botón "Bloquear usuario"**: se agrega en `GestionUsuarios.aspx`, dentro del mismo `<div class="acciones-formulario">` donde está `btnGuardar`, con una clase CSS nueva (`btn-peligro`) que comparte exactamente las reglas de tamaño de `btn-primario` (mismo padding, tipografía y ancho al 100%) pero en color de alerta, para diferenciarlo visualmente de "Guardar" sin cambiarle el tamaño. Solo se muestra en modo edición (`idUsuario != 0`), nunca en alta. Al bloquear, la pantalla pasa al mismo panel de "Desbloqueo" que ya se usa cuando se edita un usuario que llegó bloqueado por intentos fallidos (reutiliza `MostrarPanelDesbloqueo`, cero UI nueva para ese caso).
- **Alta de usuario también marca `ContrasenaProvisoria = true`**: `UsuarioService.AltaUsuario` envía por mail una contraseña temporal igual que `RecuperarContrasena` — es una clave pensada para un solo ingreso, no para quedarse usándola. Por eso el `INSERT` de `UsuarioRepositorio.Insertar` marca `ContrasenaProvisoria = 1` para todo usuario nuevo (ver sección 4.2). Como consecuencia, cualquier usuario recién dado de alta va a ver el modal de cambio de contraseña apenas loguee por primera vez, igual que un usuario que recuperó su contraseña — mismo mecanismo de `PaginaSeguraBase`, sin código nuevo para este caso puntual.

### 1.3 Qué NO se toca (fuera de alcance)

- No se implementa un bloqueo duro de la UI mientras `ContrasenaProvisoria = true` (impedir navegar hasta cambiar la clave). El modal se reabre en cada página mientras la condición siga vigente, pero se puede cerrar y seguir navegando. Endurecer esto es una decisión de producto que no estaba en la corrección.
- El botón "Desbloquear usuario" que ya existía dentro de `GestionUsuarios` (flujo CU 001-006, admin desbloquea directo desde el panel de edición) **no se toca** — es una funcionalidad distinta a la de este parche (esa es para cuando el Administrador ya está mirando la ficha de un usuario bloqueado; la nueva es para el usuario final que intenta loguearse y está bloqueado).

---

## 2. Base de datos

### 2.1 `Scripts/CrearBaseDatos.sql` — cambios

En la definición de `CREATE TABLE Usuario`, agregar la columna nueva entre `IntentosFallidos` y `Activo`:

```sql
CREATE TABLE Usuario
(
    IdUsuario INT IDENTITY(1,1) NOT NULL,
    NombreUsuario VARCHAR(50) NOT NULL,
    Contrasena VARCHAR(200) NOT NULL,
    Salt VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    NombreCompleto VARCHAR(150) NOT NULL,
    Bloqueado BIT NOT NULL CONSTRAINT DF_Usuario_Bloqueado DEFAULT (0),
    IntentosFallidos INT NOT NULL CONSTRAINT DF_Usuario_IntentosFallidos DEFAULT (0),
    ContrasenaProvisoria BIT NOT NULL CONSTRAINT DF_Usuario_ContrasenaProvisoria DEFAULT (0),
    Activo BIT NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1),
    DVH BIGINT NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuario_NombreUsuario UNIQUE (NombreUsuario)
);
GO
```

Justo arriba de esa definición, agregar el mismo tipo de comentario que ya existe para `Bitacora` (nota para bases ya creadas con una versión anterior del script):

```sql
-- Para una base ya creada con una version anterior de este script, aplicar en su lugar:
-- ALTER TABLE Usuario ADD ContrasenaProvisoria BIT NOT NULL CONSTRAINT DF_Usuario_ContrasenaProvisoria DEFAULT (0);
CREATE TABLE Usuario
(
    ...
```

No hace falta tocar el `INSERT INTO Usuario (...)` de los usuarios semilla: al no listar la columna nueva en el `INSERT`, toma el default (`0`), que es el comportamiento correcto (ningún usuario semilla arranca con clave provisoria).

---

## 3. Capa BE

### 3.1 `Operativ.BE/Entidades/Usuario.cs` — contenido completo

```csharp
using System.Collections.Generic;

namespace Operativ.BE.Entidades;
public class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; }

    public string Contrasena { get; set; }

    public string Salt { get; set; }

    public string Email { get; set; }

    public string NombreCompleto { get; set; }

    public bool Bloqueado { get; set; }

    public int IntentosFallidos { get; set; }

    public bool ContrasenaProvisoria { get; set; }

    public bool Activo { get; set; }

    public List<Familia> Familias { get; set; }

    public string NombreFamilia
    {
        get { return Familias.Count > 0 ? Familias[0].Nombre : null; }
    }

    public Usuario()
    {
        Familias = new List<Familia>();
    }
}
```

### 3.2 `Operativ.BE/Enums/TipoAccionBitacora.cs` — contenido completo

Se agrega `BloqueoManualUsuario` para diferenciar en la bitácora un bloqueo hecho a mano por un Administrador del bloqueo automático por 3 intentos fallidos (`LoginBloqueado`).

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
    BloqueoManualUsuario
}
```

### 3.3 `Operativ.BE/Modelos/AccionBitacora.cs` — contenido completo

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
            BloqueoManualUsuario
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

---

## 4. Capa DAL

### 4.1 `Operativ.DAL/Contratos/IUsuarioRepositorio.cs` — contenido completo

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

    List<Usuario> Listar(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);

    int ContarUsuarios(string filtro, int? idFamilia);

    bool ExisteNombreUsuario(string nombreUsuario, int? idUsuarioExcluir);

    bool ExisteEmail(string correoElectronico, int? idUsuarioExcluir);
}
```

### 4.2 `Operativ.DAL/Implementaciones/UsuarioRepositorio.cs` — contenido completo

`ActualizarContrasena` y `ActualizarContrasenaProvisoria` comparten el mismo UPDATE parametrizado, solo cambia el valor de `ContrasenaProvisoria` — se extrae el paso atómico compartido a un método privado (`EjecutarActualizacionContrasena`), siguiendo el mismo criterio que ya usa el repo en `IntegridadHelper` (sección 4.2 de los estándares).

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
public class UsuarioRepositorio : IUsuarioRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public UsuarioRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public Usuario GetPorNombreUsuario(string nombreUsuario)
    {
        string consulta = "SELECT IdUsuario, NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, ContrasenaProvisoria, Activo "
            + "FROM Usuario WHERE NombreUsuario = @NombreUsuario AND Activo = 1";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreUsuario", nombreUsuario)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Usuario usuario = null;

        if (tabla.Rows.Count > 0)
        {
            usuario = tabla.Rows[0].ToUsuario();
        }

        return usuario;
    }

    public Usuario GetPorId(int idUsuario)
    {
        string consulta = "SELECT IdUsuario, NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, ContrasenaProvisoria, Activo "
            + "FROM Usuario WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Usuario usuario = null;

        if (tabla.Rows.Count > 0)
        {
            usuario = tabla.Rows[0].ToUsuario();
        }

        return usuario;
    }

    public void ActualizarIntentosFallidos(int idUsuario, int intentosFallidos, bool bloqueado)
    {
        string consulta = "UPDATE Usuario SET IntentosFallidos = @IntentosFallidos, Bloqueado = @Bloqueado WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IntentosFallidos", intentosFallidos),
            new SqlParameter("@Bloqueado", bloqueado),
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    public void ActualizarContrasena(int idUsuario, string contrasena, string salt)
    {
        EjecutarActualizacionContrasena(idUsuario, contrasena, salt, false);
    }

    public void ActualizarContrasenaProvisoria(int idUsuario, string contrasena, string salt)
    {
        EjecutarActualizacionContrasena(idUsuario, contrasena, salt, true);
    }

    public void ResetearIntentosFallidos(int idUsuario)
    {
        string consulta = "UPDATE Usuario SET IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    public void Desbloquear(int idUsuario)
    {
        string consulta = "UPDATE Usuario SET Bloqueado = 0, IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    public void Bloquear(int idUsuario)
    {
        string consulta = "UPDATE Usuario SET Bloqueado = 1 WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    public int Insertar(Usuario usuario)
    {
        string consulta = "INSERT INTO Usuario (NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, ContrasenaProvisoria, Activo) "
            + "VALUES (@NombreUsuario, @Contrasena, @Salt, @Email, @NombreCompleto, 0, 0, 1, 1); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreUsuario", usuario.NombreUsuario),
            new SqlParameter("@Contrasena", usuario.Contrasena),
            new SqlParameter("@Salt", usuario.Salt),
            new SqlParameter("@Email", usuario.Email),
            new SqlParameter("@NombreCompleto", usuario.NombreCompleto)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idUsuario = Convert.ToInt32(resultado);
        ActualizarDVH(idUsuario);
        return idUsuario;
    }

    public void Modificar(Usuario usuario)
    {
        string consulta = "UPDATE Usuario SET NombreCompleto = @NombreCompleto, Email = @Email WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreCompleto", usuario.NombreCompleto),
            new SqlParameter("@Email", usuario.Email),
            new SqlParameter("@IdUsuario", usuario.IdUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(usuario.IdUsuario);
    }

    public void BajaLogica(int idUsuario)
    {
        string consulta = "UPDATE Usuario SET Activo = 0 WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    public void AsignarFamilia(int idUsuario, int idFamilia)
    {
        string consulta = "INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@IdUsuario, @IdFamilia)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdFamilia", idFamilia)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);

        List<SqlParameter> clavesFila = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdFamilia", idFamilia)
        };

        IntegridadHelper.ActualizarIntegridadClaveCompuesta("UsuarioFamilia", clavesFila);
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Usuario", "IdUsuario", id);
    }

    public List<Usuario> Listar(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT U.IdUsuario, U.NombreUsuario, U.Contrasena, U.Salt, U.Email, U.NombreCompleto, U.Bloqueado, U.IntentosFallidos, U.ContrasenaProvisoria, U.Activo, "
            + "F.IdFamilia, F.Nombre AS NombreFamilia "
            + "FROM Usuario U "
            + "LEFT JOIN UsuarioFamilia UF ON UF.IdUsuario = U.IdUsuario "
            + "LEFT JOIN Familia F ON F.IdFamilia = UF.IdFamilia "
            + "WHERE U.Activo = 1 "
            + "AND (@Filtro = '' OR U.NombreUsuario LIKE '%' + @Filtro + '%' OR U.Email LIKE '%' + @Filtro + '%') "
            + (idFamilia.HasValue ? "AND UF.IdFamilia = @IdFamilia " : string.Empty)
            + "ORDER BY U.NombreUsuario "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        if (idFamilia.HasValue)
        {
            parametros.Add(new SqlParameter("@IdFamilia", idFamilia.Value));
        }

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaUsuariosConFamilia();
    }

    public int ContarUsuarios(string filtro, int? idFamilia)
    {
        string consulta = "SELECT COUNT(*) FROM Usuario U "
            + (idFamilia.HasValue ? "INNER JOIN UsuarioFamilia UF ON UF.IdUsuario = U.IdUsuario " : string.Empty)
            + "WHERE U.Activo = 1 "
            + "AND (@Filtro = '' OR U.NombreUsuario LIKE '%' + @Filtro + '%' OR U.Email LIKE '%' + @Filtro + '%') "
            + (idFamilia.HasValue ? "AND UF.IdFamilia = @IdFamilia " : string.Empty);

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty)
        };

        if (idFamilia.HasValue)
        {
            parametros.Add(new SqlParameter("@IdFamilia", idFamilia.Value));
        }

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public bool ExisteNombreUsuario(string nombreUsuario, int? idUsuarioExcluir)
    {
        string consulta = "SELECT COUNT(*) FROM Usuario "
            + "WHERE NombreUsuario = @NombreUsuario "
            + "AND (@IdUsuarioExcluir IS NULL OR IdUsuario <> @IdUsuarioExcluir)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreUsuario", nombreUsuario),
            new SqlParameter("@IdUsuarioExcluir", (object)idUsuarioExcluir ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }

    public bool ExisteEmail(string correoElectronico, int? idUsuarioExcluir)
    {
        string consulta = "SELECT COUNT(*) FROM Usuario "
            + "WHERE Email = @Email "
            + "AND (@IdUsuarioExcluir IS NULL OR IdUsuario <> @IdUsuarioExcluir)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Email", correoElectronico),
            new SqlParameter("@IdUsuarioExcluir", (object)idUsuarioExcluir ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }

    private void EjecutarActualizacionContrasena(int idUsuario, string contrasena, string salt, bool contrasenaProvisoria)
    {
        string consulta = "UPDATE Usuario SET Contrasena = @Contrasena, Salt = @Salt, ContrasenaProvisoria = @ContrasenaProvisoria WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Contrasena", contrasena),
            new SqlParameter("@Salt", salt),
            new SqlParameter("@ContrasenaProvisoria", contrasenaProvisoria),
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }
}
```

`Insertar` ya hardcodea `Bloqueado = 0`, `IntentosFallidos = 0` y `Activo = 1` como literales del `INSERT` en vez de leerlos del objeto `Usuario` recibido (el objeto que arma `UsuarioService.AltaUsuario` ni siquiera setea esas propiedades, quedan en su default de C#). `ContrasenaProvisoria = 1` sigue exactamente ese mismo patrón: todo usuario nuevo arranca con la contraseña temporal que le mandó el mail de bienvenida, así que no hace falta ni un parámetro nuevo ni tocar `UsuarioService.Abm.cs` — el alta de usuario queda cubierta gratis por este único cambio en el `INSERT`.

### 4.3 `Operativ.DAL/Convertidores/UsuarioConvertidor.cs` — contenido completo

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores;
public static class UsuarioConvertidor
{
    public static Usuario ToUsuario(this DataRow fila)
    {
        Usuario usuario = new Usuario
        {
            IdUsuario = (int)fila["IdUsuario"],
            NombreUsuario = fila["NombreUsuario"].ToString(),
            Contrasena = fila["Contrasena"].ToString(),
            Salt = fila["Salt"].ToString(),
            Email = fila["Email"].ToString(),
            NombreCompleto = fila["NombreCompleto"].ToString(),
            Bloqueado = (bool)fila["Bloqueado"],
            IntentosFallidos = (int)fila["IntentosFallidos"],
            ContrasenaProvisoria = (bool)fila["ContrasenaProvisoria"],
            Activo = (bool)fila["Activo"]
        };
        return usuario;
    }

    public static Usuario ToUsuarioConFamilia(this DataRow fila)
    {
        Usuario usuario = fila.ToUsuario();

        if (fila["IdFamilia"] != DBNull.Value)
        {
            Familia familia = new Familia
            {
                IdFamilia = (int)fila["IdFamilia"],
                Nombre = fila["NombreFamilia"].ToString()
            };

            usuario.Familias.Add(familia);
        }

        return usuario;
    }

    public static List<Usuario> ToListaUsuariosConFamilia(this DataTable tabla)
    {
        List<Usuario> usuarios = new List<Usuario>();

        foreach (DataRow fila in tabla.Rows)
        {
            usuarios.Add(fila.ToUsuarioConFamilia());
        }

        return usuarios;
    }
}
```

---

## 5. Capa SEC

### 5.1 `Operativ.SEC/Contratos/IUsuarioService.cs` — contenido completo

```csharp
using System.Collections.Generic;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Contratos;
public interface IUsuarioService
{
    void RecuperarContrasena(string nombreUsuario);

    void CambiarClave(int idUsuario, string claveActual, string claveNueva);

    int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int idFamilia);

    void ModificarUsuario(Usuario usuario);

    void BajaUsuario(int idUsuario);

    void DesbloquearUsuario(int idUsuario);

    void BloquearUsuario(int idUsuario);

    Usuario ObtenerUsuarioPorId(int idUsuario);

    List<Usuario> ListarUsuarios(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);

    int ContarUsuarios(string filtro, int? idFamilia);
}
```

### 5.2 `Operativ.SEC/Implementaciones/UsuarioService.cs` — contenido completo

Único cambio real de comportamiento: `RecuperarContrasena` ahora llama a `ActualizarContrasenaProvisoria` en vez de `ActualizarContrasena`. Se agrega `BloquearUsuario`, simétrico a `DesbloquearUsuario`.

```csharp
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones;
public partial class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepositorio usuarioRepositorio;
    private readonly IFamiliaRepositorio familiaRepositorio;
    private readonly IBitacoraService bitacoraService;

    public UsuarioService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();
        familiaRepositorio = fabricaRepositorio.CrearFamiliaRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public void RecuperarContrasena(string nombreUsuario)
    {
        Usuario usuario = GetUsuarioExistente(nombreUsuario);
        string contrasenaTemporal = ClaveHelper.GenerarContrasenaTemporal();
        string nuevoSalt = HashHelper.GenerarSalt();
        string nuevoHash = HashHelper.GenerarHash(contrasenaTemporal, nuevoSalt);

        EmailHelper.EnviarContrasenaTemporal(usuario.Email, usuario.NombreUsuario, contrasenaTemporal);
        usuarioRepositorio.ActualizarContrasenaProvisoria(usuario.IdUsuario, nuevoHash, nuevoSalt);
        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.RecuperacionContrasena);
    }

    public void CambiarClave(int idUsuario, string claveActual, string claveNueva)
    {
        Usuario usuario = usuarioRepositorio.GetPorId(idUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);

        bool claveActualValida = HashHelper.ValidarContrasena(claveActual, usuario.Salt, usuario.Contrasena);

        if (!claveActualValida)
        {
            throw new OperativException(TipoError.ErrorContrasenaActualIncorrecta);
        }

        if (!ClaveHelper.EsCompleja(claveNueva))
        {
            throw new OperativException(TipoError.ErrorClaveNoCumpleComplejidad);
        }

        string nuevoSalt = HashHelper.GenerarSalt();
        string nuevoHash = HashHelper.GenerarHash(claveNueva, nuevoSalt);

        usuarioRepositorio.ActualizarContrasena(idUsuario, nuevoHash, nuevoSalt);
        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.CambioClave);
    }

    public void DesbloquearUsuario(int idUsuario)
    {
        usuarioRepositorio.Desbloquear(idUsuario);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.DesbloqueoUsuario);
    }

    public void BloquearUsuario(int idUsuario)
    {
        usuarioRepositorio.Bloquear(idUsuario);

        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.BloqueoManualUsuario);
    }

    private Usuario GetUsuarioExistente(string nombreUsuario)
    {
        Usuario usuario = usuarioRepositorio.GetPorNombreUsuario(nombreUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);
        return usuario;
    }
}
```

### 5.3 `Operativ.SEC/Implementaciones/Estrategias/LoginNormalStrategy.cs` — contenido completo

Cambio central de la corrección 1: `ValidarCredenciales` deja de rechazar de entrada a un usuario bloqueado. Ahora valida la contraseña primero; si el usuario está bloqueado, solo lo desbloquea cuando la contraseña es correcta **y** es la contraseña provisoria emitida por `RecuperarContrasena` (ver sección 1.1 de este plan).

```csharp
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BE.Modelos.Composite;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones.Estrategias;
public class LoginNormalStrategy : ILoginStrategy
{
    private readonly IUsuarioRepositorio usuarioRepositorio;
    private readonly IFamiliaService familiaService;
    private readonly IBitacoraService bitacoraService;

    public LoginNormalStrategy()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();

        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        familiaService = fabricaSeguridad.CrearFamiliaService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    public ResultadoAutenticacion Autenticar(string nombreUsuario, string contrasena)
    {
        Usuario usuario = ValidarCredenciales(nombreUsuario, contrasena);

        Familia perfil = familiaService.GetPerfilDeUsuario(usuario.IdUsuario);
        FamiliaCompuesto arbolPermisos = familiaService.ArmarArbolPermisos(usuario.IdUsuario);

        return new ResultadoAutenticacion
        {
            Usuario = usuario,
            Perfil = perfil,
            ArbolPermisos = arbolPermisos
        };
    }

    private Usuario ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        Usuario usuario = usuarioRepositorio.GetPorNombreUsuario(nombreUsuario)
            ?? throw new OperativException(TipoError.ErrorUsuarioNoExiste);

        bool contrasenaValida = HashHelper.ValidarContrasena(contrasena, usuario.Salt, usuario.Contrasena);

        if (usuario.Bloqueado)
        {
            ValidarDesbloqueoPorClaveTemporal(usuario, contrasenaValida);
        }
        else if (!contrasenaValida)
        {
            ManejarIntentoFallido(usuario);
        }
        else
        {
            usuarioRepositorio.ResetearIntentosFallidos(usuario.IdUsuario);
            usuario.IntentosFallidos = 0;
        }

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginExitoso);

        return usuario;
    }

    private void ValidarDesbloqueoPorClaveTemporal(Usuario usuario, bool contrasenaValida)
    {
        if (!contrasenaValida || !usuario.ContrasenaProvisoria)
        {
            throw new OperativException(TipoError.ErrorUsuarioBloqueado, new string[] { usuario.NombreUsuario });
        }

        usuarioRepositorio.Desbloquear(usuario.IdUsuario);
        usuario.Bloqueado = false;
        usuario.IntentosFallidos = 0;

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.DesbloqueoUsuario);
    }

    private void ManejarIntentoFallido(Usuario usuario)
    {
        int intentosFallidos = usuario.IntentosFallidos + 1;
        bool bloqueado = intentosFallidos >= ConfiguracionAplicacion.IntentosMaximosLogin;

        usuarioRepositorio.ActualizarIntentosFallidos(usuario.IdUsuario, intentosFallidos, bloqueado);

        if (bloqueado)
        {
            bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginBloqueado);
            throw new OperativException(TipoError.ErrorUsuarioBloqueado, new string[] { usuario.NombreUsuario });
        }

        bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.IntentoLoginFallido);
        int intentosRestantes = ConfiguracionAplicacion.IntentosMaximosLogin - intentosFallidos;
        throw new OperativException(TipoError.ErrorContrasenaIncorrecta, new string[] { intentosRestantes.ToString() });
    }
}
```

No se toca `LoginEmergenciaStrategy.cs` — el acceso de emergencia no pasa por la tabla `Usuario`.

---

## 6. Capa Web

### 6.1 `Operativ.Web/Paginas/Controles/Notificaciones.ascx` — contenido completo

Se agrega el link de acción, oculto por defecto.

```html
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Notificaciones.ascx.cs" Inherits="Operativ.Web.Controles.Notificaciones" %>
<asp:Panel ID="pnlNotificacion" runat="server" CssClass="notificacion" Visible="false">
    <asp:Label ID="lblMensaje" runat="server" />
    <div class="notificacion-accion">
        <asp:HyperLink ID="lnkDesbloquearUsuario" runat="server" CssClass="btn-primario" Visible="false" Text="<%$ Resources:Textos, BotonDesbloquearUsuario %>" />
    </div>
</asp:Panel>
```

### 6.2 `Operativ.Web/Paginas/Controles/Notificaciones.ascx.designer.cs` — contenido completo

```csharp
namespace Operativ.Web.Controles;
public partial class Notificaciones
{
    protected global::System.Web.UI.WebControls.Panel pnlNotificacion;

    protected global::System.Web.UI.WebControls.Label lblMensaje;

    protected global::System.Web.UI.WebControls.HyperLink lnkDesbloquearUsuario;
}
```

### 6.3 `Operativ.Web/Paginas/Controles/Notificaciones.ascx.cs` — contenido completo

`MostrarMensaje(Exception)` ahora, además de traducir el mensaje, decide si corresponde mostrar el link de desbloqueo (solo cuando el error es `ErrorUsuarioBloqueado`, tomando el nombre de usuario del primer parámetro de la excepción). `MostrarMensaje(string, bool)` — el punto único de renderizado — apaga el link por defecto en cada llamada, así no queda pegado de un mensaje anterior.

```csharp
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Operativ.BE.Enums;
using Operativ.BE.Errores;

namespace Operativ.Web.Controles;
public partial class Notificaciones : UserControl
{
    private static readonly Regex PrefijoCodigoError = new Regex(@"^ERR\d+\s*-\s*");

    private readonly ErroresHandler erroresHandler = new ErroresHandler();

    public void MostrarMensaje(Exception excepcion)
    {
        OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
        MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
        MostrarEnlaceDesbloqueoSiCorresponde(excepcionOperativ);
    }

    public void MostrarMensaje(TipoError tipoError)
    {
        MostrarMensaje(erroresHandler.GetMensaje(tipoError));
    }

    public void MostrarMensaje(TipoError tipoError, string[] parametros)
    {
        MostrarMensaje(erroresHandler.GetMensaje(tipoError, parametros));
    }

    public void MostrarExito(string claveRecurso)
    {
        string mensaje = (string)GetGlobalResourceObject("Textos", claveRecurso);
        MostrarMensaje(mensaje, true);
    }

    public void MostrarMensaje(string mensaje)
    {
        MostrarMensaje(mensaje, false);
    }

    public void MostrarMensaje(string mensaje, bool esExito)
    {
        pnlNotificacion.Visible = true;
        pnlNotificacion.CssClass = esExito ? "notificacion notificacion-exito" : "notificacion notificacion-error";
        lblMensaje.Text = PrefijoCodigoError.Replace(mensaje, string.Empty);
        lnkDesbloquearUsuario.Visible = false;
    }

    private void MostrarEnlaceDesbloqueoSiCorresponde(OperativException excepcionOperativ)
    {
        if (excepcionOperativ.TipoError != TipoError.ErrorUsuarioBloqueado)
        {
            return;
        }

        string nombreUsuario = excepcionOperativ.Parametros[0];
        lnkDesbloquearUsuario.NavigateUrl = "~/Paginas/Usuarios/RecuperarContrasena.aspx?usuario=" + HttpUtility.UrlEncode(nombreUsuario);
        lnkDesbloquearUsuario.Visible = true;
    }
}
```

### 6.4 `Operativ.Web/Paginas/Usuarios/RecuperarContrasena.aspx.cs` — contenido completo

Se agrega `Page_Load` para precargar el nombre de usuario cuando se llega desde el link `?usuario=...` (tanto desde el botón nuevo de `Notificaciones` como si alguien arma el link a mano).

```csharp
using System;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class RecuperarContrasena : PaginaBase
{
    private readonly IUsuarioService usuarioService;

    public RecuperarContrasena()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            txtNombreUsuario.Text = Request.QueryString["usuario"];
        }
    }

    protected void btnEnviar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            usuarioService.RecuperarContrasena(txtNombreUsuario.Text.Trim());
            ucNotificaciones.MostrarExito("MensajeExitoRecuperacionContrasena");
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }
}
```

`RecuperarContrasena.aspx` (el markup) y `RecuperarContrasena.aspx.designer.cs` no se tocan.

### 6.5 `Operativ.Web/Paginas/PaginaSeguraBase.cs` — contenido completo

Este es el punto único por el que pasan todas las páginas protegidas (todas las Home y `GestionUsuarios`). Después de validar el acceso, si el usuario logueado tiene `ContrasenaProvisoria = true`, se registra un startup script que abre el modal de cambio de contraseña ya existente (`Operativ.abrirModalCambiarClave()`, definido en `operativ-ui.js`, ya soporta llamarse sin evento).

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
        AbrirCambioClaveSiEsProvisoria();
    }

    private void ValidarAcceso()
    {
        if (!SesionHandler.HaySesionActiva())
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?err=sesion");
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

### 6.6 `Operativ.Web/Paginas/Controles/ModalCambiarClave.ascx.cs` — contenido completo

Único cambio: al cambiar la clave con éxito, se apaga `ContrasenaProvisoria` en el objeto `Usuario` que ya está en sesión (la persistencia en base la hace `UsuarioService.CambiarClave` llamando a `ActualizarContrasena`, que ya deja la columna en `false`; esto sincroniza el objeto cacheado en `Session` para que `PaginaSeguraBase` dejar de reabrir el modal en la próxima página).

```csharp
using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Master;

namespace Operativ.Web.Controles;
public partial class ModalCambiarClave : UserControl
{
    private readonly IUsuarioService usuarioService;

    private Notificaciones ControlNotificaciones
    {
        get { return ((Principal)Page.Master).ControlNotificaciones; }
    }

    public ModalCambiarClave()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
    }

    protected void btnGuardarClave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        SesionHandler sesionHandler = new SesionHandler();
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario == null)
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?err=sesion");
            return;
        }

        try
        {
            usuarioService.CambiarClave(usuario.IdUsuario, txtContrasenaActual.Text, txtContrasenaNueva.Text);
            usuario.ContrasenaProvisoria = false;
            ControlNotificaciones.MostrarExito("MensajeExitoCambioClave");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
        finally
        {
            txtContrasenaActual.Text = string.Empty;
            txtContrasenaNueva.Text = string.Empty;
            txtContrasenaConfirmar.Text = string.Empty;
        }
    }
}
```

### 6.7 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx` — contenido completo

Se agrega `btnBloquear` dentro del mismo `.acciones-formulario` que `btnGuardar`, con la clase nueva `btn-peligro` (mismo tamaño que `btn-primario`, ver sección 7). Arranca oculto (`Visible="false"`); el code-behind lo muestra solo en modo edición.

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
            DataKeyNames="IdUsuario" OnRowCommand="gvUsuarios_RowCommand" GridLines="None">
            <Columns>
                <asp:BoundField DataField="NombreUsuario" HeaderText="<%$ Resources:Textos, EtiquetaNombreUsuario %>" />
                <asp:BoundField DataField="NombreCompleto" HeaderText="<%$ Resources:Textos, EtiquetaNombreCompleto %>" />
                <asp:BoundField DataField="Email" HeaderText="<%$ Resources:Textos, EtiquetaCorreoElectronico %>" />
                <asp:BoundField DataField="NombreFamilia" HeaderText="<%$ Resources:Textos, EtiquetaFamilia %>" />
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
                    <asp:RequiredFieldValidator ID="rfvFamilia" runat="server" ControlToValidate="ddlFamilia" InitialValue=""
                        ErrorMessage="<%$ Resources:Textos, MensajeValidacionFamiliaObligatoria %>" CssClass="texto-validacion" Display="Dynamic" />
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

### 6.8 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx.designer.cs` — contenido completo

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

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvFamilia;

    protected global::System.Web.UI.WebControls.LinkButton btnGuardar;

    protected global::System.Web.UI.WebControls.LinkButton btnBloquear;

    protected global::System.Web.UI.WebControls.LinkButton btnCancelar;

    protected global::System.Web.UI.WebControls.ValidationSummary vsGestionUsuarios;
}
```

### 6.9 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx.cs` — contenido completo

Se agrega `btnBloquear_Click` (junto a los demás handlers `protected`, después de `btnDesbloquear_Click`) y se ajustan `PrepararAlta`/`MostrarPanelEdicion` para mostrar/ocultar el botón nuevo según el modo del formulario.

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
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

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            int idUsuario = Convert.ToInt32(hidIdUsuario.Value);
            int idFamilia = Convert.ToInt32(ddlFamilia.SelectedValue);

            if (idUsuario == 0)
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

                usuarioService.ModificarUsuario(usuario);
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

    private void DarDeBaja(int idUsuario)
    {
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

No se toca `Login.aspx` ni `Login.aspx.cs`: el botón nuevo vive en `Notificaciones.ascx`, que `Login.aspx` ya incluye, y `Login.aspx.cs` ya delega en `ucNotificaciones.MostrarMensaje(excepcion)`.

---

## 7. Recursos (`Textos.resx` / `Textos.en.resx`)

`BotonDesbloquearUsuario` **ya existe** en ambos archivos ("Desbloquear usuario" / "Unlock user") y se reutiliza tal cual para el link nuevo de `Notificaciones.ascx` — no hace falta agregar una clave nueva para eso.

Agregar en `Operativ.Web/App_GlobalResources/Textos.resx`, junto a las demás claves de `GestionUsuarios` (por ejemplo después de `BotonDarBaja`):

```xml
<data name="BotonBloquearUsuario" xml:space="preserve">
  <value>Bloquear usuario</value>
</data>
```

Y junto a los demás `MensajeExito*` (por ejemplo después de `MensajeExitoBajaUsuario`):

```xml
<data name="MensajeExitoBloqueoUsuario" xml:space="preserve">
  <value>Usuario bloqueado correctamente.</value>
</data>
```

Agregar en `Operativ.Web/App_GlobalResources/Textos.en.resx`, en los mismos lugares relativos:

```xml
<data name="BotonBloquearUsuario" xml:space="preserve">
  <value>Block user</value>
</data>
```

```xml
<data name="MensajeExitoBloqueoUsuario" xml:space="preserve">
  <value>User blocked successfully.</value>
</data>
```

El texto de confirmación del `OnClientClick` de `btnBloquear` (`'¿Confirma que desea bloquear a este usuario?'`) queda hardcodeado en el `.aspx`, igual que el de `lnkBaja` — no se resuelve por recurso porque el repo ya tiene esa misma inconsistencia en el botón de baja; no se introduce un patrón nuevo, se sigue el existente.

---

## 8. Estilos (`Operativ.Web/Estilos/operativ.css`)

Se agrega la clase `btn-peligro`: mismo modelo de caja que `btn-primario`/`btn-outline`/`btn-outline-peligro` (mismo padding, tipografía y tamaño de ícono), pero en color de error, para que el botón "Bloquear usuario" tenga **exactamente el mismo tamaño** que "Guardar" sin ser del mismo color.

Sumar `.btn-peligro` a los dos selectores compartidos que ya agrupan `.btn-primario, .btn-outline, .btn-outline-peligro`:

```css
.btn-primario,
.btn-outline,
.btn-outline-peligro,
.btn-peligro {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border-radius: var(--radio-boton);
    padding: 10px 16px;
    font-size: 14px;
    font-weight: 600;
    font-family: inherit;
    cursor: pointer;
    text-decoration: none;
    white-space: nowrap;
}

.btn-primario svg,
.btn-outline svg,
.btn-outline-peligro svg,
.btn-peligro svg {
    width: 16px;
    height: 16px;
    flex-shrink: 0;
}
```

Agregar el color propio, cerca del bloque de `.btn-outline-peligro`:

```css
.btn-peligro {
    background-color: var(--color-error);
    color: #ffffff;
    border: 1px solid var(--color-error);
}

.btn-peligro:hover {
    background-color: #a92020;
}
```

Extender la regla que hace full-width a los botones dentro de `.acciones-formulario` para que también aplique a `.btn-peligro` (así "Guardar" y "Bloquear usuario" quedan exactamente del mismo ancho y alto, apilados verticalmente dentro del mismo contenedor):

```css
.acciones-formulario .btn-primario,
.acciones-formulario .btn-peligro {
    width: 100%;
    justify-content: center;
}
```

Agregar un espaciado chico para el link nuevo dentro de `Notificaciones.ascx` (después del bloque `.notificacion-advertencia` es un buen lugar):

```css
.notificacion-accion {
    margin-top: 10px;
}
```

---

## 9. Pasos para Claude Code

1. `git checkout main && git pull`
2. `git checkout -b feature/parche-1.9-desbloqueo-bloqueo-usuarios`
3. Aplicar el cambio de `Scripts/CrearBaseDatos.sql` (sección 2) y recrear la base local (`sqlcmd` o SSMS contra el script completo) para probar de punta a punta.
4. Aplicar los archivos completos de las secciones 3 a 6 (BE, DAL, SEC, Web).
5. Aplicar los agregados de recursos (sección 7) y de estilos (sección 8).
6. Commits sugeridos (separados, para que el diff sea fácil de revisar):
   - `feat(db): agrega columna ContrasenaProvisoria a Usuario`
   - `feat(bll): permite desbloquear un usuario en el login con la clave temporal de recuperacion`
   - `feat(web): agrega boton Desbloquear usuario en el mensaje de login bloqueado`
   - `feat(web): fuerza el cambio de contrasena cuando la actual es provisoria`
   - `feat(web): agrega boton Bloquear usuario en GestionUsuarios`
7. Build completo: `MSBuild.exe Operativ.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"`.
8. Si compila, levantar IIS Express y probar a mano (usar los 4 usuarios semilla del script; para forzar bloqueo, fallar el login 3 veces con `cliente`, por ejemplo):
   - Login con `cliente` bloqueado + contraseña incorrecta → `ERR03` y aparece el botón "Desbloquear usuario"; al hacer clic, redirige a `RecuperarContrasena.aspx?usuario=cliente` con el campo ya completo.
   - Enviar la recuperación → llega el mail (o queda en Papercut/smtp4dev) con la clave temporal; en la base, `cliente` queda con `ContrasenaProvisoria = 1` y `Bloqueado` sigue en `1`.
   - Loguearse con `cliente` y la clave temporal → login exitoso, `Bloqueado` pasa a `0`, `IntentosFallidos` a `0`, bitácora registra `DesbloqueoUsuario`.
   - Apenas cae en la Home, se abre solo el modal "Cambiar contraseña".
   - Cambiar la contraseña desde el modal → éxito, `ContrasenaProvisoria` pasa a `0`; navegar a otra página no debe volver a abrir el modal.
   - Repetir recuperación de contraseña con un usuario que **no** esté bloqueado (por ejemplo `comercial`) → después de loguearse con la clave temporal, también debe pedirse el cambio de clave (confirma que el gateo no depende de `Bloqueado`).
   - Con un usuario bloqueado, intentar loguearse con una contraseña incorrecta que no sea la temporal recién emitida → debe seguir devolviendo `ERR03` (no debe desbloquear).
   - En `GestionUsuarios`, editar un usuario activo y no bloqueado (`admin`, por ejemplo, con otro usuario de prueba) → debe aparecer "Bloquear usuario" del mismo tamaño que "Guardar"; confirmar, bloquear, ver el mensaje de éxito y que la pantalla pasa al panel de "Desbloqueo"; la grilla debe mostrar el badge "Bloqueado".
   - En modo Alta (formulario vacío) → "Bloquear usuario" no debe aparecer.
   - Dar de alta un usuario nuevo desde `GestionUsuarios` → en la base queda con `ContrasenaProvisoria = 1`; al loguearse por primera vez con la clave temporal del mail de bienvenida, debe pedirse el cambio de contraseña igual que en el flujo de recuperación (sin haber estado bloqueado en ningún momento).
9. No mergear a `main`. Dejar la rama lista con los commits para que el dueño del repo la revise y haga el push.
