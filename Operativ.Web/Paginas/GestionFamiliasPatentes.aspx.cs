using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.SEC;

namespace Operativ.Web.Paginas
{
    public partial class GestionFamiliasPatentes : PaginaBase
    {
        protected override string PatenteRequerida => "FAMILIA_ABM";

        private readonly IFamiliaBLL _familiaBLL = FabricaBLL.Instancia.CrearFamiliaBLL();
        private readonly IPermisosBLL _permisosBLL = FabricaBLL.Instancia.CrearPermisosBLL();

        protected Literal litTitulo;
        protected TextBox txtNombreFamilia;
        protected TextBox txtDescripcionFamilia;
        protected Repeater rptFamilias;
        protected Panel pnlPatentes;
        protected Literal litFamiliaSeleccionada;
        protected CheckBoxList cblPatentes;
        protected HiddenField hdnIdFamiliaSeleccionada;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuFamilias");
                CargarFamilias();
            }
        }

        private void CargarFamilias()
        {
            rptFamilias.DataSource = _familiaBLL.Listar();
            rptFamilias.DataBind();
        }

        protected void btnCrearFamilia_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                _familiaBLL.Alta(txtNombreFamilia.Text.Trim(), txtDescripcionFamilia.Text.Trim());
                txtNombreFamilia.Text = string.Empty;
                txtDescripcionFamilia.Text = string.Empty;
                CargarFamilias();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }

        protected void rptFamilias_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Seleccionar") return;

            int idFamilia = Convert.ToInt32(e.CommandArgument);
            CargarPatentesDeFamilia(idFamilia);
        }

        private void CargarPatentesDeFamilia(int idFamilia)
        {
            var familia = _familiaBLL.Obtener(idFamilia);
            var idsAsignados = _permisosBLL.ListarIdsPatentesDeFamilia(idFamilia);
            var todasLasPatentes = _permisosBLL.ListarTodasLasPatentes();

            cblPatentes.Items.Clear();
            foreach (var patente in todasLasPatentes)
            {
                var item = new ListItem($"[{patente.Modulo}] {patente.Nombre}", patente.IdPatente.ToString())
                {
                    Selected = idsAsignados.Contains(patente.IdPatente)
                };
                cblPatentes.Items.Add(item);
            }

            hdnIdFamiliaSeleccionada.Value = idFamilia.ToString();
            litFamiliaSeleccionada.Text = familia.Nombre;
            pnlPatentes.Visible = true;
        }

        protected void btnGuardarPatentes_Click(object sender, EventArgs e)
        {
            int idFamilia = Convert.ToInt32(hdnIdFamiliaSeleccionada.Value);
            var idsAsignadosPrevios = _permisosBLL.ListarIdsPatentesDeFamilia(idFamilia);

            try
            {
                GestorAutorizacion.RequerirPatente("PATENTE_ASIGNAR");

                foreach (ListItem item in cblPatentes.Items)
                {
                    int idPatente = Convert.ToInt32(item.Value);
                    bool estabaAsignada = idsAsignadosPrevios.Contains(idPatente);

                    if (item.Selected
                        && !estabaAsignada)
                    {
                        _permisosBLL.AsignarPatenteAFamilia(idFamilia, idPatente);
                    }
                    else if (!item.Selected
                        && estabaAsignada)
                    {
                        _permisosBLL.RemoverPatenteDeFamilia(idFamilia, idPatente);
                    }
                }

                ((Master.SiteMaster)Master).MostrarExito("Patentes de la familia actualizadas correctamente.");
                CargarPatentesDeFamilia(idFamilia);
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }
    }
}
