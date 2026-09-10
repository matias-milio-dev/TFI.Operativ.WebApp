# Plan: parche de simplicidad y orden en los code-behind — Operativ (2.3)

**Repo:** `matias-milio-dev/TFI.Operativ.WebApp`, base `main` (HEAD auditado: `4fbc2150a034925755863eaadcb1736f1a069591` — "muestra el detalle de integridad y el boton recalcular en el modal del Web Master"; el hotfix 2.2.1 ya está mergeado).
**Rama a crear:** `refactor/parche-2.3-simplicidad-code-behind`.
**No hacer merge ni push a `main`.** Dejar los commits listos en la rama local para que el dueño del repo los revise y los suba él mismo.

**Este parche no cambia ningún comportamiento observable.** Es puramente de orden y de reducción de ruido. Nada de base de datos, nada de markup `.aspx`/`.ascx`, nada de `.designer.cs`, nada de recursos `.resx`, nada de CSS/JS. Si algo se ve o se comporta distinto después de aplicarlo, es un bug del parche.

Recordatorio de estándares (`Estandares_Codigo_y_Estilo_Operativ.md`): sin comentarios en C#, sin `var`, sin tuplas/record/LINQ/lambdas, `namespace X;` scoped, ifs con llaves, servicios solo vía fábricas.

---

## 0. Objetivo

1. Aplicar en todos los code-behind (`.aspx.cs` y `.ascx.cs`) el orden de miembros:
   1. Miembros de clase (campos, constantes, propiedades)
   2. Miembros de clase overridados
   3. Constructor
   4. Métodos overridados
   5. Métodos públicos (los eventos que consume el `.aspx`)
   6. Métodos privados
2. Donde se identifiquen oportunidades de acortar el código **sin perder legibilidad**, acortarlo.

---

## 1. Diseño y decisiones

### 1.1 Sobre el punto "métodos públicos (eventos de los controles aspx)"

En este proyecto los handlers de eventos están declarados `protected`, no `public`, y así deben quedar: Web Forms los resuelve por nombre desde el markup y no necesita visibilidad pública; subirlos a `public` ampliaría la superficie de la clase sin ningún beneficio. Entonces el grupo 5 se interpreta como **"los miembros que consume el `.aspx`"** (handlers de eventos + helpers llamados desde `<%# %>`), manteniendo su `protected` actual. El grupo 6 son los `private` de verdad.

`Page_Load` cae en el grupo 5: es un handler de evento, no un override. Los overrides reales de este proyecto son `OnInit`, `OnPreRender` y `AplicarVisibilidadPorPatentes`, y van **arriba** de `Page_Load`.

### 1.2 Violaciones de orden encontradas

| Archivo | Problema |
| --- | --- |
| `GestionUsuarios.aspx.cs` | `AplicarVisibilidadPorPatentes` (override, grupo 4) está enterrado entre `gvUsuarios_RowDataBound` y `btnGuardar_Click` (grupo 5). La propiedad privada `NumeroPagina` (grupo 1) está debajo de la override `PatentesPermitidas` (grupo 2). |
| `BackupRestore.aspx.cs` | `AplicarVisibilidadPorPatentes` (grupo 4) está debajo de `Page_Load` (grupo 5). |
| `ConsultarBitacora.aspx.cs` | `NumeroPagina` (grupo 1) está debajo de `PatentesPermitidas` (grupo 2). |
| `Principal.Master.cs` | `Page_Load` vacío (además de estar fuera de orden respecto de la propiedad, es código muerto — se borra). |

Ya cumplen el orden y **no se reordenan**: `Login.aspx.cs`, `PermisosUsuario.aspx.cs`, `HomeWebMaster.aspx.cs`, `RecuperarContrasena.aspx.cs`, `PaginaSeguraBase.cs`, `Notificaciones.ascx.cs`, `ResumenUsuario.ascx.cs`, `ModalCambiarClave.ascx.cs`, `SelectorIdioma.ascx.cs`, `Navbar.ascx.cs`, `NoAutorizado.aspx.cs`, `SinFamilia.aspx.cs`, `HomeAdministrador/Cliente/Comercial.aspx.cs`.

Sobre las clases privadas anidadas (`GrupoPermisos` e `ItemPermiso` en `PermisosUsuario`): la convención no habla de tipos anidados. Se dejan al final del archivo, después de los métodos privados, que es donde ya están.

### 1.3 Las tres oportunidades grandes de acortar

**(a) `(string)GetGlobalResourceObject("Textos", clave)` aparece 24 veces.** Casi siempre en el patrón de tres líneas:

```csharp
string formato = (string)GetGlobalResourceObject("Textos", "MensajeUsuarioBloqueado");
litMensajeBloqueado.Text = string.Format(formato, usuario.NombreUsuario);
```

Se agrega un helper estático `TextoRecurso` con `Obtener(clave)` y `Formato(clave, valores)`, y eso pasa a ser una línea:

```csharp
litMensajeBloqueado.Text = TextoRecurso.Formato("MensajeUsuarioBloqueado", usuario.NombreUsuario);
```

Va como clase estática y no como método de `PaginaBase` porque los `UserControl` (`Notificaciones`, `ResumenUsuario`) no heredan de `PaginaBase` y también lo necesitan. Usa `HttpContext.GetGlobalResourceObject`, que resuelve por `CurrentUICulture` — la misma cultura que ya setea `PaginaBase.InitializeCulture`, así que el comportamiento multi-idioma no cambia.

