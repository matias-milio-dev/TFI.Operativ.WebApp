using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
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
    private readonly IClienteService clienteService;

    protected override string[] PatentesPermitidas
    {
        get { return CategoriaPatente.Usuarios.NombresPatente; }
    }

    public GestionUsuarios()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();

        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        clienteService = fabricaNegocio.CrearClienteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

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

    protected void ucPaginador_PaginaCambiada(object sender, EventArgs e)
    {
        CargarGrilla();
    }

    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        ucPaginador.Reiniciar();
        CargarGrilla();
    }

    protected void btnNuevoUsuario_Click(object sender, EventArgs e)
    {
        PrepararAlta();
        MostrarPanelConFoco(txtNombreUsuarioAlta);
    }

    protected void ddlFamilia_SelectedIndexChanged(object sender, EventArgs e)
    {
        AplicarVisibilidadEmpresa(null);
    }

    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        PrepararAlta();
        pnlFormularioUsuario.Visible = false;
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
            int? idFamilia = ObtenerIdFamiliaSeleccionada(ddlFamilia);
            int? idCliente = ObtenerIdClienteSeleccionado();

            if (idUsuario == 0)
            {
                usuarioService.AltaUsuario(txtNombreUsuarioAlta.Text.Trim(), txtNombreCompleto.Text.Trim(), txtEmail.Text.Trim(), idFamilia, idCliente);
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

                usuarioService.ModificarUsuario(usuario, idFamilia, idCliente);
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
        btnDesbloquear.Visible = true;

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

        btnGuardarAlta.Visible = false;
        btnGuardarModificacion.Visible = true;
        btnBloquear.Visible = true;

        ddlFamilia.SelectedIndex = 0;

        if (usuario.Familias.Count > 0)
        {
            ddlFamilia.SelectedValue = usuario.Familias[0].IdFamilia.ToString();
        }

        AplicarVisibilidadEmpresa(usuario.IdCliente);

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
        AplicarVisibilidadEmpresa(null);

        btnGuardarAlta.Visible = true;
        btnGuardarModificacion.Visible = false;
        btnBloquear.Visible = false;

        pnlDesbloqueo.Visible = false;
        pnlCamposEdicion.Visible = true;

        tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioAlta");
    }

    private void AplicarVisibilidadEmpresa(int? idClienteSeleccionado)
    {
        bool esFamiliaCliente = EsFamiliaClienteSeleccionada();

        pnlCliente.Visible = esFamiliaCliente;

        if (!esFamiliaCliente)
        {
            return;
        }

        CargarClientes(idClienteSeleccionado);

        if (idClienteSeleccionado.HasValue)
        {
            ddlCliente.SelectedValue = idClienteSeleccionado.Value.ToString();
        }
    }

    private bool EsFamiliaClienteSeleccionada()
    {
        if (string.IsNullOrEmpty(ddlFamilia.SelectedValue))
        {
            return false;
        }

        return ddlFamilia.SelectedItem.Text == NombreFamilia.Cliente;
    }

    private void CargarClientes(int? idClienteIncluir)
    {
        List<Cliente> clientes = clienteService.ListarClientesActivos(idClienteIncluir);

        ddlCliente.Items.Clear();
        ddlCliente.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaEmpresaPlaceholder"), string.Empty));

        foreach (Cliente cliente in clientes)
        {
            ddlCliente.Items.Add(new ListItem(cliente.RazonSocial, cliente.IdCliente.ToString()));
        }
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

        List<Usuario> usuarios = usuarioService.ListarUsuarios(filtro, idFamilia, ucPaginador.NumeroPagina, tamanioPagina);
        int total = usuarioService.ContarUsuarios(filtro, idFamilia);

        gvUsuarios.DataSource = usuarios;
        gvUsuarios.DataBind();

        ucPaginador.Actualizar(total, usuarios.Count);
    }

    private int? ObtenerIdClienteSeleccionado()
    {
        if (!pnlCliente.Visible)
        {
            return null;
        }

        return ObtenerIdFamiliaSeleccionada(ddlCliente);
    }

    private int? ObtenerIdFamiliaSeleccionada(DropDownList ddl)
    {
        if (string.IsNullOrEmpty(ddl.SelectedValue))
        {
            return null;
        }

        return Convert.ToInt32(ddl.SelectedValue);
    }
}
