# Hotfix: verificación de dígitos verificadores durante el login, mensaje genérico y recálculo desde el modal — Operativ (2.2.1)

**Repo:** `matias-milio-dev/TFI.Operativ.WebApp`, base `main` (HEAD auditado: `e793608dc8d85546b56fe3a65bc0daca803d514c` — "feat(web): agrega la pagina de consultar bitacora"; los parches 2.1 y 2.2 ya están mergeados, más iteraciones propias encima: descarga de backup, restore desde archivo subido).
**Rama a crear:** `hotfix/parche-2.2.1-integridad-login`.
**No hacer merge ni push a `main`.** Dejar los commits listos en la rama local para que el dueño del repo los revise y los suba él mismo.

Recordatorio de estándares: sin comentarios en C#, sin `var`, sin tuplas/record/LINQ/lambdas, `namespace X;` scoped, ifs con llaves, orden público→protected→private, servicios solo vía fábricas, errores vía `OperativException`/`TipoError`/`DefinicionError`, auditoría vía `IBitacoraService`.

---

## 0. Correcciones a implementar

1. La verificación de integridad (dígitos verificadores) tiene que ocurrir **durante el login, después de autenticarse** — no en el `Page_Load` de la página de Login — y mostrar un **mensaje genérico** ("Ha ocurrido un problema en el sistema") en vez del detalle completo que muestra hoy.
2. Eliminar la distinción visual del acceso de emergencia. El flujo nuevo queda así:
   - Un solo botón "Ingresar". Internamente, si la integridad de la base está corrupta, el login **suma el archivo XML de emergencia como fuente de login adicional**.
   - Cuando se entra por esa vía, el popup que hoy dice "Base de datos reparada" pasa a mostrar **el detalle completo del error y las filas afectadas** (lo que antes se mostraba en el login), con un botón **"Recalcular"** que finalmente ejecuta el recálculo y vuelve al login.

---

## 1. Diseño y decisiones

### 1.1 Estado actual: la verificación está apagada

`Login.aspx.cs` tiene hoy la llamada comentada:

```csharp
//VerificarIntegridadSistema();
```

Es decir, la verificación de integridad **no corre en ningún momento** en el estado actual de `main`; el método `VerificarIntegridadSistema()` existe pero está muerto. Este hotfix no "mueve" una validación que esté funcionando: la reimplementa en el lugar correcto y la vuelve a activar. Vale tenerlo presente al probar — cualquier corrupción que hoy exista en la base local no está siendo detectada por nadie.

### 1.2 Por qué el orden es autenticar → verificar, y no al revés

El pedido es explícito: la validación ocurre "luego de autenticarse". Eso tiene dos consecuencias buenas más allá de cumplir el requisito:

- **No se filtra información a un anónimo.** Hoy la verificación corría en `Page_Load`, así que cualquiera que abriera la pantalla de login veía el detalle completo de qué tablas y qué filas estaban alteradas, sin haber probado una sola credencial. Es exactamente el tipo de dato que no debería ser público (le dice a un atacante qué tabla logró tocar y qué fila quedó marcada).
- **No se paga el costo de recorrer todas las tablas en cada visita.** `VerificarTodo()` recalcula el DVV de cada tabla verificable; hacerlo en cada carga de la pantalla de login (incluido cada postback de validación fallida) es caro y sin sentido.

El flujo queda:

```
btnIngresar_Click
  └─ intentar login normal (LoginNormalStrategy, contra la base)
       ├─ credenciales OK
       │    └─ verificar integridad
       │         ├─ íntegra    → iniciar sesión y redirigir a su Home
       │         └─ corrupta   → registrar en bitácora CON detalle,
       │                         mostrar mensaje GENÉRICO, no iniciar sesión
       └─ credenciales falladas (excepción)
            └─ verificar integridad
                 ├─ íntegra   → mostrar el error de login original
                 └─ corrupta  → reintentar contra el XML de emergencia
                      ├─ coincide    → sesión de emergencia, guardar las fallas
                      │                en sesión, redirigir a HomeWebMaster
                      │                (abre el modal con el detalle + Recalcular)
                      └─ no coincide → mostrar el error de login original
```

### 1.3 Por qué el XML se prueba recién cuando falla el login normal

Las credenciales de emergencia viven en el XML, no en la tabla `Usuario`, así que un login normal con ellas **siempre** va a fallar con `ErrorUsuarioNoExiste`. Probar primero la base y recién después el XML (y solo si la base está corrupta) mantiene el caso común barato y no cambia en nada el comportamiento para los usuarios normales.

Un punto de seguridad importante: cuando el intento contra el XML falla, **no se muestra el error de emergencia**, se muestra el error del login normal original (por ejemplo "el usuario no existe"). Si mostráramos `ErrorCredencialesEmergenciaInvalidas`, un atacante podría distinguir "existe una vía de emergencia y le pegué al usuario" de "no existe" — y de paso se enteraría de que la base está corrupta, que es justo lo que el mensaje genérico del punto 1 busca ocultar. Por eso ese `catch` traga la excepción de emergencia a propósito.

