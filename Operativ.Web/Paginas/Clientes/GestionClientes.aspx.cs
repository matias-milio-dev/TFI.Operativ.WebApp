using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class GestionClientes : PaginaSeguraBase
{
    private const string ComandoEditar = "Editar";
    private const string ComandoBaja = "Baja";

    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaClientes;
    private readonly IClienteService clienteService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.GestionarClientes }; }
    }

    public GestionClientes()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        clienteService = fabricaNegocio.CrearClienteService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

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

    protected void btnNuevaEmpresa_Click(object sender, EventArgs e)
    {
        PrepararAlta();
    }

    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        pnlFormularioCliente.Visible = false;
    }

    protected void gvClientes_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int idCliente = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == ComandoEditar)
        {
            CargarClienteParaEdicion(idCliente);
        }
        else if (e.CommandName == ComandoBaja)
        {
            DarDeBaja(idCliente);
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
            int idCliente = Convert.ToInt32(hidIdCliente.Value);

            Cliente cliente = new Cliente
            {
                IdCliente = idCliente,
                RazonSocial = txtRazonSocial.Text.Trim(),
                Cuit = txtCuit.Text.Trim(),
                Email = txtEmailEmpresa.Text.Trim()
            };

            if (idCliente == 0)
            {
                int idUsuarioCliente = Convert.ToInt32(ddlUsuarioCliente.SelectedValue);

                clienteService.AltaCliente(cliente, idUsuarioCliente);
                ControlNotificaciones.MostrarExito("MensajeExitoAltaCliente");
            }
            else
            {
                clienteService.ModificarCliente(cliente);
                ControlNotificaciones.MostrarExito("MensajeExitoModificacionCliente");
            }

            pnlFormularioCliente.Visible = false;
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void DarDeBaja(int idCliente)
    {
        try
        {
            clienteService.BajaCliente(idCliente);
            ControlNotificaciones.MostrarExito("MensajeExitoBajaCliente");

            pnlFormularioCliente.Visible = false;
            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void CargarClienteParaEdicion(int idCliente)
    {
        try
        {
            Cliente cliente = clienteService.ObtenerClientePorId(idCliente);

            hidIdCliente.Value = cliente.IdCliente.ToString();
            txtRazonSocial.Text = cliente.RazonSocial;
            txtCuit.Text = cliente.Cuit;
            txtEmailEmpresa.Text = cliente.Email;

            MostrarPanelUsuarioInicial(false);

            tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioModificacionCliente");
            pnlFormularioCliente.Visible = true;
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void PrepararAlta()
    {
        hidIdCliente.Value = "0";
        txtRazonSocial.Text = string.Empty;
        txtCuit.Text = string.Empty;
        txtEmailEmpresa.Text = string.Empty;

        MostrarPanelUsuarioInicial(true);

        tituloFormulario.InnerText = TextoRecurso.Obtener("TituloFormularioAltaCliente");
        pnlFormularioCliente.Visible = true;
    }

    private void MostrarPanelUsuarioInicial(bool visible)
    {
        pnlUsuarioInicial.Visible = visible;
        rfvUsuarioCliente.Enabled = visible;

        if (visible)
        {
            CargarUsuariosCliente();
        }
    }

    private void CargarUsuariosCliente()
    {
        List<Usuario> usuarios = clienteService.ListarUsuariosCliente();

        ddlUsuarioCliente.Items.Clear();
        ddlUsuarioCliente.Items.Add(new ListItem(TextoRecurso.Obtener("EtiquetaUsuarioClientePlaceholder"), string.Empty));

        foreach (Usuario usuario in usuarios)
        {
            string texto = usuario.NombreUsuario + " (" + usuario.NombreCompleto + ")";

            if (!string.IsNullOrEmpty(usuario.RazonSocialCliente))
            {
                texto = texto + " - " + usuario.RazonSocialCliente;
            }

            ddlUsuarioCliente.Items.Add(new ListItem(texto, usuario.IdUsuario.ToString()));
        }
    }

    private void CargarGrilla()
    {
        string filtro = txtFiltro.Text.Trim();

        List<Cliente> clientes = clienteService.ListarClientes(filtro, ucPaginador.NumeroPagina, tamanioPagina);
        int total = clienteService.ContarClientes(filtro);

        gvClientes.DataSource = clientes;
        gvClientes.DataBind();

        ucPaginador.Actualizar(total, clientes.Count);
    }
}
