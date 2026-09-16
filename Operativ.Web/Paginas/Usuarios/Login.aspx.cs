using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class Login : PaginaBase
{
    private readonly FabricaSeguridad fabricaSeguridad;
    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;
    private readonly SesionHandler sesionHandler;

    public Login()
    {
        fabricaSeguridad = new FabricaSeguridad();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            return;
        }

        if (sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo?.Nombre));
        }

        if (Request.QueryString["err"] == "sesion")
        {
            ucNotificaciones.MostrarMensaje(TipoError.ErrorSesionExpirada);
        }

        if (Request.QueryString["restaurado"] == "1")
        {
            ucNotificaciones.MostrarExito("MensajeExitoRestaurarBackup");
        }

        if (Request.QueryString["recalculado"] == "1")
        {
            ucNotificaciones.MostrarExito("MensajeExitoRecalculoDigitos");
        }
    }

    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        string nombreUsuario = txtNombreUsuario.Text.Trim();
        string contrasena = txtContrasena.Text;

        ResultadoAutenticacion resultado = null;
        Exception excepcionLoginNormal = null;

        try
        {
            ILoginStrategy estrategiaNormal = fabricaSeguridad.CrearLoginStrategy();
            resultado = estrategiaNormal.Autenticar(nombreUsuario, contrasena);
        }
        catch (Exception excepcion)
        {
            excepcionLoginNormal = excepcion;
        }

        try
        {
            List<ResultadoVerificacionTabla> fallas = integridadService.VerificarIntegridad();

            if (fallas.Count > 0)
            {
                int? idUsuario = resultado?.Usuario.IdUsuario;
                string detalle = integridadService.FormatearResumenFallas(fallas);

                bitacoraService.Registrar(idUsuario, TipoAccionBitacora.IntegridadCorrupta, detalle);

                if (excepcionLoginNormal == null)
                {
                    ucNotificaciones.MostrarMensaje(TipoError.ErrorIntegridadCorrupta);
                    return;
                }

                ILoginStrategy estrategiaEmergencia = fabricaSeguridad.CrearLoginStrategy(modoEmergencia: true);
                resultado = estrategiaEmergencia.Autenticar(nombreUsuario, contrasena);

                sesionHandler.GuardarFallasIntegridad(fallas);
            }
            else if (excepcionLoginNormal != null)
            {
                ucNotificaciones.MostrarMensaje(excepcionLoginNormal);
                return;
            }

            sesionHandler.IniciarSesion(resultado.Usuario, resultado.Perfil, resultado.ArbolPermisos);
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(resultado.Perfil?.Nombre), false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcionLoginNormal ?? excepcion);
        }
    }
}