### 1.4 El mensaje genérico: se cambia el recurso, no se agrega un `TipoError`

`TipoError.ErrorIntegridadCorrupta` (`ERR04`) ya existe y ya se usa. Lo único que hay que cambiar es el **texto del recurso**, que hoy es:

> `Se detectó una alteración en la integridad de los datos del sistema. Detalle: {0}. Si sos Web Master, podés ingresar por la vía de emergencia para reparar la base.`

Tres problemas en una sola línea: expone el detalle (`{0}`), confirma que hubo una alteración, y encima le explica al que está mirando que existe una vía de emergencia. El texto nuevo no lleva placeholder y no dice nada de eso. Se sigue llamando desde el código con `MostrarMensaje(TipoError.ErrorIntegridadCorrupta)` (el overload sin parámetros que ya existe en `Notificaciones`).

**El detalle no se pierde:** se sigue registrando completo en la bitácora vía `TipoAccionBitacora.IntegridadCorrupta` con `FormatearResumenFallas(...)` como detalle adicional, igual que hacía antes. Y ahora, gracias al parche 2.2, ese detalle es consultable desde la página de bitácora — así que el WebMaster tiene dónde verlo incluso sin pasar por el modal.

### 1.5 El detalle viaja por sesión, no por querystring

Hoy la estrategia de emergencia marca `SufijoRedireccion = "?reparado=1"` y `HomeWebMaster` abre el modal si ve ese parámetro. Para el flujo nuevo eso no alcanza: hay que llevar la lista completa de fallas (tablas + claves de filas alteradas + valores de DVV) hasta el modal, y eso no entra razonablemente en una URL — ni debería, porque sería otra vez exponer el detalle en un lugar visible.

Se guarda la `List<ResultadoVerificacionTabla>` en sesión a través de `SesionHandler` (que es el único lugar del proyecto que toca `HttpContext.Current.Session` — ninguna página lo hace directo, y este hotfix no rompe esa regla). El modal se muestra **si y solo si** hay fallas guardadas en sesión, lo que además tiene una propiedad linda: si el WebMaster cierra el modal y navega a otra parte y vuelve al Home, el modal se vuelve a abrir, porque la base sigue corrupta. Con el querystring eso no pasaba (se perdía en la primera navegación).

Como consecuencia, `ResultadoAutenticacion.SufijoRedireccion` queda sin ningún uso y se elimina; en su lugar se agrega `EsAccesoEmergencia` (bool), que es lo que el login realmente necesita saber para decidir el camino. Es un dato más honesto: antes el "soy emergencia" viajaba disfrazado de fragmento de URL.

### 1.6 El recálculo se mueve del login al botón "Recalcular"

Hoy `LoginEmergenciaStrategy.Autenticar` hace tres cosas: valida las credenciales del XML, **repara la base** (`integridadService.RepararBaseDatos()`) y registra `ReparacionEmergenciaBaseDatos` en la bitácora. Es decir, la reparación ya está hecha antes de que el WebMaster vea nada — el modal actual solo le avisa de un hecho consumado, y el botón "Aceptar" no repara nada, solo cierra sesión.

Con el flujo nuevo eso se invierte: la estrategia de emergencia **solo autentica**. La reparación y su asiento en la bitácora se mudan al handler del botón "Recalcular" del modal. Así el WebMaster primero ve qué se rompió y después decide recalcular, que es el orden que pide el pedido.

Detalle de orden dentro del handler: primero `RepararBaseDatos()` y **después** `bitacoraService.Registrar(...)`. Registrar es un `INSERT` en `Bitacora` que dispara `ActualizarDVH` sobre esa fila y el recálculo del DVV de la tabla; si se registrara antes del `RecalcularTodo()`, ese trabajo se haría dos veces. Haciéndolo después, el recálculo global deja todo consistente y la fila nueva de bitácora mantiene su propio DVH/DVV correctamente.

### 1.7 El login pierde el segundo panel

`pnlAccesoEmergencia` (con sus dos textboxes, sus dos validadores, su `ValidationSummary` y el botón "Ingresar y reparar") se elimina completo del markup, junto con `btnIngresoEmergencia_Click` y el campo `modoEmergencia` del code-behind. Ya no hay dos grupos de validación (`Login` / `Emergencia`), queda solo uno.

De paso, `pnlLoginNormal` deja de ser un `asp:Panel` y pasa a ser un `<div class="caja-login">`: era un Panel únicamente para poder ocultarlo cuando aparecía el panel de emergencia, y sin ese segundo panel nunca se oculta. Menos ruido en el designer y una cosa menos que pueda quedar en un estado raro.

