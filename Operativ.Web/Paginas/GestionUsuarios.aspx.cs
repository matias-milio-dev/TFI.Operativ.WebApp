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

                if (idUsuario == 0)
                {
                    usuarioService.AltaUsuario(txtNombreUsuarioAlta.Text.Trim(), txtNombreCompleto.Text.Trim(), txtEmail.Text.Trim(), idFamilia);
                    MostrarExito("MensajeExitoAltaUsuario");
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

                    usuarioService.ModificarUsuario(usuario);
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
                usuarioService.BajaUsuario(idUsuario);
                MostrarExito("MensajeExitoBajaUsuario");
                CargarGrilla();
            }
            catch (Exception excepcion)
            {
                MostrarError(excepcion);
            }
        }

        protected void btnDesbloquear_Click(object sender, EventArgs e)
        {
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
                MostrarError(excepcion);
            }
        }

        private void CargarUsuarioParaEdicion(int idUsuario)
        {
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
                MostrarError(excepcion);
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