**(b) `ValidarPatente` está duplicado idéntico en `GestionUsuarios` y `BackupRestore`.** Sube a `PaginaSeguraBase`, que es exactamente donde corresponde: ya tiene `AutorizacionHandler` y `ControlNotificaciones`, y la revalidación de patente del lado servidor es una preocupación de seguridad, no de cada página.

**(c) El cálculo de `desde`/`hasta` del paginado está duplicado** entre `GestionUsuarios` y `ConsultarBitacora`, con la misma aritmética de off-by-one (`((NumeroPagina - 1) * tamanioPagina) + 1`). Se extrae a un helper estático `Paginado.FormatearResumen(...)`.

Deliberadamente **no** se sube todo el paginado (la propiedad `NumeroPagina` ni los `Enabled` de los botones) a `PaginaSeguraBase`: esa clase es la base de **todas** las páginas seguras y la mayoría no paginan; meterle estado de grilla le rompe la cohesión. Lo que se extrae es solo la aritmética, que es la parte que se puede equivocar; la asignación a los controles queda local y legible en cada página.

### 1.4 Dos switches que se reemplazan por convención de nombres

`ConsultarBitacora` tiene dos `switch` de 4 casos (14 líneas cada uno) sobre `CriticidadBitacora`:

- `ObtenerClaseCriticidad`: devuelve `"badge-informativo"`, `"badge-advertencia"`, `"badge-critico"`, `"badge-grave"` → es exactamente `"badge-" + criticidad.ToString().ToLowerInvariant()`.
- `ObtenerTextoCriticidad`: devuelve la clave de recurso `EtiquetaCriticidadInformativo`, `...Advertencia`, `...Critico`, `...Grave` → es exactamente `"EtiquetaCriticidad" + criticidad.ToString()`.

Los dos pasan de 14 líneas a 1. **El trade-off, explícito:** se cambia un mapeo escrito a mano por un mapeo por convención de nombres. Si mañana se agrega un valor al enum, antes había que agregar un `case` (y sin él caía en el `default`); ahora hay que agregar la clase CSS y la clave de recurso con el nombre que corresponde (y sin ellas, la clase CSS no existiría y el texto vendría `null`). Es la misma cantidad de trabajo, corrido de lugar. Se acepta porque los nombres de CSS y de recurso **ya fueron derivados** del nombre del enum cuando se escribieron: la convención ya existe de hecho, esto solo la hace explícita.

En la misma clase, `CargarFiltros` agrega los 4 items de criticidad con 4 líneas copiadas; pasa a un `foreach` sobre `Enum.GetValues`, que además no se puede desincronizar del enum.

### 1.5 `TienePatenteIndividual`: un `foreach` de búsqueda dentro de dos loops anidados

`PermisosUsuario.ArmarGrupos` recorre categorías × patentes, y por cada patente llama a `TienePatenteIndividual`, que a su vez recorre la lista de patentes individuales. Se reemplaza construyendo **una vez** una `List<int>` de ids y usando `Contains` (que es un método propio de `List<T>`, no LINQ). Desaparece el método helper, el código queda más corto y de paso deja de ser cuadrático.

Lo mismo aplica a `ObtenerIdsFamiliasComoTexto`, que armaba una `List<string>` a mano para después hacer `string.Join`: `string.Join` tiene overload genérico para `IEnumerable<int>` desde .NET Framework 4.0, así que son 10 líneas menos.

### 1.6 Cosas que se dejan como están, a propósito

- **El `catch (Exception) { }` vacío de `ResumenUsuario.lnkCerrarSesion_Click`.** Parece un olor, pero está protegiendo algo real: si la base está caída, el usuario tiene que poder cerrar sesión igual, y el asiento en bitácora es lo prescindible. Se deja.
- **`GestionUsuarios.CargarGrilla` mantiene su guarda de `ConsultarUsuario`** aunque `AplicarVisibilidadPorPatentes` ya oculte el listado: ocultar un panel no impide que el postback llegue, y la página se gatea por **toda** la categoría de patentes, así que el escenario "entré sin poder consultar" existe de verdad.
- **`ObtenerAccionSeleccionada` / `ObtenerCriticidadSeleccionada`** quedan como dos métodos separados. Unificarlos pediría un genérico con `Enum.Parse` sobre `typeof(T)`, que es más corto pero bastante menos obvio de leer — justo lo que este parche no quiere.
- **`ModalCambiarClave`, `SelectorIdioma`, `Navbar`, `RecuperarContrasena`, `NoAutorizado`, `SinFamilia`, las tres Home simples, `Error`, `Footer`, `DashboardResumen`**: no se tocan. O ya están mínimos, o el único cambio posible sería cosmético sin ganancia.

---

## 2. Archivos nuevos

### 2.1 `Operativ.Web/Idioma/TextoRecurso.cs` — contenido completo (archivo nuevo)

Va en `Operativ.Web/Idioma/`, junto a `IdiomaHelper`, que es la carpeta donde ya vive lo relacionado con cultura y textos.

```csharp
using System.Web;

namespace Operativ.Web.Idioma;
public static class TextoRecurso
{
    private const string ArchivoRecursos = "Textos";

    public static string Obtener(string clave)
    {
        return (string)HttpContext.GetGlobalResourceObject(ArchivoRecursos, clave);
    }

    public static string Formato(string clave, params object[] valores)
    {
        return string.Format(Obtener(clave), valores);
    }
}
```

### 2.2 `Operativ.Web/Paginas/Paginado.cs` — contenido completo (archivo nuevo)

```csharp
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public static class Paginado
{
    public static string FormatearResumen(string claveRecurso, int numeroPagina, int tamanioPagina, int total, int cantidadEnPagina)
    {
        int desde = total == 0 ? 0 : ((numeroPagina - 1) * tamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        return TextoRecurso.Formato(claveRecurso, desde, hasta, total);
    }
}
```

