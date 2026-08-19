using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Web;
using Operativ.BE.Enums;

namespace Operativ.BE.Errores;
public class ErroresHandler
{
    public string GetMensaje(TipoError tipoError)
    {
        return GetMensaje(tipoError, null);
    }

    public string GetMensaje(TipoError tipoError, string[] parametros)
    {
        string codigo = GetCodigo(tipoError);
        string texto = GetTexto(tipoError, parametros);
        return codigo + " - " + texto;
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

    private string GetCodigo(TipoError tipoError)
    {
        switch (tipoError)
        {
            case TipoError.ErrorUsuarioNoExiste:
                return "ERR01";
            case TipoError.ErrorContrasenaIncorrecta:
                return "ERR02";
            case TipoError.ErrorUsuarioBloqueado:
                return "ERR03";
            case TipoError.ErrorConexionBaseDatos:
                return "ERR05";
            case TipoError.ErrorSesionExpirada:
                return "ERR11";
            case TipoError.ErrorEnvioEmail:
                return "ERR06";
            case TipoError.ErrorUsuarioYaExiste:
                return "ERR12";
            case TipoError.ErrorEmailYaRegistrado:
                return "ERR13";
            case TipoError.ErrorContrasenaActualIncorrecta:
                return "ERR14";
            case TipoError.ErrorClaveNoCumpleComplejidad:
                return "ERR15";
            case TipoError.ErrorIntegridadCorrupta:
                return "ERR04";
            case TipoError.ErrorCredencialesEmergenciaInvalidas:
                return "ERR07";
            case TipoError.ErrorArchivoEmergenciaNoDisponible:
                return "ERR08";
            default:
                return "ERR00";
        }
    }

    private string GetTexto(TipoError tipoError, string[] parametros)
    {
        string claveRecurso = GetClaveRecurso(tipoError);
        string textoRecurso = HttpContext.GetGlobalResourceObject("Textos", claveRecurso) as string;

        if (parametros == null)
        {
            return textoRecurso;
        }

        return string.Format(textoRecurso, parametros);
    }

    private string GetClaveRecurso(TipoError tipoError)
    {
        switch (tipoError)
        {
            case TipoError.ErrorUsuarioNoExiste:
                return "MensajeErrorUsuarioNoExiste";
            case TipoError.ErrorContrasenaIncorrecta:
                return "MensajeErrorContrasenaIncorrecta";
            case TipoError.ErrorUsuarioBloqueado:
                return "MensajeErrorUsuarioBloqueado";
            case TipoError.ErrorConexionBaseDatos:
                return "MensajeErrorConexionBaseDatos";
            case TipoError.ErrorSesionExpirada:
                return "MensajeErrorSesionExpirada";
            case TipoError.ErrorEnvioEmail:
                return "MensajeErrorEnvioEmail";
            case TipoError.ErrorUsuarioYaExiste:
                return "MensajeErrorUsuarioYaExiste";
            case TipoError.ErrorEmailYaRegistrado:
                return "MensajeErrorEmailYaRegistrado";
            case TipoError.ErrorContrasenaActualIncorrecta:
                return "MensajeErrorContrasenaActualIncorrecta";
            case TipoError.ErrorClaveNoCumpleComplejidad:
                return "MensajeErrorClaveNoCumpleComplejidad";
            case TipoError.ErrorIntegridadCorrupta:
                return "MensajeErrorIntegridadCorrupta";
            case TipoError.ErrorCredencialesEmergenciaInvalidas:
                return "MensajeErrorCredencialesEmergenciaInvalidas";
            case TipoError.ErrorArchivoEmergenciaNoDisponible:
                return "MensajeErrorArchivoEmergenciaNoDisponible";
            default:
                return "MensajeErrorDesconocido";
        }
    }
}
