using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Master;

namespace Operativ.Web.Paginas
{
    public partial class GestionUsuarios : PaginaSeguraBase
    {
        private const int TamanioPagina = 10;

        private readonly ErroresHandler erroresHandler = new ErroresHandler();
        private readonly IUsuarioService usuarioService;
        private readonly IFamiliaService familiaService;

        protected override string PerfilRequerido
        {
            get { return NavegacionHelper.PerfilAdministrador; }
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
                CargarFamilias();
                CargarFiltroFamilias();
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
            PrepararAlta();
            MostrarPanelConFoco(txtNombreUsuario);
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

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                int idUsuario = Convert.ToInt32(hidIdUsuario.Value);
                int idFamilia = Convert.ToInt32(ddlFamilia.SelectedValue);
                int idUsuarioEjecutor = SesionHandler.GetUsuario().IdUsuario;

                if (idUsuario == 0)
                {
                    usuarioService.AltaUsuario(txtNombreUsuario.Text.Trim(), txtNombreCompleto.Text.Trim(), txtEmail.Text.Trim(), idFamilia, idUsuarioEjecutor);
                    MostrarExito("MensajeExitoAltaUsuario");
                }
                else
                {
                    Usuario usuario = new Usuario
                    {
                        IdUsuario = idUsuario,
                        NombreUsuario = txtNombreUsuario.Text.Trim(),
                        NombreCompleto = txtNombreCompleto.Text.Trim(),
                        Email = txtEmail.Text.Trim()
                    };

                    usuarioService.ModificarUsuario(usuario, idUsuarioEjecutor);
                    MostrarExito("MensajeExitoModificacionUsuario");
                }

                PrepararAlta();
                pnlFormularioUsuario.Visible = false;
                CargarGrilla();
            }
            catch (Exception excepcion)
            {
                MostrarError(excepcion);
            }
        }

        private void DarDeBaja(int idUsuario)
        {
            try
            {
                int idUsuarioEjecutor = SesionHandler.GetUsuario().IdUsuario;

                usuarioService.BajaUsuario(idUsuario, idUsuarioEjecutor);
                MostrarExito("MensajeExitoBajaUsuario");
                CargarGrilla();
            }
            catch (Exception excepcion)
            {
                MostrarError(excepcion);
            }
        }

        private void CargarUsuarioParaEdicion(int idUsuario)
        {
            try
            {
                Usuario usuario = usuarioService.ObtenerUsuarioPorId(idUsuario);

                hidIdUsuario.Value = usuario.IdUsuario.ToString();
                txtNombreUsuario.Text = usuario.NombreUsuario;
                txtNombreUsuario.ReadOnly = true;
                txtNombreCompleto.Text = usuario.NombreCompleto;
                txtEmail.Text = usuario.Email;

                if (usuario.Familias.Count > 0)
                {
                    ddlFamilia.SelectedValue = usuario.Familias[0].IdFamilia.ToString();
                }

                tituloFormulario.InnerText = (string)GetGlobalResourceObject("Textos", "TituloFormularioModificacion");

                MostrarPanelConFoco(txtNombreCompleto);
            }
            catch (Exception excepcion)
            {
                MostrarError(excepcion);
            }
        }

        private void PrepararAlta()
        {
            hidIdUsuario.Value = "0";
            txtNombreUsuario.Text = string.Empty;
            txtNombreUsuario.ReadOnly = false;
            txtNombreCompleto.Text = string.Empty;
            txtEmail.Text = string.Empty;
            ddlFamilia.SelectedIndex = 0;

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

        private void CargarFamilias()
        {
            List<Familia> familias = familiaService.ListarFamilias();

            ddlFamilia.DataSource = familias;
            ddlFamilia.DataTextField = "Nombre";
            ddlFamilia.DataValueField = "IdFamilia";
            ddlFamilia.DataBind();

            string textoPlaceholder = (string)GetGlobalResourceObject("Textos", "EtiquetaFamiliaPlaceholder");
            ddlFamilia.Items.Insert(0, new ListItem(textoPlaceholder, string.Empty));
        }

        private void CargarFiltroFamilias()
        {
            List<Familia> familias = familiaService.ListarFamilias();

            ddlFiltroFamilia.DataSource = familias;
            ddlFiltroFamilia.DataTextField = "Nombre";
            ddlFiltroFamilia.DataValueField = "IdFamilia";
            ddlFiltroFamilia.DataBind();

            string textoTodas = (string)GetGlobalResourceObject("Textos", "EtiquetaTodasLasFamilias");
            ddlFiltroFamilia.Items.Insert(0, new ListItem(textoTodas, string.Empty));
        }

        private void CargarGrilla()
        {
            string filtro = txtFiltro.Text.Trim();
            int? idFamilia = ObtenerIdFamiliaFiltro();

            List<Usuario> usuarios = usuarioService.ListarUsuarios(filtro, idFamilia, NumeroPagina, TamanioPagina);
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
            int desde = total == 0 ? 0 : ((NumeroPagina - 1) * TamanioPagina) + 1;
            int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

            string formato = (string)GetGlobalResourceObject("Textos", "MensajeResumenPaginado");
            litResumenPaginado.Text = string.Format(formato, desde, hasta, total);
            litNumeroPagina.Text = NumeroPagina.ToString();

            btnPaginaAnterior.Enabled = NumeroPagina > 1;
            btnPaginaSiguiente.Enabled = (NumeroPagina * TamanioPagina) < total;
        }

        protected string ObtenerBadgeHtml(object dataItem)
        {
            Usuario usuario = (Usuario)dataItem;

            if (usuario.Familias == null || usuario.Familias.Count == 0)
            {
                return "<span class=\"badge\">-</span>";
            }

            string nombreFamilia = usuario.Familias[0].Nombre;
            string claseBadge = ObtenerClaseBadge(nombreFamilia);

            return string.Format("<span class=\"badge {0}\">{1}</span>", claseBadge, Server.HtmlEncode(nombreFamilia));
        }

        private string ObtenerClaseBadge(string nombreFamilia)
        {
            switch (nombreFamilia)
            {
                case NavegacionHelper.PerfilAdministrador:
                    return "badge-administrador";
                case NavegacionHelper.PerfilCliente:
                    return "badge-cliente";
                case NavegacionHelper.PerfilComercial:
                    return "badge-comercial";
                case NavegacionHelper.PerfilWebMaster:
                    return "badge-webmaster";
                default:
                    return string.Empty;
            }
        }

        private void MostrarExito(string claveRecurso)
        {
            string mensaje = (string)GetGlobalResourceObject("Textos", claveRecurso);
            ((Principal)Master).ControlNotificaciones.MostrarMensaje(mensaje, true);
        }

        private void MostrarError(Exception excepcion)
        {
            OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
            ((Principal)Master).ControlNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
        }
    }
}