La clase CSS `.panel-emergencia` queda huérfana y se borra.

### 1.8 Qué queda como está (fuera de alcance)

- **`LoginNormalStrategy` no se toca.** Sigue registrando `LoginExitoso` dentro de `ValidarCredenciales` cuando las credenciales son válidas. Eso significa que, si la base está corrupta, la bitácora va a mostrar un `LoginExitoso` seguido de un `IntegridadCorrupta` para un usuario que en realidad **no** obtuvo sesión. Es defendible tal cual (las credenciales *fueron* correctas; lo que se denegó fue la sesión, por otro motivo), y arreglar la semántica del nombre de esa acción es un refactor de la estrategia que no corresponde a un hotfix.
- **Si la verificación de integridad falla por excepción** (por ejemplo un `SqlException`, o la tabla `DigitosVerticales` inaccesible), el error se traduce y se muestra como lo que es (`ERR05` / `ERR16`), no como el mensaje genérico de integridad. Son dos problemas distintos: "no pude verificar" no es lo mismo que "verifiqué y está corrupta", y mezclarlos haría más difícil diagnosticar el primero. El comportamiento viejo (forzar modo emergencia ante cualquier excepción del chequeo) desaparece con el panel.
- No se agrega ninguna patente nueva: el acceso de emergencia no pasa por el modelo de patentes (la sesión de emergencia arma un `FamiliaCompuesto` vacío a propósito), y `HomeWebMaster` ya se gatea por `PerfilesPermitidos = WebMaster`.

---

## 2. Capa BE

### 2.1 `Operativ.BE/Modelos/ResultadoAutenticacion.cs` — contenido completo

```csharp
using Operativ.BE.Entidades;
using Operativ.BE.Modelos.Composite;

namespace Operativ.BE.Modelos;
public class ResultadoAutenticacion
{
    public Usuario Usuario { get; set; }

    public Familia Perfil { get; set; }

    public FamiliaCompuesto ArbolPermisos { get; set; }

    public bool EsAccesoEmergencia { get; set; }
}
```

---

## 3. Capa SEC

### 3.1 `Operativ.SEC/Handlers/SesionHandler.cs` — contenido completo

Se agrega el par guardar/leer de las fallas de integridad. No hace falta un método para limpiarlas: `CerrarSesion()` ya hace `Session.Clear()` y `Session.Abandon()`, y el único camino que las borra es justamente cerrar la sesión de emergencia después de recalcular.

```csharp
using System.Collections.Generic;
using System.Web;
using Operativ.BE.Modelos;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;

namespace Operativ.SEC.Handlers;
public class SesionHandler
{
    private const string ClaveUsuario = "Operativ_UsuarioLogueado";

    private const string ClavePerfil = "Operativ_PerfilLogueado";

    private const string ClaveArbolPermisos = "Operativ_ArbolPermisosLogueado";

    private const string ClaveFallasIntegridad = "Operativ_FallasIntegridad";

    public void IniciarSesion(Usuario usuario, Familia perfil, FamiliaCompuesto arbolPermisos)
    {
        HttpContext.Current.Session[ClaveUsuario] = usuario;
        HttpContext.Current.Session[ClavePerfil] = perfil;
        HttpContext.Current.Session[ClaveArbolPermisos] = arbolPermisos;
    }

    public Usuario GetUsuario()
    {
        return HttpContext.Current.Session[ClaveUsuario] as Usuario;
    }

    public Familia GetPerfil()
    {
        return HttpContext.Current.Session[ClavePerfil] as Familia;
    }

    public FamiliaCompuesto GetArbolPermisos()
    {
        return HttpContext.Current.Session[ClaveArbolPermisos] as FamiliaCompuesto;
    }

    public void GuardarFallasIntegridad(List<ResultadoVerificacionTabla> fallas)
    {
        HttpContext.Current.Session[ClaveFallasIntegridad] = fallas;
    }

    public List<ResultadoVerificacionTabla> GetFallasIntegridad()
    {
        return HttpContext.Current.Session[ClaveFallasIntegridad] as List<ResultadoVerificacionTabla>;
    }

    public bool HaySesionActiva()
    {
        return GetUsuario() != null;
    }

    public void CerrarSesion()
    {
        HttpContext.Current.Session.Clear();
        HttpContext.Current.Session.Abandon();
    }
}
```

Nota: el estado de sesión de la app es el default de ASP.NET (`InProc` — no hay `<sessionState>` en `Web.config`), así que guardar objetos de dominio directamente es válido y ya es lo que se venía haciendo con `Usuario`/`Familia`/`FamiliaCompuesto`. Si en el futuro se pasara a `StateServer` o `SQLServer`, habría que marcar `ResultadoVerificacionTabla` como `[Serializable]` — igual que haría falta para las otras tres clases que ya viajan por sesión.

