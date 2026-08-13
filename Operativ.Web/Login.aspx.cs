using System;
using System.Web.UI;
using Operativ.BE.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BLL.Contratos;
using Operativ.BLL.Errores;
using Operativ.BLL.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web
{
    public partial class Login : Page
    {
        private SesionHandler sesionHandler;
        private ErroresHandler erroresHandler;

        protected void Page_Load(object sender, EventArgs e)
        {
            sesionHandler = new SesionHandler();
            erroresHandler = new ErroresHandler();

            if (!IsPostBack)
            {
                if (sesionHandler.HaySesionActiva())
                {
                    Familia perfilActivo = sesionHandler.GetPerfil();
                    Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo.Nombre));
                }

                if (Request.QueryString["err"] == "sesion")
                {
                    ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(TipoError.ErrorSesionExpirada));
                }
            }
        }

        protected void btnIngresar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                FabricaNegocio fabricaNegocio = new FabricaNegocio();

                IUsuarioNegocio usuarioNegocio = fabricaNegocio.CrearUsuarioNegocio();
                Usuario usuario = usuarioNegocio.ValidarCredenciales(
                    txtNombreUsuario.Text.Trim(),
                    txtContrasena.Text);

                IFamiliaNegocio familiaNegocio = fabricaNegocio.CrearFamiliaNegocio();
                Familia perfil = familiaNegocio.GetPerfilDeUsuario(usuario.IdUsuario);
                FamiliaCompuesto arbolPermisos = familiaNegocio.ArmarArbolPermisos(usuario.IdUsuario);

                sesionHandler.IniciarSesion(usuario, perfil, arbolPermisos);

                Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfil.Nombre), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception excepcion)
            {
                OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
                ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
            }
        }
    }
}
