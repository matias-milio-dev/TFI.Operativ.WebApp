using System;
using System.Collections.Generic;
using Operativ.BE.Modelos.Composite;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.BE.Errores;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.SEC.Handlers;
using Operativ.SEC.Helpers;
using Operativ.Web.Paginas;

namespace Operativ.Web;
public partial class Login : PaginaBase
{
    private readonly IUsuarioService usuarioService;
    private readonly IFamiliaService familiaService;
    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;
    private readonly SesionHandler sesionHandler;
    private bool modoEmergencia;

    public Login()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        usuarioService = fabricaSeguridad.CrearUsuarioService();
        familiaService = fabricaSeguridad.CrearFamiliaService();
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

        try
        {
            Usuario usuario = usuarioService.ValidarCredenciales(
                txtNombreUsuario.Text.Trim(),
                txtContrasena.Text);

            Familia perfil = familiaService.GetPerfilDeUsuario(usuario.IdUsuario);
            FamiliaCompuesto arbolPermisos = familiaService.ArmarArbolPermisos(usuario.IdUsuario);

            sesionHandler.IniciarSesion(usuario, perfil, arbolPermisos);

            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfil.Nombre), false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            ucNotificaciones.MostrarMensaje(excepcion);
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
            bool credencialesValidas = LoginEmergenciaHelper.ValidarCredenciales(
                txtUsuarioEmergencia.Text.Trim(), txtContrasenaEmergencia.Text);

            if (!credencialesValidas)
            {
                throw new OperativException(TipoError.ErrorCredencialesEmergenciaInvalidas);
            }

            integridadService.RepararBaseDatos();

            bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);

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