### 3.2 `Operativ.SEC/Implementaciones/Estrategias/LoginEmergenciaStrategy.cs` — contenido completo

Se le saca la reparación y el asiento en bitácora (se mudan al modal, ver 1.6), con lo cual la clase deja de necesitar `IIntegridadService` e `IBitacoraService` y se queda sin constructor. Marca `EsAccesoEmergencia = true`.

```csharp
using System;
using System.Xml;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.BE.Modelos;
using Operativ.BE.Modelos.Composite;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Helpers;

namespace Operativ.SEC.Implementaciones.Estrategias;
public class LoginEmergenciaStrategy : ILoginStrategy
{
    public ResultadoAutenticacion Autenticar(string nombreUsuario, string contrasena)
    {
        if (!ValidarCredenciales(nombreUsuario, contrasena))
        {
            throw new OperativException(TipoError.ErrorCredencialesEmergenciaInvalidas);
        }

        Usuario usuarioEmergencia = new Usuario
        {
            IdUsuario = 0,
            NombreUsuario = nombreUsuario,
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

        return new ResultadoAutenticacion
        {
            Usuario = usuarioEmergencia,
            Perfil = perfilEmergencia,
            ArbolPermisos = new FamiliaCompuesto(),
            EsAccesoEmergencia = true
        };
    }

    private bool ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        XmlDocument documento = XmlHelper.CargarDocumento(ConfiguracionAplicacion.RutaXmlEmergencia);

        string nombreUsuarioEsperado = XmlHelper.LeerNodo(documento, "NombreUsuario");
        string salt = XmlHelper.LeerNodo(documento, "Salt");
        string hashAlmacenado = XmlHelper.LeerNodo(documento, "HashContrasena");

        if (!string.Equals(nombreUsuario, nombreUsuarioEsperado, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HashHelper.ValidarContrasena(contrasena, salt, hashAlmacenado);
    }
}
```

`FabricaSeguridad.CrearLoginStrategy(bool modoEmergencia)` no cambia: sigue devolviendo `new LoginEmergenciaStrategy()` o `new LoginNormalStrategy()` según el flag. `LoginNormalStrategy` no se toca.

---

## 4. Capa Web

### 4.1 `Operativ.Web/Paginas/Usuarios/Login.aspx` — contenido completo

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Operativ.Web.Login" %>
<%@ Register TagPrefix="uc" TagName="Notificaciones" Src="~/Paginas/Controles/Notificaciones.ascx" %>
<%@ Register TagPrefix="uc" TagName="SelectorIdioma" Src="~/Paginas/Controles/SelectorIdioma.ascx" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title runat="server" meta:resourcekey="TituloPagina">Operativ - Iniciar sesión</title>
    <link runat="server" rel="icon" type="image/x-icon" href="~/favicon.ico" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&amp;display=swap" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="<%= Operativ.Web.Paginas.RecursoEstatico.ObtenerUrl("~/Estilos/operativ.css") %>" />
</head>
<body class="pagina-login">
    <form id="formLogin" runat="server">
        <div class="selector-idioma-flotante">
            <uc:SelectorIdioma ID="ucSelectorIdioma" runat="server" />
        </div>
        <uc:Notificaciones ID="ucNotificaciones" runat="server" />
        <div class="caja-login">
            <h1>Operativ</h1>
            <div class="campo-formulario">
                <label for="<%= txtNombreUsuario.ClientID %>"><asp:Literal ID="litEtiquetaUsuario" runat="server" Text="<%$ Resources:Textos, EtiquetaNombreUsuario %>" /></label>
                <asp:TextBox ID="txtNombreUsuario" runat="server" />
                <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionUsuarioObligatorio %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Login" />
            </div>
            <div class="campo-formulario">
                <label for="<%= txtContrasena.ClientID %>"><asp:Literal ID="litEtiquetaContrasena" runat="server" Text="<%$ Resources:Textos, EtiquetaContrasena %>" /></label>
                <asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" />
                <asp:RequiredFieldValidator ID="rfvContrasena" runat="server" ControlToValidate="txtContrasena"
                    ErrorMessage="<%$ Resources:Textos, MensajeValidacionContrasenaObligatoria %>" CssClass="texto-validacion" Display="Dynamic" ValidationGroup="Login" />
            </div>
            <asp:Button ID="btnIngresar" runat="server" Text="<%$ Resources:Textos, BotonIniciarSesion %>" CssClass="boton-principal" OnClick="btnIngresar_Click" ValidationGroup="Login" />
            <asp:HyperLink ID="lnkRecuperarContrasena" runat="server" NavigateUrl="~/Paginas/Usuarios/RecuperarContrasena.aspx" CssClass="enlace-secundario" Text="<%$ Resources:Textos, EnlaceOlvidoContrasena %>" />
            <asp:ValidationSummary ID="vsLogin" runat="server" CssClass="texto-validacion" ValidationGroup="Login" />
        </div>
    </form>
