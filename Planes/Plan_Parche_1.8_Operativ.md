# Plan: Centralizar el manejo de notificaciones y reordenar miembros — Operativ

**Repo:** `matias-milio-dev/TFI.Operativ.WebApp`, base `main`.
**Rama a crear:** `refactor/notificaciones-centralizadas`.
**No hacer merge ni push a `main`.** Dejar los commits listos en la rama local para que el dueño del repo los revise y los suba él mismo.

Este documento fue armado por Claude (conversación previa) auditando el código real del repo. Trae el diseño ya decidido y el contenido completo de cada archivo a tocar — no hace falta re-derivar el approach, solo aplicarlo. Donde diga "contenido completo del archivo", reemplazá el archivo entero por ese bloque (no un diff parcial).

Respetar en todo momento `Planes/` (si existe un documento de estándares de código en el repo o en el material del proyecto) y en particular:
- Sin comentarios `//` ni `/* */` ni XML docs.
- Sin `var`: tipos explícitos siempre.
- Sin tuplas, sin `record`, sin LINQ.
- `namespace X;` con scoped namespace, nunca con bloque `{ }`.
- Ifs siempre con llaves.
- Campos privados `private readonly` sin guion bajo (`erroresHandler`, no `_erroresHandler`).
- Nunca `this.`.
- No encadenar métodos sobre un objeto recién creado (asignar a variable primero).

---

## 0. Objetivo

Hoy, muchos code-behinds hacen esto para mostrar un error:

```csharp
OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
```

o esto para mostrar un éxito:

```csharp
string mensaje = (string)GetGlobalResourceObject("Textos", claveRecurso);
ucNotificaciones.MostrarMensaje(mensaje, true);
```

El objetivo es que el control `Notificaciones.ascx.cs` absorba esa lógica, y que los code-behinds queden así:

```csharp
ucNotificaciones.MostrarMensaje(excepcion);
```
```csharp
ucNotificaciones.MostrarExito("MensajeExitoRecuperacionContrasena");
```

Además:
- El catch-all de `ErroresHandler.TraducirExcepcion` debe devolver un `TipoError` genérico nuevo (`FalloNoManejadoGenerico`), no `ErrorConexionBaseDatos` como hoy (es semánticamente incorrecto para una excepción no reconocida).
- Todas las clases y code-behinds del repo deben quedar organizadas así: **campos/propiedades → constructor(es) → métodos públicos o protected → métodos privados**. Esta pasada es sobre **todo el repo**, no solo los archivos que toca el punto anterior.

---

## 1. Nuevo `TipoError.FalloNoManejadoGenerico`

### 1.1 `Operativ.BE/Enums/TipoError.cs` — contenido completo

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
    FalloNoManejadoGenerico
}
```

### 1.2 `Operativ.BE/Modelos/DefinicionError.cs` — contenido completo

Los códigos ERR usados llegan hasta ERR15. El nuevo usa **ERR16**.

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

### 1.3 `Operativ.BE/Errores/ErroresHandler.cs` — contenido completo

Único cambio real: el `return` final de `TraducirExcepcion`.

```csharp
using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Web;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;

namespace Operativ.BE.Errores;
public class ErroresHandler
{
    public string GetMensaje(TipoError tipoError)
    {
        return GetMensaje(tipoError, null);
    }

    public string GetMensaje(TipoError tipoError, string[] parametros)
    {
        DefinicionError definicion = DefinicionError.ObtenerPorTipo(tipoError);
        string texto = GetTexto(definicion.ClaveRecurso, parametros);
        return definicion.Codigo + " - " + texto;
    }

    public string GetMensaje(OperativException excepcion)
    {
        return GetMensaje(excepcion.TipoError, excepcion.Parametros);
    }

    public OperativException TraducirExcepcion(Exception excepcion)
    {
        if (excepcion is OperativException)
        {
            return (OperativException)excepcion;
        }

        if (excepcion is SqlException)
        {
            return new OperativException(TipoError.ErrorConexionBaseDatos);
        }

        if (excepcion is SmtpException)
        {
            return new OperativException(TipoError.ErrorEnvioEmail);
        }

        return new OperativException(TipoError.FalloNoManejadoGenerico);
    }

    private string GetTexto(string claveRecurso, string[] parametros)
    {
        string textoRecurso = HttpContext.GetGlobalResourceObject("Textos", claveRecurso) as string;

        if (parametros == null)
        {
            return textoRecurso;
        }

        return string.Format(textoRecurso, parametros);
    }
}
```

### 1.4 Recursos — agregar una clave nueva en cada archivo

`Operativ.Web/App_GlobalResources/Textos.resx` — agregar, junto a las demás `MensajeError*` (por ejemplo después de `MensajeErrorClaveNoCumpleComplejidad`):

```xml
<data name="MensajeErrorFalloNoManejadoGenerico" xml:space="preserve">
  <value>Ocurrió un error inesperado. Intentá nuevamente más tarde.</value>
