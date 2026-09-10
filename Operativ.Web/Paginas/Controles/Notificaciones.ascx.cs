using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.Web.Idioma;

namespace Operativ.Web.Controles;
public partial class Notificaciones : UserControl
{
    private static readonly Regex PrefijoCodigoError = new Regex(@"^ERR\d+\s*-\s*");

    private readonly ErroresHandler erroresHandler = new ErroresHandler();

    public void MostrarMensaje(Exception excepcion)
    {
        OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
        MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
        MostrarEnlaceDesbloqueoSiCorresponde(excepcionOperativ);
    }

    public void MostrarMensaje(TipoError tipoError)
    {
        MostrarMensaje(erroresHandler.GetMensaje(tipoError));
    }

    public void MostrarMensaje(TipoError tipoError, string[] parametros)
    {
        MostrarMensaje(erroresHandler.GetMensaje(tipoError, parametros));
    }

    public void MostrarMensaje(string mensaje)
    {
        MostrarMensaje(mensaje, false);
    }

    public void MostrarMensaje(string mensaje, bool esExito)
    {
        pnlNotificacion.Visible = true;
        pnlNotificacion.CssClass = esExito ? "notificacion notificacion-exito" : "notificacion notificacion-error";
        lblMensaje.Text = PrefijoCodigoError.Replace(mensaje, string.Empty);
        lnkDesbloquearUsuario.Visible = false;
    }

    public void MostrarExito(string claveRecurso)
    {
        MostrarMensaje(TextoRecurso.Obtener(claveRecurso), true);
    }

    public void MostrarExito(string claveRecurso, params object[] valores)
    {
        MostrarMensaje(TextoRecurso.Formato(claveRecurso, valores), true);
    }

    private void MostrarEnlaceDesbloqueoSiCorresponde(OperativException excepcionOperativ)
    {
        if (excepcionOperativ.TipoError != TipoError.ErrorUsuarioBloqueado)
        {
            return;
        }

        string nombreUsuario = excepcionOperativ.Parametros[0];
        lnkDesbloquearUsuario.NavigateUrl = "~/Paginas/Usuarios/RecuperarContrasena.aspx?usuario=" + HttpUtility.UrlEncode(nombreUsuario);
        lnkDesbloquearUsuario.Visible = true;
    }
}
