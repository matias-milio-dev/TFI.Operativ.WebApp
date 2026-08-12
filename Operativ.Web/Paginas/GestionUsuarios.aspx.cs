using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE;
using Operativ.BLL;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.SEC;

namespace Operativ.Web.Paginas
{
    public partial class GestionUsuarios : PaginaBase
    {
        protected override string PatenteRequerida => "USUARIO_LISTAR";

        private readonly IUsuarioBLL _usuarioBLL = FabricaBLL.Instancia.CrearUsuarioBLL();

        protected Literal litTitulo;
        protected TextBox txtFiltro;
        protected GridView gvUsuarios;
        protected Panel pnlFormulario;
        protected Literal litSubtitulo;
        protected HiddenField hdnIdUsuario;
        protected TextBox txtNombreUsuario;
        protected TextBox txtNombreCompleto;
        protected TextBox txtCorreo;
        protected DropDownList ddlPerfil;
        protected DropDownList ddlIdioma;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litTitulo.Text = (string)GetGlobalResourceObject("Textos", "MenuUsuarios");
                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            gvUsuarios.DataSource = _usuarioBLL.ListarUsuarios(txtFiltro.Text.Trim(), 1, 50);
            gvUsuarios.DataBind();
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            pnlFormulario.Visible = true;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlFormulario.Visible = false;
        }

        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idUsuario = Convert.ToInt32(gvUsuarios.DataKeys[Convert.ToInt32(e.CommandArgument)].Value);

            switch (e.CommandName)
            {
                case "Editar":
                    CargarFormularioParaEdicion(idUsuario);
                    break;
                case "Baja":
                    EjecutarConManejoDeErrores(() =>
                    {
                        GestorAutorizacion.RequerirPatente("USUARIO_BAJA");
                        _usuarioBLL.BajaUsuario(idUsuario);
                    });
                    CargarGrilla();
                    break;
                case "Desbloquear":
                    EjecutarConManejoDeErrores(() =>
                    {
                        GestorAutorizacion.RequerirPatente("USUARIO_MODIFICAR");
                        _usuarioBLL.DesbloquearUsuario(idUsuario);
                    });
                    CargarGrilla();
                    break;
            }
        }

        private void CargarFormularioParaEdicion(int idUsuario)
        {
            Usuario usuario = _usuarioBLL.ObtenerUsuario(idUsuario);
            hdnIdUsuario.Value = usuario.IdUsuario.ToString();
            txtNombreUsuario.Text = usuario.NombreUsuario;
            txtNombreUsuario.Enabled = false;
            txtNombreCompleto.Text = usuario.NombreCompleto;
            txtCorreo.Text = usuario.CorreoElectronico;
            ddlPerfil.SelectedValue = usuario.CodigoPerfil;
            ddlIdioma.SelectedValue = usuario.IdiomaPreferido;
            pnlFormulario.Visible = true;
        }

        private void LimpiarFormulario()
        {
            hdnIdUsuario.Value = "0";
            txtNombreUsuario.Text = string.Empty;
            txtNombreUsuario.Enabled = true;
            txtNombreCompleto.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            ddlPerfil.SelectedIndex = 0;
            ddlIdioma.SelectedIndex = 0;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            var perfil = (PerfilUsuario)Enum.Parse(typeof(PerfilUsuario), ddlPerfil.SelectedValue, ignoreCase: true);
            int idUsuario = Convert.ToInt32(hdnIdUsuario.Value);

            EjecutarConManejoDeErrores(() =>
            {
                if (idUsuario == 0)
                {
                    GestorAutorizacion.RequerirPatente("USUARIO_ALTA");
                    _usuarioBLL.AltaUsuario(txtNombreUsuario.Text.Trim(), txtNombreCompleto.Text.Trim(),
                        txtCorreo.Text.Trim(), perfil, ddlIdioma.SelectedValue, out string claveTemporal);
                    ((Master.SiteMaster)Master).MostrarExito($"Usuario creado. Contraseña temporal: {claveTemporal}");
                }
                else
                {
                    GestorAutorizacion.RequerirPatente("USUARIO_MODIFICAR");
                    _usuarioBLL.ModificarUsuario(new Usuario
                    {
                        IdUsuario = idUsuario,
                        NombreCompleto = txtNombreCompleto.Text.Trim(),
                        CorreoElectronico = txtCorreo.Text.Trim(),
                        IdPerfil = (int)perfil,
                        IdiomaPreferido = ddlIdioma.SelectedValue
                    });
                    ((Master.SiteMaster)Master).MostrarExito("Usuario actualizado correctamente.");
                }
            });

            pnlFormulario.Visible = false;
            CargarGrilla();
        }

        private void EjecutarConManejoDeErrores(Action accion)
        {
            try
            {
                accion();
            }
            catch (ExcepcionNegocio excepcionNegocio)
            {
                ((Master.SiteMaster)Master).MostrarMensaje(excepcionNegocio.CodigoError);
            }
        }
    }
}