</data>
```

`Operativ.Web/App_GlobalResources/Textos.en.resx` — mismo lugar relativo:

```xml
<data name="MensajeErrorFalloNoManejadoGenerico" xml:space="preserve">
  <value>An unexpected error occurred. Please try again later.</value>
</data>
```

---

## 2. `Notificaciones.ascx.cs` — el control que absorbe la lógica

`Operativ.Web/Paginas/Controles/Notificaciones.ascx.cs` — contenido completo (reemplaza el archivo entero):

```csharp
using System;
using System.Text.RegularExpressions;
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
    }
}
```

No tocar `Notificaciones.ascx` (el markup) ni `Notificaciones.ascx.designer.cs`.

---

## 3. `PaginaSeguraBase.cs` — property para llegar al control sin repetir el cast

Esto es solo para páginas que usan la Master Page (`Principal.Master`). Las páginas que **no** usan Master (`Login.aspx`, `RecuperarContrasena.aspx`) tienen su propio campo `ucNotificaciones` directo y no necesitan esto — ya quedan simplificadas solo con el paso 2.

`Operativ.Web/Paginas/PaginaSeguraBase.cs` — contenido completo:

```csharp
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
}
```

`PaginaBase.cs` (la clase padre, sin Master) **no se toca** — no tiene ninguna noción de notificaciones y no debe agregarse ninguna, porque rompería en las páginas sin Master Page.

---

## 4. Code-behinds a actualizar

### 4.1 `Operativ.Web/Paginas/Usuarios/Login.aspx.cs` — contenido completo

```csharp
using System;
using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.SEC.Helpers;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class Login : PaginaBase
{
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;
    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;
    private readonly SesionHandler sesionHandler;
    private bool modoEmergencia;

    public Login()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo.Nombre));
        }
        VerificarIntegridadSistema();
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

        try
        {
            Usuario usuario = usuarioService.ValidarCredenciales(
                txtNombreUsuario.Text.Trim(),
                txtContrasena.Text);

            Familia perfil = familiaService.GetPerfilDeUsuario(usuario.IdUsuario);
            FamiliaCompuesto arbolPermisos = familiaService.ArmarArbolPermisos(usuario.IdUsuario);

            sesionHandler.IniciarSesion(usuario, perfil, arbolPermisos);

            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfil.Nombre), false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void btnIngresoEmergencia_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            bool credencialesValidas = LoginEmergenciaHelper.ValidarCredenciales(
                txtUsuarioEmergencia.Text.Trim(), txtContrasenaEmergencia.Text);

            if (!credencialesValidas)
            {
                throw new OperativException(TipoError.ErrorCredencialesEmergenciaInvalidas);
            }

            integridadService.RepararBaseDatos();

            bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);

            Usuario usuarioEmergencia = new Usuario
            {
                IdUsuario = 0,
                NombreUsuario = txtUsuarioEmergencia.Text.Trim(),
                NombreCompleto = "Web Master (acceso de emergencia)",
                Activo = true,
                Bloqueado = false
            };

            Familia perfilEmergencia = new Familia
            {
                IdFamilia = 0,
                Nombre = "WebMaster",
                Descripcion = "Acceso de emergencia"
            };

            sesionHandler.IniciarSesion(usuarioEmergencia, perfilEmergencia, new FamiliaCompuesto());

            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilEmergencia.Nombre) + "?reparado=1", false);
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

Notar: `using Operativ.BE.Errores;` **se mantiene** — `OperativException` se sigue lanzando explícitamente en `btnIngresoEmergencia_Click`. Se eliminó el campo `erroresHandler` (quedaba sin usos) y su alta en el constructor.

### 4.2 `Operativ.Web/Paginas/Usuarios/RecuperarContrasena.aspx.cs` — contenido completo

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

`using Operativ.BE.Errores;` se elimina — ya no queda ningún uso de `ErroresHandler`/`OperativException` en este archivo.

### 4.3 `Operativ.Web/Paginas/Controles/ModalCambiarClave.ascx.cs` — contenido completo

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

Este control es un `UserControl`, no hereda `PaginaSeguraBase`, por eso tiene su propia property privada `ControlNotificaciones` en vez de heredarla. `using Operativ.BE.Errores;` se elimina (sin más usos).

### 4.4 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx.cs` — contenido completo