</body>
</html>
```

### 4.2 `Operativ.Web/Paginas/Usuarios/Login.aspx.designer.cs` — contenido completo

```csharp
namespace Operativ.Web;
public partial class Login
{
    protected global::Operativ.Web.Controles.Notificaciones ucNotificaciones;

    protected global::Operativ.Web.Controles.SelectorIdioma ucSelectorIdioma;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaUsuario;

    protected global::System.Web.UI.WebControls.TextBox txtNombreUsuario;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvNombreUsuario;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaContrasena;

    protected global::System.Web.UI.WebControls.TextBox txtContrasena;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvContrasena;

    protected global::System.Web.UI.WebControls.Button btnIngresar;

    protected global::System.Web.UI.WebControls.HyperLink lnkRecuperarContrasena;

    protected global::System.Web.UI.WebControls.ValidationSummary vsLogin;
}
```

### 4.3 `Operativ.Web/Paginas/Usuarios/Login.aspx.cs` — contenido completo

El `Response.Redirect(..., false)` + `CompleteRequest()` ya se usaba así en este archivo: con `endResponse: false` no se lanza `ThreadAbortException`, así que la redirección no puede quedar atrapada por los `catch` de más arriba.

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

    public Login()
    {
        fabricaSeguridad = new FabricaSeguridad();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            return;
        }

        if (sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo?.Nombre));
        }

        if (Request.QueryString["err"] == "sesion")
        {
            ucNotificaciones.MostrarMensaje(TipoError.ErrorSesionExpirada);
        }

        if (Request.QueryString["restaurado"] == "1")
        {
            MostrarExito("MensajeExitoRestaurarBackup");
        }

        if (Request.QueryString["recalculado"] == "1")
        {
            MostrarExito("MensajeExitoRecalculoDigitos");
        }
    }

    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        string nombreUsuario = txtNombreUsuario.Text.Trim();
        string contrasena = txtContrasena.Text;

        try
        {
            IntentarLoginNormal(nombreUsuario, contrasena);
        }
        catch (Exception excepcionLoginNormal)
        {
            IntentarLoginEmergencia(nombreUsuario, contrasena, excepcionLoginNormal);
        }
    }

    private void IntentarLoginNormal(string nombreUsuario, string contrasena)
    {
        ILoginStrategy estrategia = fabricaSeguridad.CrearLoginStrategy();
        ResultadoAutenticacion resultado = estrategia.Autenticar(nombreUsuario, contrasena);

        List<ResultadoVerificacionTabla> fallas = integridadService.VerificarIntegridad();

        if (fallas.Count > 0)
        {
            RegistrarFallasEnBitacora(resultado.Usuario.IdUsuario, fallas);
            ucNotificaciones.MostrarMensaje(TipoError.ErrorIntegridadCorrupta);
            return;
        }

        IniciarSesionYRedirigir(resultado);
    }

    private void IntentarLoginEmergencia(string nombreUsuario, string contrasena, Exception excepcionLoginNormal)
    {
        List<ResultadoVerificacionTabla> fallas;

        try
        {
            fallas = integridadService.VerificarIntegridad();
        }
        catch (Exception)
        {
            ucNotificaciones.MostrarMensaje(excepcionLoginNormal);
            return;
        }

        if (fallas.Count == 0)
        {
            ucNotificaciones.MostrarMensaje(excepcionLoginNormal);
            return;
        }

        ResultadoAutenticacion resultado;

        try
        {
            ILoginStrategy estrategia = fabricaSeguridad.CrearLoginStrategy(modoEmergencia: true);
            resultado = estrategia.Autenticar(nombreUsuario, contrasena);
        }
        catch (Exception)
        {
            ucNotificaciones.MostrarMensaje(excepcionLoginNormal);
            return;
        }

        RegistrarFallasEnBitacora(null, fallas);
        sesionHandler.GuardarFallasIntegridad(fallas);
        IniciarSesionYRedirigir(resultado);
    }

    private void IniciarSesionYRedirigir(ResultadoAutenticacion resultado)
    {
        sesionHandler.IniciarSesion(resultado.Usuario, resultado.Perfil, resultado.ArbolPermisos);
        Response.Redirect(NavegacionHelper.ObtenerUrlHome(resultado.Perfil?.Nombre), false);
        Context.ApplicationInstance.CompleteRequest();
    }

    private void RegistrarFallasEnBitacora(int? idUsuario, List<ResultadoVerificacionTabla> fallas)
    {
        string detalle = integridadService.FormatearResumenFallas(fallas);
        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.IntegridadCorrupta, detalle);
    }

    private void MostrarExito(string claveRecurso)
    {
        string mensaje = (string)GetGlobalResourceObject("Textos", claveRecurso);
        ucNotificaciones.MostrarMensaje(mensaje, true);
    }
}
```

