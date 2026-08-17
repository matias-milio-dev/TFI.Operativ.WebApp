using System;
using Operativ.BE.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web
{
    public partial class Login : PaginaBase
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
                FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();

                IUsuarioService usuarioService = fabricaSeguridad.CrearUsuarioService();
                Usuario usuario = usuarioService.ValidarCredenciales(
                    txtNombreUsuario.Text.Trim(),
                    txtContrasena.Text);

                IFamiliaService familiaService = fabricaSeguridad.CrearFamiliaService();
                Familia perfil = familiaService.GetPerfilDeUsuario(usuario.IdUsuario);
                FamiliaCompuesto arbolPermisos = familiaService.ArmarArbolPermisos(usuario.IdUsuario);

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