Esta página ya tenía wrappers privados `MostrarExito`/`MostrarError` que reimplementaban justo lo que ahora vive en `Notificaciones.ascx.cs` — se eliminan. Además, `btnDesbloquear_Click` (protected) estaba ubicado después de `DarDeBaja` (private) — se reordena junto con el resto de los handlers `protected` (ver punto 5 más abajo, este archivo es el ejemplo concreto de la regla de organización).

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class GestionUsuarios : PaginaSeguraBase
{
    private const int TamanioPagina = 10;
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

        List<Usuario> usuarios = usuarioService.ListarUsuarios(filtro, idFamilia, NumeroPagina, TamanioPagina);
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
        int desde = total == 0 ? 0 : ((NumeroPagina - 1) * TamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        string formato = (string)GetGlobalResourceObject("Textos", "MensajeResumenPaginado");
        litResumenPaginado.Text = string.Format(formato, desde, hasta, total);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnPaginaAnterior.Enabled = NumeroPagina > 1;
        btnPaginaSiguiente.Enabled = (NumeroPagina * TamanioPagina) < total;
    }
}
```

Cambios respecto del original: se eliminó el campo `erroresHandler` y `using Operativ.BE.Errores;` (sin más usos), se eliminó `using Operativ.Web.Master;` (ya no se castea `Master` directamente, eso vive ahora en `PaginaSeguraBase`), se eliminaron los métodos privados `MostrarExito`/`MostrarError`, y `btnDesbloquear_Click` se movió junto a los demás `protected` (antes estaba después de `DarDeBaja`, un método privado).

Importante: los demás usos de `GetGlobalResourceObject("Textos", ...)` en este archivo (`litMensajeBloqueado`, `tituloFormulario`, placeholders de los combos, `litResumenPaginado`) **no son mensajes de notificación** — no tocarlos, quedan igual que están.

---

## 5. Regla de organización — pasada sobre TODO el repo

**Regla:** en cada clase (BE, BLL, DAL, SEC, Web — code-behinds, controles, servicios, repositorios, handlers, helpers), el orden de los miembros debe ser:

1. Campos y propiedades.
2. Constructor(es).
3. Todos los métodos `public`, `protected` o `internal` (sin necesidad de reordenar entre sí más allá de lo que ayude a la legibilidad).
4. Todos los métodos `private`.

Nunca debe aparecer un método privado antes que un método público/protected/internal.

### Cómo aplicar esto

- **No tocar la lógica interna de ningún método** al reordenar — es un corte y pega de bloques completos, no una reescritura. El objetivo es cero cambios de comportamiento.
- **No aplica a interfaces** (todo lo que esté en una carpeta `Contratos/`): una interfaz es 100% pública por definición, no hay nada que reordenar.
- **No tocar los `*.designer.cs`**: son generados por ASP.NET Web Forms, no se editan a mano.
- Los `enum` no tienen métodos, no aplica.
- Las propiedades (`get`/`set`, incluidas las auto-implementadas) se consideran parte de la sección 1 (junto a los campos), siguiendo el patrón que ya usa el propio repo en `GestionUsuarios.aspx.cs` (`PerfilesPermitidos`, `NumeroPagina`) y en `PaginaSeguraBase.cs` (`SesionHandler`, `AutorizacionHandler`) — no se mezclan con la sección 3/4 de métodos.

### Ya identificado como violación (se corrige en este mismo plan, ver 4.4)

- `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx.cs`: `btnDesbloquear_Click` (protected) estaba después de `DarDeBaja` (private). Ya corregido en el contenido completo de la sección 4.4.

### Ya auditados y confirmados conformes (no requieren cambios de orden, dejar como están salvo lo indicado en las secciones 1-4)

- `Operativ.BE/Errores/ErroresHandler.cs`
- `Operativ.BE/Modelos/DefinicionError.cs`
- `Operativ.Web/Paginas/Usuarios/Login.aspx.cs` (después del refactor de la sección 4.1)
- `Operativ.Web/Paginas/Usuarios/RecuperarContrasena.aspx.cs` (después del refactor de la sección 4.2)
- `Operativ.Web/Paginas/Controles/ModalCambiarClave.ascx.cs` (después del refactor de la sección 4.3)
- `Operativ.Web/Paginas/Controles/ResumenUsuario.ascx.cs`
- `Operativ.Web/Paginas/Controles/Navbar.ascx.cs`
- `Operativ.Web/Paginas/Controles/SelectorIdioma.ascx.cs`
- `Operativ.Web/Paginas/Controles/DashboardResumen.ascx.cs`
- `Operativ.Web/Paginas/PaginaBase.cs`
- `Operativ.Web/Paginas/PaginaSeguraBase.cs` (después del refactor de la sección 3)
- `Operativ.Web/Paginas/Comun/NoAutorizado.aspx.cs`
- `Operativ.Web/Paginas/Comun/Error.aspx.cs`
- `Operativ.Web/Paginas/Home/HomeAdministrador.aspx.cs`
- `Operativ.Web/Paginas/Home/HomeWebMaster.aspx.cs`
- `Operativ.Web/Paginas/Home/HomeCliente.aspx.cs`
- `Operativ.Web/Global.asax.cs`
- `Operativ.Web/Master/Principal.Master.cs`

### Pendientes de auditar (no llegué a leerlos por límite de la sesión anterior — revisar y corregir si hace falta)

Recorrer estos archivos/carpetas y aplicar la regla donde corresponda:

**Operativ.Web** (lo que falta):
- `Paginas/Home/HomeComercial.aspx.cs`
- `Paginas/Controles/Footer.ascx.cs`

**Operativ.SEC:**
- `Handlers/AutorizacionHandler.cs`
- `Handlers/SesionHandler.cs`
- `Helpers/AesHelper.cs`
- `Helpers/ClaveHelper.cs`
- `Helpers/EmailHelper.cs`
- `Helpers/HashHelper.cs`
- `Helpers/LoginEmergenciaHelper.cs`
- `Implementaciones/BitacoraService.cs`
- `Implementaciones/FamiliaService.cs`
- `Implementaciones/IntegridadService.cs`
- `Implementaciones/UsuarioService.cs`
- `Implementaciones/UsuarioService.Abm.cs`
- `Fabricas/FabricaSeguridad.cs`
- `Configuracion/ConfiguracionAplicacion.cs`
- (`Contratos/*.cs` — son interfaces, no aplica)

**Operativ.DAL:**
- `Implementaciones/BitacoraRepositorio.cs`
- `Implementaciones/FamiliaRepositorio.cs`
- `Implementaciones/IntegridadRepositorio.cs`
- `Implementaciones/UsuarioRepositorio.cs`
- `Convertidores/FamiliaConvertidor.cs`
- `Convertidores/PatenteConvertidor.cs`
- `Convertidores/UsuarioConvertidor.cs`
- `Conexion/*.cs`
- `Fabricas/*.cs`
- `Integridad/*.cs`
- (`Contratos/*.cs` — interfaces, no aplica)

**Operativ.BE:**
- `Entidades/*.cs`
- `Composite/*.cs`
- `Modelos/AccionBitacora.cs`
- `Modelos/ResultadoVerificacionTabla.cs`
- `Modelos/TablasVerificables.cs`

**Operativ.BLL:**
- `Fabricas/FabricaNegocio.cs` (hoy es una clase vacía, probablemente no aplica, pero confirmar)

Para cada archivo: si ya cumple el orden, no tocarlo (evitar diffs sin cambios reales). Si no cumple, reordenar sin tocar lógica.

---

## 6. Pasos para Claude Code

1. `git checkout main && git pull`
2. `git checkout -b refactor/notificaciones-centralizadas`
3. Aplicar los cambios de las secciones 1 a 4 (contenido completo de cada archivo listado).
4. Recorrer la lista de la sección 5 y corregir el orden donde falte.
5. Commits sugeridos (separados, para que el diff sea fácil de revisar):
   - `feat: agrega TipoError.FalloNoManejadoGenerico y corrige catch-all de ErroresHandler`
   - `refactor: centraliza manejo de mensajes en Notificaciones.ascx.cs`
   - `refactor: simplifica code-behinds que mostraban mensajes (Login, RecuperarContrasena, ModalCambiarClave, GestionUsuarios)`
   - `style: reordena miembros publicos/protected antes que privados en todo el repo`
6. Build completo: `MSBuild.exe Operativ.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"` (ruta del MSBuild según `CLAUDE.md` del repo).
7. Si el build compila, levantar IIS Express (`Operativ.Web/IniciarIISExpress.bat` o F5) y probar a mano, comparando contra el comportamiento antes del refactor:
   - `Login.aspx`: credenciales incorrectas (debe seguir mostrando `ERR01`/`ERR02`/`ERR03`), sesión expirada vía `?err=sesion` (`ERR11`).
   - `RecuperarContrasena.aspx`: usuario inexistente (`ERR01`) y éxito (mensaje de `MensajeExitoRecuperacionContrasena`).
   - `GestionUsuarios.aspx`: alta, modificación, baja y desbloqueo de usuario — mismos mensajes de éxito/error que antes.
   - Modal de cambiar contraseña: éxito y contraseña actual incorrecta (`ERR14`).
   - Forzar una excepción no mapeada (si es fácil de simular) y confirmar que ahora muestra `ERR16` en vez de `ERR05`.
8. No mergear a `main`. Dejar la rama lista con los commits para que el dueño del repo la revise y haga el push.
