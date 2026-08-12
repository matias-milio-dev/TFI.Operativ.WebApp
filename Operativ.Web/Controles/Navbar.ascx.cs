using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.SEC;

namespace Operativ.Web.Controles
{
    public partial class Navbar : UserControl
    {
        protected HyperLink lnkInicio;
        protected HyperLink lnkUsuarios;
        protected HyperLink lnkFamilias;
        protected HyperLink lnkBitacora;
        protected HyperLink lnkClientes;
        protected HyperLink lnkPaquetes;
        protected HyperLink lnkSuscripciones;
        protected HyperLink lnkActivos;
        protected HyperLink lnkIncidentes;
        protected HyperLink lnkServicios;
        protected HyperLink lnkMonitoreo;
        protected HyperLink lnkAdministracion;
        protected LinkButton btnIdiomaEs;
        protected LinkButton btnIdiomaEn;

        protected void Page_Load(object sender, EventArgs e)
        {
            AsignarTextos();

            bool autenticado = ContextoSesion.Actual.EstaAutenticado;

            lnkUsuarios.Visible = autenticado && GestorAutorizacion.TienePatente("USUARIO_LISTAR");
            lnkFamilias.Visible = autenticado && GestorAutorizacion.TienePatente("PATENTE_ASIGNAR");
            lnkBitacora.Visible = autenticado && GestorAutorizacion.TienePatente("BITACORA_CONSULTAR");
            lnkClientes.Visible = autenticado && GestorAutorizacion.TienePatente("CLIENTE_ABM");
            lnkPaquetes.Visible = autenticado && GestorAutorizacion.TienePatente("PAQUETE_ABM");
            lnkSuscripciones.Visible = autenticado && GestorAutorizacion.TienePatente("SUSCRIPCION_ABM");
            lnkActivos.Visible = autenticado && GestorAutorizacion.TienePatente("ACTIVO_ABM");
            lnkIncidentes.Visible = autenticado && GestorAutorizacion.TienePatente("INCIDENTE_CONSULTAR");
            lnkServicios.Visible = autenticado;
            lnkMonitoreo.Visible = autenticado && GestorAutorizacion.TienePatente("MONITOREO_DASHBOARD");
            lnkAdministracion.Visible = autenticado && (GestorAutorizacion.TienePatente("BASEDATOS_REPARAR") || GestorAutorizacion.TienePatente("BASEDATOS_BACKUP"));
        }

        private void AsignarTextos()
        {
            lnkInicio.Text = (string)GetGlobalResourceObject("Textos", "MenuInicio");
            lnkUsuarios.Text = (string)GetGlobalResourceObject("Textos", "MenuUsuarios");
            lnkFamilias.Text = (string)GetGlobalResourceObject("Textos", "MenuFamilias");
            lnkBitacora.Text = (string)GetGlobalResourceObject("Textos", "MenuBitacora");
            lnkClientes.Text = (string)GetGlobalResourceObject("Textos", "MenuClientes");
            lnkPaquetes.Text = (string)GetGlobalResourceObject("Textos", "MenuPaquetes");
            lnkSuscripciones.Text = (string)GetGlobalResourceObject("Textos", "MenuSuscripciones");
            lnkActivos.Text = (string)GetGlobalResourceObject("Textos", "MenuActivos");
            lnkIncidentes.Text = (string)GetGlobalResourceObject("Textos", "MenuIncidentes");
            lnkServicios.Text = (string)GetGlobalResourceObject("Textos", "MenuServicios");
            lnkMonitoreo.Text = (string)GetGlobalResourceObject("Textos", "MenuMonitoreo");
            lnkAdministracion.Text = (string)GetGlobalResourceObject("Textos", "MenuAdministracion");
        }

        protected void btnIdiomaEs_Click(object sender, EventArgs e)
        {
            CambiarIdioma("es");
        }

        protected void btnIdiomaEn_Click(object sender, EventArgs e)
        {
            CambiarIdioma("en");
        }

        private void CambiarIdioma(string codigoIdioma)
        {
            var cookie = new HttpCookie(Global.CookieIdioma, codigoIdioma) { Expires = DateTime.Now.AddYears(1) };
            Response.Cookies.Add(cookie);
            Response.Redirect(Request.RawUrl, endResponse: true);
        }
    }
}
