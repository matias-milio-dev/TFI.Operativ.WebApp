using System;
using System.Web.UI;
using Operativ.BE.Modelos;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web.Controles;
public partial class Navbar : UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        AutorizacionHandler autorizacionHandler = new AutorizacionHandler();
        string nombrePerfil = autorizacionHandler.GetNombrePerfil();

        lnkHome.Visible = !string.IsNullOrEmpty(nombrePerfil);

        if (lnkHome.Visible)
        {
            lnkHome.NavigateUrl = ResolveUrl(NavegacionHelper.ObtenerUrlHome(nombrePerfil));
        }

        lnkUsuarios.Visible = autorizacionHandler.TieneAlgunaPatente(CategoriaPatente.Usuarios.NombresPatente);
        lnkBackup.Visible = autorizacionHandler.TieneAlgunaPatente(new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup });
        lnkBitacora.Visible = autorizacionHandler.TienePatente(NombrePatente.ConsultarBitacora);
        lnkPaquetes.Visible = autorizacionHandler.TienePatente(NombrePatente.GestionarCatalogo);
    }
}
