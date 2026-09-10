using System;
using System.Collections.Generic;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class HomeWebMaster : PaginaSeguraBase
{
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
        try
        {
            integridadService.RepararBaseDatos();
            bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);

            SesionHandler.CerrarSesion();
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?recalculado=1");
        }
        catch (Exception excepcion)
        {
            ControlNotificaciones.MostrarMensaje(excepcion);
        }
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
}