---

## 3. Clases base y controles compartidos

### 3.1 `Operativ.Web/Paginas/PaginaSeguraBase.cs` — contenido completo

Cambios: se agrega `ValidarPatente` (grupo 5, porque lo consumen las páginas hijas como parte de su flujo de eventos) y se compacta la declaración de las propiedades para que respiren igual que el resto.

```csharp
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Handlers;
using Operativ.Web.Controles;
using Operativ.Web.Master;

namespace Operativ.Web.Paginas;
public abstract class PaginaSeguraBase : PaginaBase
{
    protected SesionHandler SesionHandler { get; private set; }

    protected AutorizacionHandler AutorizacionHandler { get; private set; }

    protected Notificaciones ControlNotificaciones
    {
        get { return ((Principal)Master).ControlNotificaciones; }
    }

    protected virtual string[] PerfilesPermitidos
    {
        get { return new string[0]; }
    }

    protected virtual string[] PatentesPermitidas
    {
        get { return new string[0]; }
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

    protected bool ValidarPatente(string nombrePatente)
    {
        if (AutorizacionHandler.TienePatente(nombrePatente))
        {
            return true;
        }

        ControlNotificaciones.MostrarMensaje(TipoError.ErrorSinPermiso, new string[] { nombrePatente });
        return false;
    }

    private void ValidarAcceso()
    {
        if (!SesionHandler.HaySesionActiva())
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?err=sesion");
        }

        if (PatentesPermitidas.Length > 0)
        {
            if (!AutorizacionHandler.TieneAlgunaPatente(PatentesPermitidas))
            {
                Response.Redirect("~/Paginas/Comun/NoAutorizado.aspx");
            }

            return;
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

### 3.2 `Operativ.Web/Paginas/Controles/Notificaciones.ascx.cs` — contenido completo

Cambios: usa `TextoRecurso` y gana un overload `MostrarExito(clave, valores)`, que es lo que hoy obliga a `BackupRestore` a armar el `string.Format` a mano.

```csharp
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.Web.Idioma;

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

    public void MostrarExito(string claveRecurso)
    {
        MostrarMensaje(TextoRecurso.Obtener(claveRecurso), true);
    }

    public void MostrarExito(string claveRecurso, params object[] valores)
    {
        MostrarMensaje(TextoRecurso.Formato(claveRecurso, valores), true);
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

### 3.3 `Operativ.Web/Paginas/Controles/ResumenUsuario.ascx.cs` — contenido completo

Cambios: `sesionHandler` pasa a ser `readonly` y se inicializa en el constructor, junto a la otra dependencia. Antes se asignaba en `Page_Load` y se usaba en `lnkCerrarSesion_Click`: funciona porque `Page_Load` corre antes que los eventos, pero es una dependencia de orden implícita e innecesaria.

```csharp
using System;
using System.Web.UI;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Idioma;

namespace Operativ.Web.Controles;
public partial class ResumenUsuario : UserControl
{
    private readonly IBitacoraService bitacoraService;
    private readonly SesionHandler sesionHandler;

    public ResumenUsuario()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Usuario usuario = sesionHandler.GetUsuario();

        if (usuario == null)
        {
            Visible = false;
            return;
        }

        Familia perfil = sesionHandler.GetPerfil();
        string nombrePerfil = perfil != null
            ? "<strong>" + perfil.Nombre + "</strong>"
            : TextoRecurso.Obtener("EtiquetaSinFamilia");

        lblBienvenida.Text = TextoRecurso.Formato("MensajeBienvenida", usuario.NombreUsuario, nombrePerfil);
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

### 3.4 `Operativ.Web/Master/Principal.Master.cs` — contenido completo

Se borra el `Page_Load` vacío (y con él el `using System;`, que queda sin uso).

```csharp
using System.Web.UI;
using Operativ.Web.Controles;

namespace Operativ.Web.Master;
public partial class Principal : MasterPage
{
    public Notificaciones ControlNotificaciones
    {
        get { return ucNotificaciones; }
    }
}
```

---

## 4. Páginas

### 4.1 `Operativ.Web/Paginas/Usuarios/GestionUsuarios.aspx.cs` — contenido completo

Cambios: `NumeroPagina` sube al grupo 1; `AplicarVisibilidadPorPatentes` sube al grupo 4; se borra `ValidarPatente` (ahora en la base); los tres `if (btnX.Visible) { ... }` se vuelven una línea con `&&`; la elección de patente alta/modificación se extrae a `ObtenerPatenteGuardar()` (estaba duplicada en dos métodos); el "combo vacío → null" se extrae a `ObtenerIdFamiliaSeleccionada(ddl)` (estaba escrito de dos formas distintas, un `if` en un lado y un ternario inline en el otro); todos los accesos a recursos pasan por `TextoRecurso`.

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class GestionUsuarios : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaUsuarios;
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;

    private int NumeroPagina
    {
        get { return ViewState["NumeroPagina"] == null ? 1 : (int)ViewState["NumeroPagina"]; }
        set { ViewState["NumeroPagina"] = value; }
    }

    protected override string[] PatentesPermitidas
    {
        get { return CategoriaPatente.Usuarios.NombresPatente; }
    }

    public GestionUsuarios()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
    }

    protected override void AplicarVisibilidadPorPatentes()
    {
        bool puedeConsultar = AutorizacionHandler.TienePatente(NombrePatente.ConsultarUsuario);
        pnlFiltros.Visible = puedeConsultar;
        pnlListado.Visible = puedeConsultar;

        btnNuevoUsuario.Visible = AutorizacionHandler.TienePatente(NombrePatente.AltaUsuario);
        btnGuardar.Visible = btnGuardar.Visible && AutorizacionHandler.TienePatente(ObtenerPatenteGuardar());
        btnBloquear.Visible = btnBloquear.Visible && AutorizacionHandler.TienePatente(NombrePatente.BloqueoUsuario);
        btnDesbloquear.Visible = btnDesbloquear.Visible && AutorizacionHandler.TienePatente(NombrePatente.DesbloqueoUsuario);
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

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid || !ValidarPatente(ObtenerPatenteGuardar()))
        {
            return;
        }

        try
        {
            int idUsuario = Convert.ToInt32(hidIdUsuario.Value);
            int? idFamilia = ObtenerIdFamiliaSeleccionada(ddlFamilia);

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
            MostrarPanelEdicion(usuarioService.ObtenerUsuarioPorId(idUsuario));
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
            MostrarPanelDesbloqueo(usuarioService.ObtenerUsuarioPorId(idUsuario));
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private string ObtenerPatenteGuardar()
    {
        return hidIdUsuario.Value == "0" ? NombrePatente.AltaUsuario : NombrePatente.ModificacionUsuario;
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

        hidIdUsuario.Value = usuario.IdUsuario.ToString();
        litMensajeBloqueado.Text = TextoRecurso.Formato("MensajeUsuarioBloqueado", usuario.NombreUsuario);
        tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioModificacion");

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

        tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioModificacion");

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

        tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioAlta");
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

        ddl.Items.Insert(0, new ListItem(TextoRecurso.Obtener(claveTextoPlaceholder), string.Empty));
    }

    private void CargarGrilla()
    {
        if (!AutorizacionHandler.TienePatente(NombrePatente.ConsultarUsuario))
        {
            return;
        }

        string filtro = txtFiltro.Text.Trim();
        int? idFamilia = ObtenerIdFamiliaSeleccionada(ddlFiltroFamilia);

        List<Usuario> usuarios = usuarioService.ListarUsuarios(filtro, idFamilia, NumeroPagina, tamanioPagina);
        int total = usuarioService.ContarUsuarios(filtro, idFamilia);

        gvUsuarios.DataSource = usuarios;
        gvUsuarios.DataBind();

        ActualizarResumenPaginado(total, usuarios.Count);
    }

    private int? ObtenerIdFamiliaSeleccionada(DropDownList ddl)
    {
        if (string.IsNullOrEmpty(ddl.SelectedValue))
        {
            return null;
        }

        return Convert.ToInt32(ddl.SelectedValue);
    }

    private void ActualizarResumenPaginado(int total, int cantidadEnPagina)
    {
        litResumenPaginado.Text = Paginado.FormatearResumen("MensajeResumenPaginado", NumeroPagina, tamanioPagina, total, cantidadEnPagina);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnPaginaAnterior.Enabled = NumeroPagina > 1;
        btnPaginaSiguiente.Enabled = (NumeroPagina * tamanioPagina) < total;
    }
}
```

Dos detalles del diff que conviene mirar con atención al aplicar:

- `CargarUsuarioParaEdicion` ya no setea `hidIdUsuario.Value` por su cuenta: lo hacían **también** `MostrarPanelEdicion` y `MostrarPanelDesbloqueo` justo después, con el mismo valor. Se le agregó el seteo a `MostrarPanelDesbloqueo` (que no lo tenía) y se quitó del llamador, así los dos caminos quedan simétricos y el dato se setea en un solo lugar.
- `btnGuardar_Click` combina las dos guardas en un `||` con corto circuito: si la página no es válida, `ValidarPatente` no corre, y por lo tanto no se apila un error de permisos encima de los mensajes de validación. Era el comportamiento anterior también (dos `if` separados), solo queda más corto.

### 4.2 `Operativ.Web/Paginas/Sistema/ConsultarBitacora.aspx.cs` — contenido completo

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class ConsultarBitacora : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaBitacora;
    private readonly IBitacoraService bitacoraService;

    private int NumeroPagina
    {
        get { return ViewState["NumeroPagina"] == null ? 1 : (int)ViewState["NumeroPagina"]; }
        set { ViewState["NumeroPagina"] = value; }
    }

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ConsultarBitacora }; }
    }

    public ConsultarBitacora()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarFiltros();
        }

        CargarGrilla();
    }

    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        NumeroPagina = 1;
        CargarGrilla();
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

    protected string ObtenerClaseCriticidad(CriticidadBitacora criticidad)
    {
        return "badge-" + criticidad.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoCriticidad(CriticidadBitacora criticidad)
    {
        return TextoRecurso.Obtener("EtiquetaCriticidad" + criticidad.ToString());
    }

    private void CargarFiltros()
    {
        ddlFiltroAccion.Items.Clear();
        ddlFiltroAccion.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaTodasLasAcciones"), string.Empty));

        foreach (AccionBitacora accion in AccionBitacora.ObtenerTodas())
        {
            ddlFiltroAccion.Items.Add(new ListItem(accion.Descripcion.Replace("{0}", "N"), accion.Tipo.ToString()));
        }

        ddlFiltroCriticidad.Items.Clear();
        ddlFiltroCriticidad.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaTodasLasCriticidades"), string.Empty));

        foreach (CriticidadBitacora criticidad in Enum.GetValues(typeof(CriticidadBitacora)))
        {
            ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(criticidad), criticidad.ToString()));
        }
    }

    private void CargarGrilla()
    {
        string filtroUsuario = txtFiltroUsuario.Text.Trim();
        TipoAccionBitacora? accion = ObtenerAccionSeleccionada();
        CriticidadBitacora? criticidad = ObtenerCriticidadSeleccionada();
        DateTime? fechaDesde = ObtenerFecha(txtFechaDesde.Text);
        DateTime? fechaHasta = ObtenerFecha(txtFechaHasta.Text);

        List<Bitacora> registros = bitacoraService.Buscar(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta, NumeroPagina, tamanioPagina);
        int total = bitacoraService.ContarRegistros(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta);

        gvBitacora.DataSource = registros;
        gvBitacora.DataBind();

        ActualizarResumenPaginado(total, registros.Count);
    }

    private TipoAccionBitacora? ObtenerAccionSeleccionada()
    {
        if (string.IsNullOrEmpty(ddlFiltroAccion.SelectedValue))
        {
            return null;
        }

        return (TipoAccionBitacora)Enum.Parse(typeof(TipoAccionBitacora), ddlFiltroAccion.SelectedValue);
    }

    private CriticidadBitacora? ObtenerCriticidadSeleccionada()
    {
        if (string.IsNullOrEmpty(ddlFiltroCriticidad.SelectedValue))
        {
            return null;
        }

        return (CriticidadBitacora)Enum.Parse(typeof(CriticidadBitacora), ddlFiltroCriticidad.SelectedValue);
    }

    private DateTime? ObtenerFecha(string texto)
    {
        DateTime fecha;

        if (!DateTime.TryParse(texto, out fecha))
        {
            return null;
        }

        return fecha;
    }

    private void ActualizarResumenPaginado(int total, int cantidadEnPagina)
    {
        litResumenPaginado.Text = Paginado.FormatearResumen("MensajeResumenPaginadoBitacora", NumeroPagina, tamanioPagina, total, cantidadEnPagina);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnPaginaAnterior.Enabled = NumeroPagina > 1;
        btnPaginaSiguiente.Enabled = (NumeroPagina * tamanioPagina) < total;
    }
}
```

Se fue el `default` de los dos switches. En `ObtenerClaseCriticidad` no hacía falta (cualquier valor del enum produce una clase); en `ObtenerTextoCriticidad`, un valor nuevo sin su clave de recurso daría `null` en vez del `ToString()` del enum — lo cual es correcto que se note, porque significa que falta la traducción.

### 4.3 `Operativ.Web/Paginas/Sistema/BackupRestore.aspx.cs` — contenido completo

Cambios: `AplicarVisibilidadPorPatentes` sube arriba de `Page_Load`; se borra `ValidarPatente` (base); el `string.Format` a mano del mensaje de éxito usa el overload nuevo de `MostrarExito`; se unifica el bloque repetido de "restaurar y volver al login".

```csharp
using System;
using System.IO;
using System.Web.UI.WebControls;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

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

    protected override void AplicarVisibilidadPorPatentes()
    {
        btnCrearBackup.Visible = AutorizacionHandler.TienePatente(NombrePatente.RealizarBackup);
        btnRestaurarDesdeArchivo.Visible = AutorizacionHandler.TienePatente(NombrePatente.RestaurarBackup);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarGrilla();
        }
    }

    protected void btnCrearBackup_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.RealizarBackup))
        {
            return;
        }

        try
        {
            ControlNotificaciones.MostrarExito("MensajeExitoCrearBackup", backupService.CrearBackup());
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    protected void btnRestaurarDesdeArchivo_Click(object sender, EventArgs e)
    {
        if (!ValidarPatente(NombrePatente.RestaurarBackup))
        {
            return;
        }

        if (!fileuploadRestaurar.HasFile)
        {
            ControlNotificaciones.MostrarMensaje(TextoRecurso.Obtener("MensajeArchivoRestaurarObligatorio"), false);
            return;
        }

        string rutaTemporal = backupService.PrepararRutaParaSubida();

        try
        {
            fileuploadRestaurar.SaveAs(rutaTemporal);
            backupService.RestaurarBackupDesdeRuta(rutaTemporal);
            CerrarSesionYVolverAlLogin();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
        finally
        {
            if (File.Exists(rutaTemporal))
            {
                File.Delete(rutaTemporal);
            }
        }
    }

    protected void gvBackups_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string nombreArchivo = e.CommandArgument.ToString();

        if (e.CommandName == "Descargar")
        {
            DescargarArchivo(nombreArchivo);
            return;
        }

        if (e.CommandName != "Restaurar" || !ValidarPatente(NombrePatente.RestaurarBackup))
        {
            return;
        }

        try
        {
            backupService.RestaurarBackup(nombreArchivo);
            CerrarSesionYVolverAlLogin();
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

    private void CerrarSesionYVolverAlLogin()
    {
        SesionHandler.CerrarSesion();
        Response.Redirect("~/Paginas/Usuarios/Login.aspx?restaurado=1");
    }

    private void DescargarArchivo(string nombreArchivo)
    {
        string rutaCompleta = backupService.ObtenerRutaCompleta(nombreArchivo);

        if (!File.Exists(rutaCompleta))
        {
            ControlNotificaciones.MostrarMensaje(TipoError.ErrorArchivoBackupNoExiste, new string[] { nombreArchivo });
            return;
        }

        Response.Clear();
        Response.ContentType = "application/octet-stream";
        Response.AddHeader("Content-Disposition", "attachment; filename=" + nombreArchivo);
        Response.TransmitFile(rutaCompleta);
        Response.Flush();
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }

    private void CargarGrilla()
    {
        gvBackups.DataSource = backupService.ListarBackups();
        gvBackups.DataBind();
    }
}
```

Ojo con un cambio de orden real acá: en `gvBackups_RowCommand`, la guarda pasó a ser `if (e.CommandName != "Restaurar" || !ValidarPatente(...))`. Con corto circuito, si el comando no es "Restaurar" no se evalúa la patente — igual que antes, cuando eran dos `if` separados en ese mismo orden.

### 4.4 `Operativ.Web/Paginas/Usuarios/PermisosUsuario.aspx.cs` — contenido completo

Cambios: se trabaja con `List<int>` de ids en lugar de `List<Patente>` + búsqueda lineal, con lo cual desaparece `TienePatenteIndividual`; se agrega `ObtenerIds(List<Patente>)` que reemplaza dos loops idénticos de extracción de ids; `ObtenerIdsFamiliasComoTexto` usa `string.Join` directo sobre la lista de enteros; recursos por `TextoRecurso`.

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class PermisosUsuario : PaginaSeguraBase
{
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;
    private readonly IPatenteService patenteService;
    private int idUsuario;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.AsignarPatente, NombrePatente.RemoverPatente }; }
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

        lnkVolver.NavigateUrl = "~/Paginas/Usuarios/GestionUsuarios.aspx";
        txtBuscarPermiso.Attributes["placeholder"] = TextoRecurso.Obtener("EtiquetaBuscarPermiso");

        if (!IsPostBack)
        {
            CargarFiltroFamilia();
            CargarPagina();
        }
    }

    protected void rptCategorias_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
        {
            return;
        }

        GrupoPermisos grupo = (GrupoPermisos)e.Item.DataItem;

        HtmlGenericControl contenedorGrupo = (HtmlGenericControl)e.Item.FindControl("grupoPermiso");
        contenedorGrupo.Attributes["data-categoria"] = grupo.Clave;

        if (grupo.ExpandidoPorDefecto)
        {
            contenedorGrupo.Attributes["class"] = contenedorGrupo.Attributes["class"] + " expandido";
        }

        HtmlGenericControl spanTitulo = (HtmlGenericControl)e.Item.FindControl("spanTituloCategoria");
        spanTitulo.InnerText = grupo.Titulo;

        string formatoSeleccionados = TextoRecurso.Obtener("EtiquetaSeleccionadosCategoria");
        string formatoSeleccionadoSingular = TextoRecurso.Obtener("EtiquetaSeleccionadoCategoriaSingular");
        HtmlGenericControl spanSeleccionados = (HtmlGenericControl)e.Item.FindControl("spanSeleccionadosCategoria");
        spanSeleccionados.InnerText = string.Format(
            grupo.CantidadSeleccionada == 1 ? formatoSeleccionadoSingular : formatoSeleccionados,
            grupo.CantidadSeleccionada);
        spanSeleccionados.Attributes["data-formato"] = formatoSeleccionados;
        spanSeleccionados.Attributes["data-formato-singular"] = formatoSeleccionadoSingular;

        if (grupo.CantidadSeleccionada > 0)
        {
            HtmlButton btnAlternar = (HtmlButton)e.Item.FindControl("btnAlternarCategoria");
            btnAlternar.Attributes["class"] = btnAlternar.Attributes["class"] + " grupo-permiso-alternar-activo";
        }

        Repeater rptPermisos = (Repeater)e.Item.FindControl("rptPermisos");
        rptPermisos.DataSource = grupo.Permisos;
        rptPermisos.DataBind();
    }

    protected void rptPermisos_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
        {
            return;
        }

        ItemPermiso permiso = (ItemPermiso)e.Item.DataItem;

        HtmlGenericControl fila = (HtmlGenericControl)e.Item.FindControl("filaPermiso");
        fila.Attributes["data-nombre"] = permiso.Nombre.ToLowerInvariant();
        fila.Attributes["data-descripcion"] = permiso.Descripcion.ToLowerInvariant();
        fila.Attributes["data-familias"] = permiso.IdsFamilias;

        HiddenField hidIdPatente = (HiddenField)e.Item.FindControl("hidIdPatente");
        hidIdPatente.Value = permiso.IdPatente.ToString();

        CheckBox chk = (CheckBox)e.Item.FindControl("chkSeleccionada");
        chk.Checked = permiso.Seleccionada;
        chk.Enabled = permiso.Habilitada;

        HtmlGenericControl spanNombre = (HtmlGenericControl)e.Item.FindControl("spanNombrePatente");
        spanNombre.InnerText = permiso.Nombre;

        HtmlGenericControl spanDescripcion = (HtmlGenericControl)e.Item.FindControl("spanDescripcionPatente");
        spanDescripcion.InnerText = permiso.Descripcion;

        HtmlGenericControl spanBadge = (HtmlGenericControl)e.Item.FindControl("spanBadgeHeredada");
        spanBadge.InnerText = TextoRecurso.Obtener(permiso.HeredadaPorFamilia ? "EtiquetaHeredadaPorFamilia" : "EtiquetaNoHeredada");
        spanBadge.Attributes["class"] = permiso.HeredadaPorFamilia
            ? "badge-permiso badge-permiso-heredado"
            : "badge-permiso badge-permiso-no-heredado";
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            List<int> idsIndividuales = ObtenerIds(patenteService.GetPatentesIndividualesDeUsuario(idUsuario));
            List<int> idsAAsignar = new List<int>();
            List<int> idsAQuitar = new List<int>();

            foreach (RepeaterItem itemCategoria in rptCategorias.Items)
            {
                Repeater rptPermisos = (Repeater)itemCategoria.FindControl("rptPermisos");

                foreach (RepeaterItem itemPermiso in rptPermisos.Items)
                {
                    ClasificarPermisoDeFila(itemPermiso, idsIndividuales, idsAAsignar, idsAQuitar);
                }
            }

            patenteService.AsignarPatentes(idUsuario, idsAAsignar.ToArray());
            patenteService.QuitarPatentes(idUsuario, idsAQuitar.ToArray());

            ControlNotificaciones.MostrarExito("MensajeExitoPermisosUsuario");
            CargarPagina();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void ClasificarPermisoDeFila(RepeaterItem itemPermiso, List<int> idsIndividuales, List<int> idsAAsignar, List<int> idsAQuitar)
    {
        CheckBox chk = (CheckBox)itemPermiso.FindControl("chkSeleccionada");

        if (!chk.Enabled)
        {
            return;
        }

        HiddenField hidIdPatente = (HiddenField)itemPermiso.FindControl("hidIdPatente");
        int idPatente = Convert.ToInt32(hidIdPatente.Value);
        bool yaAsignada = idsIndividuales.Contains(idPatente);

        if (chk.Checked && !yaAsignada)
        {
            idsAAsignar.Add(idPatente);
        }
        else if (!chk.Checked && yaAsignada)
        {
            idsAQuitar.Add(idPatente);
        }
    }

    private void CargarFiltroFamilia()
    {
        ddlFiltroFamilia.DataSource = familiaService.ListarFamilias();
        ddlFiltroFamilia.DataTextField = "Nombre";
        ddlFiltroFamilia.DataValueField = "IdFamilia";
        ddlFiltroFamilia.DataBind();

        ddlFiltroFamilia.Items.Insert(0, new ListItem(TextoRecurso.Obtener("EtiquetaFiltrarPorFamilia"), string.Empty));
    }

    private void CargarPagina()
    {
        Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

        tituloPermisos.InnerText = TextoRecurso.Formato("TituloPermisosUsuario", usuario.NombreUsuario);

        rptCategorias.DataSource = ArmarGrupos(usuario);
        rptCategorias.DataBind();
    }

    private List<GrupoPermisos> ArmarGrupos(Usuario usuario)
    {
        List<int> idsPatentesFamilia = ObtenerIdsPatentesFamilia(usuario);
        List<int> idsIndividuales = ObtenerIds(patenteService.GetPatentesIndividualesDeUsuario(idUsuario));
        List<Patente> todasLasPatentes = patenteService.ListarTodas();
        Dictionary<int, List<int>> familiasPorPatente = ObtenerFamiliasPorPatente();

        bool puedeAsignar = AutorizacionHandler.TienePatente(NombrePatente.AsignarPatente);
        bool puedeRemover = AutorizacionHandler.TienePatente(NombrePatente.RemoverPatente);

        List<GrupoPermisos> grupos = new List<GrupoPermisos>();

        foreach (CategoriaPatente categoria in CategoriaPatente.ObtenerTodas())
        {
            GrupoPermisos grupo = new GrupoPermisos
            {
                Clave = categoria.Tipo.ToString(),
                Titulo = TextoRecurso.Obtener(categoria.ClaveRecurso),
                Permisos = new List<ItemPermiso>()
            };

            foreach (Patente patente in todasLasPatentes)
            {
                if (CategoriaPatente.ObtenerPorPatente(patente.Nombre) != categoria)
                {
                    continue;
                }

                ItemPermiso permiso = new ItemPermiso
                {
                    IdPatente = patente.IdPatente,
                    Nombre = patente.Nombre,
                    Descripcion = patente.Descripcion,
                    HeredadaPorFamilia = idsPatentesFamilia.Contains(patente.IdPatente),
                    Seleccionada = idsIndividuales.Contains(patente.IdPatente),
                    IdsFamilias = ObtenerIdsFamiliasComoTexto(familiasPorPatente, patente.IdPatente)
                };
                permiso.Habilitada = permiso.Seleccionada ? puedeRemover : puedeAsignar;

                grupo.Permisos.Add(permiso);

                if (permiso.Seleccionada)
                {
                    grupo.CantidadSeleccionada++;
                }
            }

            if (grupo.Permisos.Count > 0)
            {
                grupo.ExpandidoPorDefecto = grupos.Count == 0;
                grupos.Add(grupo);
            }
        }

        return grupos;
    }

    private Dictionary<int, List<int>> ObtenerFamiliasPorPatente()
    {
        Dictionary<int, List<int>> resultado = new Dictionary<int, List<int>>();

        foreach (Familia familia in familiaService.ListarFamilias())
        {
            foreach (Patente patente in familiaService.GetPatentesDeFamilia(familia.IdFamilia))
            {
                if (!resultado.ContainsKey(patente.IdPatente))
                {
                    resultado[patente.IdPatente] = new List<int>();
                }

                resultado[patente.IdPatente].Add(familia.IdFamilia);
            }
        }

        return resultado;
    }

    private string ObtenerIdsFamiliasComoTexto(Dictionary<int, List<int>> familiasPorPatente, int idPatente)
    {
        if (!familiasPorPatente.ContainsKey(idPatente))
        {
            return string.Empty;
        }

        return string.Join(",", familiasPorPatente[idPatente]);
    }

    private List<int> ObtenerIdsPatentesFamilia(Usuario usuario)
    {
        if (usuario.Familias.Count == 0)
        {
            return new List<int>();
        }

        return ObtenerIds(familiaService.GetPatentesDeFamilia(usuario.Familias[0].IdFamilia));
    }

    private List<int> ObtenerIds(List<Patente> patentes)
    {
        List<int> ids = new List<int>();

        foreach (Patente patente in patentes)
        {
            ids.Add(patente.IdPatente);
        }

        return ids;
    }

    private class GrupoPermisos
    {
        public string Clave;
        public string Titulo;
        public bool ExpandidoPorDefecto;
        public int CantidadSeleccionada;
        public List<ItemPermiso> Permisos;
    }

    private class ItemPermiso
    {
        public int IdPatente;
        public string Nombre;
        public string Descripcion;
        public bool HeredadaPorFamilia;
        public bool Seleccionada;
        public bool Habilitada;
        public string IdsFamilias;
    }
}
```

Nota sobre `IdsFamilias`: se movió dentro del inicializador de objeto, junto al resto de las propiedades. `Habilitada` **no** se puede mover ahí porque depende de `permiso.Seleccionada`, que se está calculando en ese mismo inicializador — queda en la línea siguiente, como estaba.

### 4.5 `Operativ.Web/Paginas/Home/HomeWebMaster.aspx.cs` — contenido completo

Solo cambia el acceso a recursos.

```csharp
using System;
using System.Collections.Generic;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

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
        if (resultado.ClavesFilasInvalidas.Count == 0)
        {
            return TextoRecurso.Obtener("MensajeCantidadRegistrosNoCoincide");
        }

        return TextoRecurso.Formato("MensajeFilasAfectadasIntegridad", string.Join(", ", resultado.ClavesFilasInvalidas));
    }

    protected string ObtenerDetalleDvv(ResultadoVerificacionTabla resultado)
    {
        return TextoRecurso.Formato("MensajeDetalleDvv", resultado.ValorDvvAlmacenado, resultado.ValorDvvCalculado);
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

### 4.6 `Operativ.Web/Paginas/Usuarios/Login.aspx.cs` — contenido completo

Cambio: se borra el helper privado `MostrarExito`, porque `Notificaciones` **ya** expone `MostrarExito(clave)` que hace exactamente lo mismo — era un concepto duplicado.

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
            ucNotificaciones.MostrarExito("MensajeExitoRestaurarBackup");
        }

        if (Request.QueryString["recalculado"] == "1")
        {
            ucNotificaciones.MostrarExito("MensajeExitoRecalculoDigitos");
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
}
```

---

## 5. Pasos para Claude Code

1. `git checkout main && git pull`
2. `git checkout -b refactor/parche-2.3-simplicidad-code-behind`
3. Crear los dos archivos nuevos de la sección 2 (`TextoRecurso.cs`, `Paginado.cs`) y agregarlos al `.csproj` de `Operativ.Web` si el proyecto no usa globbing de archivos.
4. Aplicar la sección 3 (base y controles compartidos) y después la sección 4 (páginas).
5. Verificar que no quedó ninguna referencia a los miembros eliminados: buscar en la solución `GetGlobalResourceObject` (solo debería quedar dentro de `TextoRecurso`), `ValidarPatente` (definido solo en `PaginaSeguraBase`), `TienePatenteIndividual` (no debería existir), y `MostrarExito(` en `Login` (debe ser sobre `ucNotificaciones`, no un método propio).
6. Commits sugeridos:
   - `refactor(web): agrega TextoRecurso para los accesos a recursos globales`
   - `refactor(web): sube ValidarPatente a PaginaSeguraBase`
   - `refactor(web): extrae la aritmetica del resumen de paginado`
   - `refactor(web): ordena los miembros de los code-behind segun la convencion`
   - `refactor(web): simplifica ConsultarBitacora, PermisosUsuario y BackupRestore`
   - `refactor(web): elimina el Page_Load vacio del master`
7. Build completo: `MSBuild.exe Operativ.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"`. Debe compilar sin warnings nuevos (atención a `using` que quedaron sin uso: `Operativ.SEC.Configuracion` sigue usándose, pero `System.Web.UI` en `Principal.Master.cs` sí se mantiene por `MasterPage`).
8. Pruebas de regresión — el objetivo es confirmar que **nada** cambió:
   - **Gestión de usuarios:** listar, filtrar por texto y por familia, paginar adelante y atrás (verificar el texto "Mostrando X a Y de Z usuarios" en la primera, una intermedia y la última página), alta, modificación, baja, bloqueo, desbloqueo, y el scroll automático al panel del formulario.
   - Con un usuario sin `AltaUsuario`: el botón "Nuevo usuario" no aparece. Sin `ConsultarUsuario`: no aparecen filtros ni listado. Sin `BajaUsuario`: no aparece el botón en las filas.
   - **Permisos de usuario:** abrir la página, ver los badges "Heredado por familia"/"No heredado" correctos, el contador "N seleccionados" por categoría (probar el caso de exactamente 1 para verificar el singular), el buscador y el filtro por familia del lado cliente, tildar y destildar, guardar, y confirmar contra la base que se asignó/quitó lo esperado.
   - **Bitácora:** filtrar por cada uno de los 5 filtros y combinados, paginar, verificar que los badges de criticidad siguen saliendo con el mismo color (los 4 valores) y el texto traducido, y que el combo de acciones muestra "N" en la acción con placeholder.
   - **Cambiar el idioma a inglés** y repetir un par de pantallas: es el chequeo clave de que `TextoRecurso` resuelve la misma cultura que antes.
   - **Backup:** crear un backup, descargar, restaurar desde la grilla, restaurar subiendo un archivo, y el caso de subir sin seleccionar archivo (mensaje de obligatorio).
   - **Login e integridad:** login normal, login con base corrupta (mensaje genérico), login de emergencia con el XML, modal con el detalle y botón Recalcular.
   - **Cerrar sesión** desde el encabezado (verifica el cambio de `sesionHandler` a `readonly` en `ResumenUsuario`).
9. No mergear a `main`. Dejar la rama lista con los commits para que el dueño del repo la revise y haga el push.
