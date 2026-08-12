using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;

namespace Operativ.Web.Paginas
{
    public partial class ConsultaBitacora : PaginaBase
    {
        private const int TamanioPagina = 25;

        protected override string PatenteRequerida => "BITACORA_CONSULTAR";

        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        protected Literal litTitulo;
        protected TextBox txtFechaDesde;
        protected TextBox txtFechaHasta;
        protected TextBox txtAccion;
        protected DropDownList ddlCriticidad;
        protected GridView gvBitacora;
        protected Literal litPagina;

        private int NumeroPaginaActual
        {
            get => ViewState["NumeroPagina"] as int? ?? 1;
            set => ViewState["NumeroPagina"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuBitacora");
                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            DateTime? fechaDesde = string.IsNullOrEmpty(txtFechaDesde.Text) ? (DateTime?)null : DateTime.Parse(txtFechaDesde.Text);
            DateTime? fechaHasta = string.IsNullOrEmpty(txtFechaHasta.Text) ? (DateTime?)null : DateTime.Parse(txtFechaHasta.Text).AddDays(1).AddTicks(-1);
            string accion = string.IsNullOrWhiteSpace(txtAccion.Text) ? null : txtAccion.Text.Trim().ToUpperInvariant();
            string criticidad = string.IsNullOrEmpty(ddlCriticidad.SelectedValue) ? null : ddlCriticidad.SelectedValue;

            var registros = _bitacoraBLL.Listar(fechaDesde, fechaHasta, null, accion, criticidad, NumeroPaginaActual, TamanioPagina);

            gvBitacora.DataSource = registros;
            gvBitacora.DataBind();

            litPagina.Text = $"Página {NumeroPaginaActual}";
            btnAnterior.Enabled = NumeroPaginaActual > 1;
            btnSiguiente.Enabled = registros.Count == TamanioPagina;
        }

        protected Button btnAnterior;
        protected Button btnSiguiente;

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            NumeroPaginaActual = 1;
            CargarGrilla();
        }

        protected void btnAnterior_Click(object sender, EventArgs e)
        {
            NumeroPaginaActual = Math.Max(1, NumeroPaginaActual - 1);
            CargarGrilla();
        }

        protected void btnSiguiente_Click(object sender, EventArgs e)
        {
            NumeroPaginaActual++;
            CargarGrilla();
        }
    }
}
