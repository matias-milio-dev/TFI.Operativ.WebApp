using System;

namespace Operativ.Web.Paginas
{
    public partial class HomeWebMaster : PaginaSeguraBase
    {
        protected override string PerfilRequerido
        {
            get { return NavegacionHelper.PerfilWebMaster; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlReparacionExitosa.Visible = Request.QueryString["reparado"] == "1";
            }
        }

        protected void btnAceptarReparacion_Click(object sender, EventArgs e)
        {
            SesionHandler.CerrarSesion();
            Response.Redirect("~/Login.aspx");
        }
    }
}
