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
    private bool modoEmergencia;

    public Login()
    {
        fabricaSeguridad = new FabricaSeguridad();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
        sesionHandler = new SesionHandler();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo.Nombre));
        }
        VerificarIntegridadSistema();
        if (!IsPostBack && Request.QueryString["err"] == "sesion")
        {
            ucNotificaciones.MostrarMensaje(TipoError.ErrorSesionExpirada);
        }
    }

    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        if (modoEmergencia || !Page.IsValid)
        {
            return;
        }

        ProcesarLogin(
            fabricaSeguridad.CrearLoginStrategy(),
            txtNombreUsuario.Text.Trim(),
            txtContrasena.Text);
    }

    protected void btnIngresoEmergencia_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        ProcesarLogin(
            fabricaSeguridad.CrearLoginStrategy(modoEmergencia: true),
            txtUsuarioEmergencia.Text.Trim(),
            txtContrasenaEmergencia.Text);
    }

    private void ProcesarLogin(ILoginStrategy estrategia, string nombreUsuario, string contrasena)
    {
        try
        {
            ResultadoAutenticacion resultado = estrategia.Autenticar(nombreUsuario, contrasena);
            sesionHandler.IniciarSesion(resultado.Usuario, resultado.Perfil, resultado.ArbolPermisos);
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(resultado.Perfil.Nombre) + resultado.SufijoRedireccion, false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }

    private void VerificarIntegridadSistema()
    {
        try
        {
            List<ResultadoVerificacionTabla> resultadosInvalidos = integridadService.VerificarIntegridad();
            modoEmergencia = resultadosInvalidos.Count > 0;
            if (modoEmergencia)
            {
                string detalle = integridadService.FormatearResumenFallas(resultadosInvalidos);
                ucNotificaciones.MostrarMensaje(TipoError.ErrorIntegridadCorrupta, new string[] { detalle });
                bitacoraService.Registrar(null, TipoAccionBitacora.IntegridadCorrupta, detalle);
                pnlLoginNormal.Visible = false;
                pnlAccesoEmergencia.Visible = true;
            }
        }
        catch (Exception excepcion)
        {
            bitacoraService.Registrar(null, TipoAccionBitacora.IntegridadCorrupta, excepcion.Message);
            modoEmergencia = true;
            pnlLoginNormal.Visible = false;
            pnlAccesoEmergencia.Visible = true;
            ucNotificaciones.MostrarMensaje(excepcion);
        }
    }
}