### 4.4 `Operativ.Web/Paginas/Home/HomeWebMaster.aspx` — contenido completo

El modal pasa de "ya reparé" a "esto se rompió, ¿recalculás?". `Container.DataItem` se castea a `ResultadoVerificacionTabla`, así que hace falta la directiva `Import` del namespace de modelos.

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomeWebMaster.aspx.cs" Inherits="Operativ.Web.Paginas.HomeWebMaster" MasterPageFile="~/Master/Principal.Master" %>
<%@ Import Namespace="Operativ.BE.Modelos" %>
<%@ Register TagPrefix="uc" TagName="DashboardResumen" Src="~/Paginas/Controles/DashboardResumen.ascx" %>
<asp:Content ID="ContentHomeWebMaster" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <h1 runat="server" meta:resourcekey="TituloHome">Panel de Web Master</h1>
        <p runat="server" meta:resourcekey="DescripcionHome">Mantenimiento técnico de la plataforma Operativ.</p>
    </div>
    <uc:DashboardResumen ID="ucDashboardResumen" runat="server" />
    <asp:Panel ID="pnlIntegridadCorrupta" runat="server" CssClass="modal-overlay activo" Visible="false">
        <div class="modal-caja">
            <div class="modal-encabezado">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>
                </span>
                <div>
                    <h2><asp:Literal runat="server" Text="<%$ Resources:Textos, TituloIntegridadCorrupta %>" /></h2>
                    <p><asp:Literal runat="server" Text="<%$ Resources:Textos, DescripcionIntegridadCorrupta %>" /></p>
                </div>
            </div>

            <div class="detalle-integridad">
                <asp:Repeater ID="rptFallasIntegridad" runat="server">
                    <ItemTemplate>
                        <div class="detalle-integridad-tabla">
                            <span class="detalle-integridad-nombre"><%# Eval("NombreTabla") %></span>
                            <span class="detalle-integridad-filas"><%# ObtenerDetalleFalla((ResultadoVerificacionTabla)Container.DataItem) %></span>
                            <span class="detalle-integridad-dvv"><%# ObtenerDetalleDvv((ResultadoVerificacionTabla)Container.DataItem) %></span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="modal-acciones">
                <asp:Button ID="btnRecalcular" runat="server" CssClass="btn-primario" CausesValidation="false"
                    Text="<%$ Resources:Textos, BotonRecalcularDigitos %>" OnClick="btnRecalcular_Click" />
            </div>
        </div>
    </asp:Panel>
</asp:Content>
```

### 4.5 `Operativ.Web/Paginas/Home/HomeWebMaster.aspx.designer.cs` — contenido completo

```csharp
namespace Operativ.Web.Paginas;
public partial class HomeWebMaster
{
    protected global::System.Web.UI.WebControls.Panel pnlIntegridadCorrupta;

    protected global::System.Web.UI.WebControls.Repeater rptFallasIntegridad;

    protected global::System.Web.UI.WebControls.Button btnRecalcular;
}
```

### 4.6 `Operativ.Web/Paginas/Home/HomeWebMaster.aspx.cs` — contenido completo

```csharp
using System;
using System.Collections.Generic;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class HomeWebMaster : PaginaSeguraBase
{
    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;

    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilWebMaster }; }
    }

    public HomeWebMaster()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarModalIntegridad();
        }
    }

    protected void btnRecalcular_Click(object sender, EventArgs e)
    {
        try
        {
            integridadService.RepararBaseDatos();
            bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);

            SesionHandler.CerrarSesion();
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?recalculado=1");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected string ObtenerDetalleFalla(ResultadoVerificacionTabla resultado)
    {
        if (resultado.ClavesFilasInvalidas.Count > 0)
        {
            string formato = (string)GetGlobalResourceObject("Textos", "MensajeFilasAfectadasIntegridad");
            return string.Format(formato, string.Join(", ", resultado.ClavesFilasInvalidas));
        }

        return (string)GetGlobalResourceObject("Textos", "MensajeCantidadRegistrosNoCoincide");
    }

    protected string ObtenerDetalleDvv(ResultadoVerificacionTabla resultado)
    {
        string formato = (string)GetGlobalResourceObject("Textos", "MensajeDetalleDvv");
        return string.Format(formato, resultado.ValorDvvAlmacenado, resultado.ValorDvvCalculado);
    }

    private void CargarModalIntegridad()
    {
        List<ResultadoVerificacionTabla> fallas = SesionHandler.GetFallasIntegridad();

        if (fallas == null || fallas.Count == 0)
        {
            return;
        }

        pnlIntegridadCorrupta.Visible = true;
        rptFallasIntegridad.DataSource = fallas;
        rptFallasIntegridad.DataBind();
    }
}
```

`pnlIntegridadCorrupta.Visible` se setea solo en el `!IsPostBack` y se mantiene solo en el postback del botón porque `Visible` de un `Panel` viaja en ViewState — no hace falta volver a bindear el Repeater al hacer click en "Recalcular" (de hecho ese click termina en un redirect).

---

## 5. Recursos (`Textos.resx` / `Textos.en.resx`)

**Modificar** el valor de `MensajeErrorIntegridadCorrupta` en ambos archivos (se le saca el `{0}` y la mención al acceso de emergencia — ver 1.4). El código pasa a llamarlo sin parámetros, así que si quedara un `{0}` el `string.Format` no se ejecutaría y el placeholder se mostraría crudo:

```xml
<!-- Textos.resx -->
<data name="MensajeErrorIntegridadCorrupta" xml:space="preserve">
  <value>Ha ocurrido un problema en el sistema. Intentá nuevamente más tarde o contactá a soporte técnico.</value>
