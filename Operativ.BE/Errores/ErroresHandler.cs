using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Web;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;

namespace Operativ.BE.Errores;
public class ErroresHandler
{
    public string GetMensaje(TipoError tipoError)
    {
        return GetMensaje(tipoError, null);
    }

    public string GetMensaje(TipoError tipoError, string[] parametros)
    {
        DefinicionError definicion = DefinicionError.ObtenerPorTipo(tipoError);
        string texto = GetTexto(definicion.ClaveRecurso, parametros);
        return definicion.Codigo + " - " + texto;
    }

    public string GetMensaje(OperativException excepcion)
    {
        return GetMensaje(excepcion.TipoError, excepcion.Parametros);
    }

    public OperativException TraducirExcepcion(Exception excepcion)
    {
        if (excepcion is OperativException)
        {
            return (OperativException)excepcion;
        }

        if (excepcion is SqlException)
        {
            return new OperativException(TipoError.ErrorConexionBaseDatos);
        }

        if (excepcion is SmtpException)
        {
            return new OperativException(TipoError.ErrorEnvioEmail);
        }

        return new OperativException(TipoError.ErrorConexionBaseDatos);
    }

    private string GetTexto(string claveRecurso, string[] parametros)
    {
        string textoRecurso = HttpContext.GetGlobalResourceObject("Textos", claveRecurso) as string;

        if (parametros == null)
        {
            return textoRecurso;
        }

        return string.Format(textoRecurso, parametros);
    }
}
