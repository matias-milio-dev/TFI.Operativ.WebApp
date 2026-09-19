using System;
using System.Collections.Generic;
using System.Web;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.Web.Idioma;

namespace Operativ.Web.Paginas;
public partial class ConsultaSuscripciones : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaSuscripciones;
    private readonly ISuscripcionService suscripcionService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ConsultarSuscripciones }; }
    }

    public ConsultaSuscripciones()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        suscripcionService = fabricaNegocio.CrearSuscripcionService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ucPaginador.TamanioPagina = tamanioPagina;

        CargarGrilla();
    }

    protected void ucPaginador_PaginaCambiada(object sender, EventArgs e)
    {
        CargarGrilla();
    }

    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        ucPaginador.Reiniciar();
        CargarGrilla();
    }

    protected string ObtenerClaseEstadoSuscripcion(EstadoSuscripcion estado)
    {
        return "badge-suscripcion-" + estado.ToString().ToLowerInvariant();
    }

    protected string ObtenerTextoEstadoSuscripcion(EstadoSuscripcion estado)
    {
        return TextoRecurso.Obtener("EtiquetaEstadoSuscripcion" + estado.ToString());
    }

    protected string ObtenerTextoFecha(DateTime? fecha)
    {
        if (!fecha.HasValue)
        {
            return "-";
        }

        return fecha.Value.ToString("dd/MM/yyyy");
    }

    protected string ObtenerTextoComprobante(string codigoComprobante)
    {
        if (string.IsNullOrEmpty(codigoComprobante))
        {
            return "-";
        }

        return HttpUtility.HtmlEncode(codigoComprobante);
    }

    private void CargarGrilla()
    {
        string filtro = txtFiltro.Text.Trim();
        int? idCliente = ObtenerIdClienteSesion();

        List<Suscripcion> suscripciones = suscripcionService.ListarSuscripciones(filtro, idCliente, ucPaginador.NumeroPagina, tamanioPagina);
        int total = suscripcionService.ContarSuscripciones(filtro, idCliente);

        gvSuscripciones.DataSource = suscripciones;
        gvSuscripciones.DataBind();

        ucPaginador.Actualizar(total, suscripciones.Count);
    }
}