</data>
```
```xml
<!-- Textos.en.resx -->
<data name="MensajeErrorIntegridadCorrupta" xml:space="preserve">
  <value>A problem occurred in the system. Please try again later or contact technical support.</value>
</data>
```

**Agregar** en `Textos.resx`:

```xml
<data name="TituloIntegridadCorrupta" xml:space="preserve">
  <value>Problema de integridad detectado</value>
</data>
<data name="DescripcionIntegridadCorrupta" xml:space="preserve">
  <value>Se detectó una alteración en la integridad de los datos del sistema. Revisá el detalle y recalculá los dígitos verificadores para restablecerla. Al recalcular se cerrará esta sesión de emergencia y volverás al inicio de sesión.</value>
</data>
<data name="MensajeFilasAfectadasIntegridad" xml:space="preserve">
  <value>Registros alterados: {0}</value>
</data>
<data name="MensajeCantidadRegistrosNoCoincide" xml:space="preserve">
  <value>No coincide la cantidad de registros; posible alta o baja fuera del sistema.</value>
</data>
<data name="MensajeDetalleDvv" xml:space="preserve">
  <value>DVV almacenado: {0} — calculado: {1}</value>
</data>
<data name="BotonRecalcularDigitos" xml:space="preserve">
  <value>Recalcular</value>
</data>
<data name="MensajeExitoRecalculoDigitos" xml:space="preserve">
  <value>Los dígitos verificadores fueron recalculados correctamente. Iniciá sesión nuevamente.</value>
</data>
```

**Agregar** en `Textos.en.resx`:

```xml
<data name="TituloIntegridadCorrupta" xml:space="preserve">
  <value>Integrity problem detected</value>
</data>
<data name="DescripcionIntegridadCorrupta" xml:space="preserve">
  <value>A data integrity violation was detected in the system. Review the detail and recalculate the check digits to restore it. Recalculating will close this emergency session and take you back to the login.</value>
</data>
<data name="MensajeFilasAfectadasIntegridad" xml:space="preserve">
  <value>Altered records: {0}</value>
</data>
<data name="MensajeCantidadRegistrosNoCoincide" xml:space="preserve">
  <value>The record count does not match; a row may have been inserted or deleted outside the system.</value>
</data>
<data name="MensajeDetalleDvv" xml:space="preserve">
  <value>Stored DVV: {0} — calculated: {1}</value>
</data>
<data name="BotonRecalcularDigitos" xml:space="preserve">
  <value>Recalculate</value>
</data>
<data name="MensajeExitoRecalculoDigitos" xml:space="preserve">
  <value>Check digits were recalculated successfully. Please log in again.</value>
</data>
```

`TituloReparacionExitosa` y `DescripcionReparacionExitosa` quedan sin uso en ambos archivos. Se pueden dejar (no rompen nada) o borrar; mismo criterio que se aplicó con `MensajeValidacionFamiliaObligatoria` cuando la familia pasó a ser opcional. `BotonAceptar` **no** se toca (se usa en otros lugares).

---

## 6. Estilos (`Operativ.Web/Estilos/operativ.css`)

**Borrar** la regla que queda huérfana al desaparecer el panel de emergencia:

```css
.panel-emergencia {
    margin-top: 20px;
    border-top-color: #b3261e;
}
```

**Agregar** el bloque de detalle del modal, después de `.caja-info-requisitos` (que es el bloque informativo equivalente del otro modal, así queda cerca de lo que se le parece):

```css
.detalle-integridad {
    margin: 8px 0 20px 0;
    padding: 16px;
    background-color: #fbe2e2;
    border: 1px solid #f2b6b6;
    border-radius: 8px;
    max-height: 260px;
    overflow-y: auto;
}

.detalle-integridad-tabla {
    padding: 10px 0;
    border-bottom: 1px solid #f2b6b6;
}

