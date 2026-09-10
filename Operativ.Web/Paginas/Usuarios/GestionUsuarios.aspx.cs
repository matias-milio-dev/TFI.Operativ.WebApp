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
