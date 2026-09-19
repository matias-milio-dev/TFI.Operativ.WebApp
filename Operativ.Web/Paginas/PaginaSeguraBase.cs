using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.SEC.Handlers;
using Operativ.Web.Controles;
using Operativ.Web.Master;

namespace Operativ.Web.Paginas;
public abstract class PaginaSeguraBase : PaginaBase
{
    private const string AtributoPatente = "data-patente";

    protected SesionHandler SesionHandler { get; private set; }

    protected AutorizacionHandler AutorizacionHandler { get; private set; }

    protected Notificaciones ControlNotificaciones
    {
        get { return ((Principal)Master).ControlNotificaciones; }
    }

    protected virtual string[] PerfilesPermitidos
    {
        get { return new string[0]; }
    }

    protected virtual string[] PatentesPermitidas
    {
        get { return new string[0]; }
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        SesionHandler = new SesionHandler();
        AutorizacionHandler = new AutorizacionHandler();

        ValidarAcceso();
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        AbrirCambioClaveSiEsProvisoria();
        OcultarControlesSinPatente(this);
    }

    protected override void RaisePostBackEvent(IPostBackEventHandler sourceControl, string eventArgument)
    {
        string patentesDeclaradas = ObtenerPatentesDeclaradas(sourceControl as Control);

        if (patentesDeclaradas != null && !ValidarPatentesDeclaradas(patentesDeclaradas))
        {
            return;
        }

        base.RaisePostBackEvent(sourceControl, eventArgument);
    }

    protected bool ValidarPatente(string nombrePatente)
    {
        if (AutorizacionHandler.TienePatente(nombrePatente))
        {
            return true;
        }

        ControlNotificaciones.MostrarMensaje(TipoError.ErrorSinPermiso, new string[] { nombrePatente });
        return false;
    }

    protected int? ObtenerIdClienteSesion()
    {
        Usuario usuario = SesionHandler.GetUsuario();

        if (usuario == null)
        {
            return null;
        }

        return usuario.IdCliente;
    }

    private void OcultarControlesSinPatente(Control contenedor)
    {
        foreach (Control hijo in contenedor.Controls)
        {
            string patentesDeclaradas = ObtenerPatentesDeclaradas(hijo);

            if (patentesDeclaradas != null && hijo.Visible && !AutorizacionHandler.TieneAlgunaPatente(SepararPatentes(patentesDeclaradas)))
            {
                hijo.Visible = false;
            }

            if (hijo.HasControls())
            {
                OcultarControlesSinPatente(hijo);
            }
        }
    }

    private string ObtenerPatentesDeclaradas(Control control)
    {
        WebControl controlWeb = control as WebControl;

        if (controlWeb == null)
        {
            return null;
        }

        string patentesDeclaradas = controlWeb.Attributes[AtributoPatente];

        if (string.IsNullOrEmpty(patentesDeclaradas))
        {
            return null;
        }

        return patentesDeclaradas;
    }

    private string[] SepararPatentes(string patentesDeclaradas)
    {
        return patentesDeclaradas.Split(',');
    }

    private bool ValidarPatentesDeclaradas(string patentesDeclaradas)
    {
        string[] patentes = SepararPatentes(patentesDeclaradas);

        if (AutorizacionHandler.TieneAlgunaPatente(patentes))
        {
            return true;
        }

        ControlNotificaciones.MostrarMensaje(TipoError.ErrorSinPermiso, new string[] { patentesDeclaradas });
        return false;
    }

    private void ValidarAcceso()
    {
        if (!SesionHandler.HaySesionActiva())
        {
            Response.Redirect("~/Paginas/Usuarios/Login.aspx?err=sesion");
        }

        if (PatentesPermitidas.Length > 0)
        {
            if (!AutorizacionHandler.TieneAlgunaPatente(PatentesPermitidas))
            {
                Response.Redirect("~/Paginas/Comun/NoAutorizado.aspx");
            }

            return;
        }

        if (SesionHandler.GetPerfil() == null)
        {
            Response.Redirect("~/Paginas/Comun/SinFamilia.aspx");
        }

        if (!AutorizacionHandler.EsAlgunPerfil(PerfilesPermitidos))
        {
            Response.Redirect("~/Paginas/Comun/NoAutorizado.aspx");
        }
    }

    private void AbrirCambioClaveSiEsProvisoria()
    {
        Usuario usuario = SesionHandler.GetUsuario();

        if (usuario != null && usuario.ContrasenaProvisoria)
        {
            ClientScript.RegisterStartupScript(GetType(), "AbrirModalCambiarClave", "Operativ.abrirModalCambiarClave();", true);
        }
    }
}