using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;

namespace Operativ.Web.Paginas
{
    public partial class Servicios : PaginaBase
    {
        private readonly ICatalogoBLL _catalogoBLL = FabricaBLL.Instancia.CrearCatalogoBLL();

        protected Literal litTitulo;
        protected TextBox txtFiltro;
        protected GridView gvCatalogo;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuServicios");
                Buscar();
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e) => Buscar();

        private void Buscar()
        {
            var catalogo = _catalogoBLL.Consultar(string.IsNullOrWhiteSpace(txtFiltro.Text) ? null : txtFiltro.Text.Trim());
            gvCatalogo.DataSource = catalogo.Paquetes;
            gvCatalogo.DataBind();
        }
    }
}
