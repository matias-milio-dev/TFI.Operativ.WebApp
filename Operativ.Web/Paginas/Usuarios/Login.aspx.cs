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

        try
        {
            IntentarLoginNormal(nombreUsuario, contrasena);
        }
        catch (Exception excepcionLoginNormal)
        {
            IntentarLoginEmergencia(nombreUsuario, contrasena, excepcionLoginNormal);
        }
    }

    private void IntentarLoginNormal(string nombreUsuario, string contrasena)
    {
        ILoginStrategy estrategia = fabricaSeguridad.CrearLoginStrategy();
        ResultadoAutenticacion resultado = estrategia.Autenticar(nombreUsuario, contrasena);

        List<ResultadoVerificacionTabla> fallas = integridadService.VerificarIntegridad();

        if (fallas.Count > 0)
        {
            RegistrarFallasEnBitacora(resultado.Usuario.IdUsuario, fallas);
            ucNotificaciones.MostrarMensaje(TipoError.ErrorIntegridadCorrupta);
            return;
        }

        IniciarSesionYRedirigir(resultado);
    }

    private void IntentarLoginEmergencia(string nombreUsuario, string contrasena, Exception excepcionLoginNormal)
    {
        List<ResultadoVerificacionTabla> fallas;

        try
        {
            fallas = integridadService.VerificarIntegridad();
        }
        catch (Exception)
        {
            ucNotificaciones.MostrarMensaje(excepcionLoginNormal);
            return;
        }

        if (fallas.Count == 0)
        {
            ucNotificaciones.MostrarMensaje(excepcionLoginNormal);
            return;
        }

        ResultadoAutenticacion resultado;

        try
        {
            ILoginStrategy estrategia = fabricaSeguridad.CrearLoginStrategy(modoEmergencia: true);
            resultado = estrategia.Autenticar(nombreUsuario, contrasena);
        }
        catch (Exception)
        {
            ucNotificaciones.MostrarMensaje(excepcionLoginNormal);
            return;
        }

        RegistrarFallasEnBitacora(null, fallas);
        sesionHandler.GuardarFallasIntegridad(fallas);
        IniciarSesionYRedirigir(resultado);
    }

    private void IniciarSesionYRedirigir(ResultadoAutenticacion resultado)
    {
        sesionHandler.IniciarSesion(resultado.Usuario, resultado.Perfil, resultado.ArbolPermisos);
        Response.Redirect(NavegacionHelper.ObtenerUrlHome(resultado.Perfil?.Nombre), false);
        Context.ApplicationInstance.CompleteRequest();
    }

    private void RegistrarFallasEnBitacora(int? idUsuario, List<ResultadoVerificacionTabla> fallas)
    {
        string detalle = integridadService.FormatearResumenFallas(fallas);
        bitacoraService.Registrar(idUsuario, TipoAccionBitacora.IntegridadCorrupta, detalle);
    }
}
