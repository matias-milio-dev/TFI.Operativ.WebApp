using System;
using System.Web.UI;
using Operativ.Comun;
using Operativ.SEC;

namespace Operativ.Web
{
    public abstract class PaginaBase : Page
    {
        protected virtual string PatenteRequerida => null;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!ContextoSesion.Actual.EstaAutenticado)
            {
                Response.Redirect("~/Login.aspx", endResponse: true);
                return;
            }

            if (PatenteRequerida != null
                && !GestorAutorizacion.TienePatente(PatenteRequerida))
            {
                Session["Operativ_UltimoMensajeError"] = ManejadorErrores.Resolver(CodigosError.ErrorAccesoDenegado);
                Response.Redirect("~/Paginas/ErrorGenerico.aspx", endResponse: true);
            }
        }
    }
}
