using System;
using Operativ.BLL.Contratos;
using Operativ.BLL.Fabricas;

namespace Operativ.Web.Paginas;
public partial class Servicios : PaginaBase
{
    private const string AperturaSvg = "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\">";
    private const string CierreSvg = "</svg>";

    private readonly IServicioService servicioService;

    public Servicios()
    {
        FabricaNegocio fabricaNegocio = new FabricaNegocio();
        servicioService = fabricaNegocio.CrearServicioService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            rptServicios.DataSource = servicioService.ListarServicios();
            rptServicios.DataBind();
        }
    }

    protected string ObtenerIconoSvg(string claveIcono)
    {
        return AperturaSvg + ObtenerTrazoIcono(claveIcono) + CierreSvg;
    }

    private string ObtenerTrazoIcono(string claveIcono)
    {
        switch (claveIcono)
        {
            case "DeviceAsAService":
                return "<rect x=\"2\" y=\"3\" width=\"20\" height=\"14\" rx=\"2\" ry=\"2\"></rect>"
                    + "<line x1=\"8\" y1=\"21\" x2=\"16\" y2=\"21\"></line>"
                    + "<line x1=\"12\" y1=\"17\" x2=\"12\" y2=\"21\"></line>";
            case "GestionDeActivos":
                return "<path d=\"M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z\"></path>"
                    + "<path d=\"M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z\"></path>";
            case "SoporteTecnico":
                return "<path d=\"M3 18v-6a9 9 0 0 1 18 0v6\"></path>"
                    + "<path d=\"M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3z\"></path>"
                    + "<path d=\"M3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z\"></path>";
            case "RenovacionTecnologica":
                return "<polyline points=\"23 4 23 10 17 10\"></polyline>"
                    + "<polyline points=\"1 20 1 14 7 14\"></polyline>"
                    + "<path d=\"M3.51 9a9 9 0 0 1 14.85-3.36L23 10\"></path>"
                    + "<path d=\"M1 14l4.64 4.36A9 9 0 0 0 20.49 15\"></path>";
            case "Consultoria":
                return "<line x1=\"18\" y1=\"20\" x2=\"18\" y2=\"10\"></line>"
                    + "<line x1=\"12\" y1=\"20\" x2=\"12\" y2=\"4\"></line>"
                    + "<line x1=\"6\" y1=\"20\" x2=\"6\" y2=\"14\"></line>";
            case "GestionDeLicencias":
                return "<path d=\"M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z\"></path>"
                    + "<polyline points=\"14 2 14 8 20 8\"></polyline>"
                    + "<line x1=\"16\" y1=\"13\" x2=\"8\" y2=\"13\"></line>"
                    + "<line x1=\"16\" y1=\"17\" x2=\"8\" y2=\"17\"></line>";
            default:
                return "<circle cx=\"12\" cy=\"12\" r=\"10\"></circle>";
        }
    }
}
