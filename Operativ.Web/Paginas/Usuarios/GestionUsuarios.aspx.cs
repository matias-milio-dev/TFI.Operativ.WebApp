using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class GestionUsuarios : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaUsuarios;
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;

    protected override string[] PatentesPermitidas
    {
        get { return NombrePatente.ModuloUsuarios; }
    }

    private int NumeroPagina
    {
        get { return ViewState["NumeroPagina"] == null ? 1 : (int)ViewState["NumeroPagina"]; }
        set { ViewState["NumeroPagina"] = value; }
    }

    public GestionUsuarios()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
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

    protected override void AplicarVisibilidadPorPatentes()
    {
        bool puedeConsultar = AutorizacionHandler.TienePatente(NombrePatente.ConsultarUsuario);
        pnlFiltros.Visible = puedeConsultar;
        pnlListado.Visible = puedeConsultar;

        btnNuevoUsuario.Visible = AutorizacionHandler.TienePatente(NombrePatente.AltaUsuario);

        if (btnGuardar.Visible)
        {
            bool esAlta = hidIdUsuario.Value == "0";
            string patenteRequerida = esAlta ? NombrePatente.AltaUsuario : NombrePatente.ModificacionUsuario;
            btnGuardar.Visible = AutorizacionHandler.TienePatente(patenteRequerida);
        }

        if (btnBloquear.Visible)
        {
            btnBloquear.Visible = AutorizacionHandler.TienePatente(NombrePatente.BloqueoUsuario);
        }

        if (btnDesbloquear.Visible)
        {
            btnDesbloquear.Visible = AutorizacionHandler.TienePatente(NombrePatente.DesbloqueoUsuario);
        }
    }

    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        int idUsuario = Convert.ToInt32(hidIdUsuario.Value);
        bool esAlta = idUsuario == 0;
        string patenteRequerida = esAlta ? NombrePatente.AltaUsuario : NombrePatente.ModificacionUsuario;

        if (!ValidarPatente(patenteRequerida))
        {
            return;
        }

        try
        {
            int? idFamilia = string.IsNullOrEmpty(ddlFamilia.SelectedValue) ? (int?)null : Convert.ToInt32(ddlFamilia.SelectedValue);

            if (esAlta)
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

            Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);
            MostrarPanelEdicion(usuario);

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

            Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);
            MostrarPanelDesbloqueo(usuario);

            CargarGrilla();
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private bool ValidarPatente(string nombrePatente)
    {
        if (AutorizacionHandler.TienePatente(nombrePatente))
        {
            return true;
        }

        ControlNotificaciones.MostrarMensaje(TipoError.ErrorSinPermiso, new string[] { nombrePatente });
        return false;
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

            hidIdUsuario.Value = usuario.IdUsuario.ToString();

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

        string formato = (string)GetGlobalResourceObject("Textos", "MensajeUsuarioBloqueado");
        litMensajeBloqueado.Text = string.Format(formato, usuario.NombreUsuario);

        tituloFormulario.InnerText = (string)GetGlobalResourceObject("Textos", "TituloFormularioModificacion");

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

        tituloFormulario.InnerText = (string)GetGlobalResourceObject("Textos", "TituloFormularioModificacion");

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

        tituloFormulario.InnerText = (string)GetGlobalResourceObject("Textos", "TituloFormularioAlta");
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

        string textoPlaceholder = (string)GetGlobalResourceObject("Textos", claveTextoPlaceholder);
        ddl.Items.Insert(0, new ListItem(textoPlaceholder, string.Empty));
    }

    private void CargarGrilla()
    {
        if (!AutorizacionHandler.TienePatente(NombrePatente.ConsultarUsuario))
        {
            return;
        }

        string filtro = txtFiltro.Text.Trim();
        int? idFamilia = ObtenerIdFamiliaFiltro();

        List<Usuario> usuarios = usuarioService.ListarUsuarios(filtro, idFamilia, NumeroPagina, tamanioPagina);
        int total = usuarioService.ContarUsuarios(filtro, idFamilia);

        gvUsuarios.DataSource = usuarios;
        gvUsuarios.DataBind();

        ActualizarResumenPaginado(total, usuarios.Count);
    }

    private int? ObtenerIdFamiliaFiltro()
    {
        if (string.IsNullOrEmpty(ddlFiltroFamilia.SelectedValue))
        {
            return null;
        }

        return Convert.ToInt32(ddlFiltroFamilia.SelectedValue);
    }

    private void ActualizarResumenPaginado(int total, int cantidadEnPagina)
    {
        int desde = total == 0 ? 0 : ((NumeroPagina - 1) * tamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        string formato = (string)GetGlobalResourceObject("Textos", "MensajeResumenPaginado");
        litResumenPaginado.Text = string.Format(formato, desde, hasta, total);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnPaginaAnterior.Enabled = NumeroPagina > 1;
        btnPaginaSiguiente.Enabled = (NumeroPagina * tamanioPagina) < total;
    }
}
