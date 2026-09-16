using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class HomeWebMaster : PaginaSeguraBase
{
    private const string DetalleRecalculoExitoso = "Recálculo de dígitos verificadores completado";

    private const string DetalleRecalculoFallido = "Recálculo de dígitos verificadores fallido";

    private readonly IIntegridadService integridadService;
    private readonly IBitacoraService bitacoraService;

    protected override string[] PerfilesPermitidos
    {
        get { return new[] { NavegacionHelper.PerfilWebMaster }; }
    }

    public HomeWebMaster()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        integridadService = fabricaSeguridad.CrearIntegridadService();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarModalIntegridad();
        }
    }

    protected void btnRecalcular_Click(object sender, EventArgs e)
    {
        Usuario usuario = SesionHandler.GetUsuario();
        string resumenFallas = ObtenerResumenFallas();

        try
        {
            integridadService.RepararBaseDatos();
        }
        catch (Exception excepcion)
        {
            RegistrarRecalculoEnBitacora(DetalleRecalculoFallido, usuario, resumenFallas);
            ControlNotificaciones.MostrarMensaje(excepcion);
            return;
        }

        RegistrarRecalculoEnBitacora(DetalleRecalculoExitoso, usuario, resumenFallas);

        SesionHandler.CerrarSesion();
        Response.Redirect("~/Paginas/Usuarios/Login.aspx?recalculado=1", false);
        Context.ApplicationInstance.CompleteRequest();
    }

    protected string ObtenerDetalleFalla(ResultadoVerificacionTabla resultado)
    {
        if (resultado.ClavesFilasInvalidas.Count == 0)
        {
            return TextoRecurso.Obtener("MensajeCantidadRegistrosNoCoincide");
        }

        return TextoRecurso.Formato("MensajeFilasAfectadasIntegridad", string.Join(", ", resultado.ClavesFilasInvalidas));
    }

    protected string ObtenerDetalleDvv(ResultadoVerificacionTabla resultado)
    {
        return TextoRecurso.Formato("MensajeDetalleDvv", resultado.ValorDvvAlmacenado, resultado.ValorDvvCalculado);
    }

    private void CargarModalIntegridad()
    {
        List<ResultadoVerificacionTabla> fallas = SesionHandler.GetFallasIntegridad();

        if (fallas == null || fallas.Count == 0)
        {
            return;
        }

        pnlIntegridadCorrupta.Visible = true;
        rptFallasIntegridad.DataSource = fallas;
        rptFallasIntegridad.DataBind();
    }

    private string ObtenerResumenFallas()
    {
        List<ResultadoVerificacionTabla> fallas = SesionHandler.GetFallasIntegridad();

        if (fallas == null || fallas.Count == 0)
        {
            return null;
        }

        return integridadService.FormatearResumenFallas(fallas);
    }

    private void RegistrarRecalculoEnBitacora(string resultadoRecalculo, Usuario usuario, string resumenFallas)
    {
        string detalle = resultadoRecalculo;

        if (usuario != null)
        {
            detalle = detalle + " por " + usuario.NombreUsuario;
        }

        if (!string.IsNullOrEmpty(resumenFallas))
        {
            detalle = detalle + " sobre " + resumenFallas;
        }

        bitacoraService.Registrar(usuario?.IdUsuario, TipoAccionBitacora.ReparacionEmergenciaBaseDatos, detalle);
    }
}
