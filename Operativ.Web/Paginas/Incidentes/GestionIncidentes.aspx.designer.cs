namespace Operativ.Web.Paginas;
public partial class GestionIncidentes
{
    protected global::System.Web.UI.WebControls.Panel pnlFiltros;

    protected global::System.Web.UI.WebControls.TextBox txtFiltro;

    protected global::System.Web.UI.WebControls.DropDownList ddlFiltroEstado;

    protected global::System.Web.UI.WebControls.LinkButton btnBuscar;

    protected global::System.Web.UI.WebControls.LinkButton btnNuevoIncidente;

    protected global::System.Web.UI.WebControls.Panel pnlListado;

    protected global::System.Web.UI.WebControls.GridView gvIncidentes;

    protected global::Operativ.Web.Controles.Paginador ucPaginador;

    protected global::System.Web.UI.WebControls.Panel pnlFormularioIncidente;

    protected global::System.Web.UI.WebControls.DropDownList ddlActivo;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvActivo;

    protected global::System.Web.UI.WebControls.DropDownList ddlCategoria;

    protected global::System.Web.UI.WebControls.DropDownList ddlPrioridad;

    protected global::System.Web.UI.WebControls.TextBox txtDescripcion;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvDescripcion;

    protected global::System.Web.UI.WebControls.LinkButton btnGuardar;

    protected global::System.Web.UI.WebControls.LinkButton btnCancelarAlta;

    protected global::System.Web.UI.WebControls.ValidationSummary vsIncidente;

    protected global::System.Web.UI.WebControls.Panel pnlDetalleIncidente;

    protected global::System.Web.UI.HtmlControls.HtmlGenericControl tituloDetalle;

    protected global::System.Web.UI.WebControls.HiddenField hidIdIncidente;

    protected global::System.Web.UI.WebControls.Literal litActivo;

    protected global::System.Web.UI.WebControls.Literal litCliente;

    protected global::System.Web.UI.WebControls.Literal litCategoria;

    protected global::System.Web.UI.WebControls.Literal litPrioridad;

    protected global::System.Web.UI.WebControls.Literal litEstado;

    protected global::System.Web.UI.WebControls.Literal litFechaAlta;

    protected global::System.Web.UI.WebControls.Literal litDescripcion;

    protected global::System.Web.UI.WebControls.Panel pnlResolucion;

    protected global::System.Web.UI.WebControls.Literal litFechaCierre;

    protected global::System.Web.UI.WebControls.Literal litComentarioResolucion;

    protected global::System.Web.UI.WebControls.Panel pnlCierre;

    protected global::System.Web.UI.WebControls.TextBox txtComentarioResolucion;

    protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvComentarioResolucion;

    protected global::System.Web.UI.WebControls.LinkButton btnConfirmarCierre;

    protected global::System.Web.UI.WebControls.ValidationSummary vsCierre;

    protected global::System.Web.UI.WebControls.LinkButton btnCerrarDetalle;
}
