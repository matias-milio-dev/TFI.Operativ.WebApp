using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Modelos;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web.Controles;
public partial class Navbar : UserControl
{
    private const string ClaseEnlace = "navbar-link";
    private const string ClaseEnlaceActivo = "navbar-link-activo";

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
        lnkFamilias.Visible = autorizacionHandler.TienePatente(NombrePatente.GestionarFamilias);
        lnkBackup.Visible = autorizacionHandler.TieneAlgunaPatente(new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup });
        lnkBitacora.Visible = autorizacionHandler.TienePatente(NombrePatente.ConsultarBitacora);
        lnkPaquetes.Visible = autorizacionHandler.TienePatente(NombrePatente.GestionarCatalogo);
        lnkActivos.Visible = autorizacionHandler.TienePatente(NombrePatente.GestionarActivos);
        lnkClientes.Visible = autorizacionHandler.TienePatente(NombrePatente.GestionarClientes);
        lnkIncidentes.Visible = autorizacionHandler.TieneAlgunaPatente(CategoriaPatente.Incidentes.NombresPatente);
        lnkMiSuscripcion.Visible = autorizacionHandler.TienePatente(NombrePatente.GestionarSuscripciones);
        lnkSuscripciones.Visible = autorizacionHandler.TienePatente(NombrePatente.ConsultarSuscripciones);
        lnkFacturas.Visible = autorizacionHandler.TienePatente(NombrePatente.ConsultarFacturas);
        lnkMisActivos.Visible = autorizacionHandler.TienePatente(NombrePatente.MonitorearActivos);

        MarcarEnlaceActivo();
    }

    private void MarcarEnlaceActivo()
    {
        string paginaActual = Path.GetFileName(Request.AppRelativeCurrentExecutionFilePath);

        HyperLink[] enlaces =
        {
            lnkMisActivos, lnkUsuarios, lnkFamilias, lnkClientes, lnkPaquetes, lnkActivos, lnkIncidentes,
            lnkMiSuscripcion, lnkSuscripciones, lnkFacturas, lnkBackup, lnkBitacora
        };

        bool hayCoincidencia = false;

        foreach (HyperLink enlace in enlaces)
        {
            string archivoEnlace = Path.GetFileName(enlace.NavigateUrl);

            if (enlace.Visible && string.Equals(archivoEnlace, paginaActual, StringComparison.OrdinalIgnoreCase))
            {
                // Se reasigna la clase completa (no se concatena): CssClass viaja en ViewState y se acumularia en cada postback.
                enlace.CssClass = ClaseEnlace + " " + ClaseEnlaceActivo;
                hayCoincidencia = true;
            }
        }

        // Inicio se arma en runtime segun el perfil, por eso es el caso por defecto.
        lnkHome.CssClass = hayCoincidencia ? ClaseEnlace : ClaseEnlace + " " + ClaseEnlaceActivo;
    }
}
