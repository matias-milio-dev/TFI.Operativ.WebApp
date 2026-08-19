using System;
using System.Collections.Generic;
using Operativ.BE.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.SEC.Helpers;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class Login : PaginaBase
{
    private SesionHandler sesionHandler;
    private ErroresHandler erroresHandler;
    private bool modoEmergencia;

    protected void Page_Load(object sender, EventArgs e)
    {
        sesionHandler = new SesionHandler();
        erroresHandler = new ErroresHandler();

        if (!IsPostBack && sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo.Nombre));
        }

        VerificarIntegridadSistema();

        if (!IsPostBack && Request.QueryString["err"] == "sesion")
        {
            ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(TipoError.ErrorSesionExpirada));
        }
    }

    private void VerificarIntegridadSistema()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();

        try
        {
            IIntegridadService integridadService = fabricaSeguridad.CrearIntegridadService();
            List<ResultadoVerificacionTabla> resultadosInvalidos = integridadService.VerificarIntegridad();

            modoEmergencia = resultadosInvalidos.Count > 0;

            if (modoEmergencia)
            {
                string detalle = integridadService.FormatearResumenFallas(resultadosInvalidos);
                ucNotificaciones.MostrarMensaje(
                    erroresHandler.GetMensaje(TipoError.ErrorIntegridadCorrupta, new string[] { detalle }));
                RegistrarIntegridadCorrupta(fabricaSeguridad, detalle);
                pnlLoginNormal.Visible = false;
                pnlAccesoEmergencia.Visible = true;
            }
        }
        catch (Exception excepcion)
        {
            modoEmergencia = true;
            OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
            ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
            RegistrarIntegridadCorrupta(fabricaSeguridad, excepcion.Message);
            pnlLoginNormal.Visible = false;
            pnlAccesoEmergencia.Visible = true;
        }
    }

    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        if (modoEmergencia || !Page.IsValid)
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

    protected void btnIngresoEmergencia_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            bool credencialesValidas = EmergenciaHelper.ValidarCredenciales(
                txtUsuarioEmergencia.Text.Trim(), txtContrasenaEmergencia.Text);

            if (!credencialesValidas)
            {
                throw new OperativException(TipoError.ErrorCredencialesEmergenciaInvalidas);
            }

            FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
            IIntegridadService integridadService = fabricaSeguridad.CrearIntegridadService();
            integridadService.RepararBaseDatos();

            RegistrarReparacionEmergencia(fabricaSeguridad);

            Usuario usuarioEmergencia = new Usuario
            {
                IdUsuario = 0,
                NombreUsuario = txtUsuarioEmergencia.Text.Trim(),
                NombreCompleto = "Web Master (acceso de emergencia)",
                Activo = true,
                Bloqueado = false
            };

            Familia perfilEmergencia = new Familia
            {
                IdFamilia = 0,
                Nombre = "WebMaster",
                Descripcion = "Acceso de emergencia"
            };

            sesionHandler.IniciarSesion(usuarioEmergencia, perfilEmergencia, new FamiliaCompuesto());

            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilEmergencia.Nombre) + "?reparado=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
            ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
        }
    }

    private void RegistrarReparacionEmergencia(FabricaSeguridad fabricaSeguridad)
    {
        IBitacoraService bitacoraService = fabricaSeguridad.CrearBitacoraService();
        bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);
    }

    private void RegistrarIntegridadCorrupta(FabricaSeguridad fabricaSeguridad, string detalle)
    {
        try
        {
            IBitacoraService bitacoraService = fabricaSeguridad.CrearBitacoraService();
            bitacoraService.Registrar(null, TipoAccionBitacora.IntegridadCorrupta, detalle);
        }
        catch (Exception)
        {
        }
    }
}