.detalle-integridad-tabla:first-child {
    padding-top: 0;
}

.detalle-integridad-tabla:last-child {
    padding-bottom: 0;
    border-bottom: none;
}

.detalle-integridad-nombre {
    display: block;
    font-size: 14px;
    font-weight: 600;
    color: var(--color-error);
}

.detalle-integridad-filas {
    display: block;
    margin-top: 2px;
    font-size: 13px;
    color: var(--color-texto);
    overflow-wrap: break-word;
    word-wrap: break-word;
}

.detalle-integridad-dvv {
    display: block;
    margin-top: 2px;
    font-size: 12px;
    color: var(--color-texto-secundario);
}
```

El `max-height` con scroll propio es a propósito: si se alteraron muchas filas, la lista puede ser larga, y sin eso el `.modal-caja` (que ya tiene `max-height: 90vh`) empujaría el botón "Recalcular" fuera de la vista.

---

## 7. Pasos para Claude Code

1. `git checkout main && git pull`
2. `git checkout -b hotfix/parche-2.2.1-integridad-login`
3. Aplicar los archivos de las secciones 2 a 4 (BE → SEC → Web).
4. Aplicar los recursos (sección 5) y el CSS (sección 6).
5. Confirmar que no quedaron referencias muertas: buscar en la solución `SufijoRedireccion`, `pnlAccesoEmergencia`, `btnIngresoEmergencia`, `pnlLoginNormal`, `pnlReparacionExitosa`, `btnAceptarReparacion`, `panel-emergencia`, `VerificarIntegridadSistema` y `reparado=1` — todos tienen que haber desaparecido.
6. Commits sugeridos:
   - `refactor(be): reemplaza SufijoRedireccion por EsAccesoEmergencia en ResultadoAutenticacion`
   - `refactor(sec): saca la reparacion de la base del login de emergencia`
   - `feat(sec): guarda las fallas de integridad en sesion`
   - `fix(web): verifica la integridad durante el login y muestra un mensaje genérico`
   - `feat(web): muestra el detalle de integridad y el boton recalcular en el modal del Web Master`
   - `refactor(web): elimina el panel de acceso de emergencia del login`
7. Build completo: `MSBuild.exe Operativ.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"`.
8. Pruebas manuales. Para corromper la integridad a mano, con la app levantada, hacer un `UPDATE` directo por SSMS sobre una fila sin pasar por el sistema (por ejemplo `UPDATE Usuario SET Email = 'alterado@test.com' WHERE NombreUsuario = 'cliente';`) — eso deja el DVH de esa fila desalineado. Para el otro caso (cantidad de registros), un `DELETE` directo de una fila de `UsuarioPatente`.
   - **Base íntegra:** loguearse con `admin` → entra normal a su Home, sin ningún mensaje. Confirmar que la pantalla de login **no** muestra nada de integridad al cargar (antes lo hacía en el `Page_Load`).
   - **Base corrupta, usuario normal con credenciales correctas:** loguearse con `admin` → **no** entra, y el mensaje es el genérico (`ERR04 - Ha ocurrido un problema en el sistema...`), sin nombres de tabla ni filas. Verificar en la tabla `Bitacora` que sí quedó el asiento `IntegridadCorrupta` con el detalle completo, y que se puede ver desde la página de bitácora entrando después con `webmaster`.
   - **Base corrupta, credenciales incorrectas:** loguearse con `admin` y una contraseña mal → muestra el error de contraseña incorrecta con los intentos restantes, igual que siempre (no el mensaje de integridad, y no revela nada de emergencia).
   - **Base corrupta, credenciales de emergencia:** usar el usuario/contraseña del XML de `App_Data/AccesoEmergencia.xml` en el **mismo** formulario de login (ya no hay panel aparte) → entra a `HomeWebMaster` y se abre el modal con el detalle completo: nombre de cada tabla afectada, las filas alteradas (o el mensaje de cantidad que no coincide) y los valores de DVV. Confirmar que la base **todavía no** se reparó (los DVH siguen mal en SSMS).
   - Cerrar el modal con la tecla de escape o navegando a otra página del WebMaster y volver al Home → el modal se vuelve a abrir (la base sigue corrupta).
   - Click en "Recalcular" → cierra sesión, vuelve al login con el mensaje verde de éxito; en SSMS los DVH/DVV quedan alineados y aparece el asiento `ReparacionEmergenciaBaseDatos` en `Bitacora`.
   - Volver a loguearse con `admin` → ahora entra normal.
   - **Base íntegra, credenciales de emergencia:** probar el usuario del XML con la base ya reparada → debe fallar como un login cualquiera ("el usuario no existe"), sin dejar entrar y sin insinuar que existe una vía de emergencia.
9. No mergear a `main`. Dejar la rama lista con los commits para que el dueño del repo la revise y haga el push.
