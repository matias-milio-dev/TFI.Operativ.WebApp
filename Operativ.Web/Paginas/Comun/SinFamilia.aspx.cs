using System;
using Operativ.SEC.Handlers;

namespace Operativ.Web.Paginas;
public partial class SinFamilia : PaginaBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
        SesionHandler sesionHandler = new SesionHandler();

        if (!sesionHandler.HaySesionActiva())
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx");
        }
    }
}
