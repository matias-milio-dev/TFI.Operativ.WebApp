using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using Operativ.BE.Enums;
using Operativ.BE.Errores;

namespace Operativ.Web.Controles;
public partial class Notificaciones : UserControl
{
    private static readonly Regex PrefijoCodigoError = new Regex(@"^ERR\d+\s*-\s*");

    private readonly ErroresHandler erroresHandler = new ErroresHandler();

    public void MostrarMensaje(Exception excepcion)
    {
        OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
        MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
    }

    public void MostrarMensaje(TipoError tipoError)
    {
        MostrarMensaje(erroresHandler.GetMensaje(tipoError));
    }

    public void MostrarMensaje(TipoError tipoError, string[] parametros)
    {
        MostrarMensaje(erroresHandler.GetMensaje(tipoError, parametros));
    }

    public void MostrarExito(string claveRecurso)
    {
        string mensaje = (string)GetGlobalResourceObject("Textos", claveRecurso);
        MostrarMensaje(mensaje, true);
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
    }
}
