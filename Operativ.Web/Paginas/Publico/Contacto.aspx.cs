using System;
using System.Web.UI.WebControls;
using Operativ.Web.Idioma;
using Operativ.Web.Master;

namespace Operativ.Web.Paginas;
public partial class Contacto : PaginaBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarAsuntos();
        }
    }

    protected void btnEnviar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        Publico master = (Publico)Master;
        master.ControlNotificaciones.MostrarExito("MensajeExitoEnvioConsulta");

        LimpiarFormulario();
    }

    private void CargarAsuntos()
    {
        ddlAsunto.Items.Clear();
        ddlAsunto.Items.Add(new ListItem(TextoRecurso.Obtener("PlaceholderAsuntoContacto"), string.Empty));
        ddlAsunto.Items.Add(new ListItem(TextoRecurso.Obtener("AsuntoDeviceAsAService"), "DeviceAsAService"));
        ddlAsunto.Items.Add(new ListItem(TextoRecurso.Obtener("AsuntoGestionActivos"), "GestionActivos"));
        ddlAsunto.Items.Add(new ListItem(TextoRecurso.Obtener("AsuntoSoporteTecnico"), "SoporteTecnico"));
        ddlAsunto.Items.Add(new ListItem(TextoRecurso.Obtener("AsuntoConsultoria"), "Consultoria"));
        ddlAsunto.Items.Add(new ListItem(TextoRecurso.Obtener("AsuntoOtro"), "Otro"));
    }

    private void LimpiarFormulario()
    {
        txtNombreCompleto.Text = string.Empty;
        txtEmail.Text = string.Empty;
        txtEmpresa.Text = string.Empty;
        txtMensaje.Text = string.Empty;
        ddlAsunto.SelectedIndex = 0;
    }
}
