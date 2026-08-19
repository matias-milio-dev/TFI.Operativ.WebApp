using System;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web;
public class Global : System.Web.HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        IIntegridadService integridadService = fabricaSeguridad.CrearIntegridadService();
        integridadService.InicializarDigitos();
    }

    protected void Application_Error(object sender, EventArgs e)
    {
        Exception excepcion = Server.GetLastError();

        if (excepcion != null)
        {
            Server.ClearError();
            Response.Redirect("~/Paginas/Comun/Error.aspx");
        }
    }

    protected void Session_Start(object sender, EventArgs e)
    {
    }

    protected void Session_End(object sender, EventArgs e)
    {
    }
}
