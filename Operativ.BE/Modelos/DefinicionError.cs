using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Modelos;

public class DefinicionError
{
    public TipoError Tipo { get; }

    public string Codigo { get; }

    public string ClaveRecurso { get; }

    public DefinicionError(TipoError tipo, string codigo, string claveRecurso)
    {
        Tipo = tipo;
        Codigo = codigo;
        ClaveRecurso = claveRecurso;
    }

    public static readonly DefinicionError ErrorUsuarioNoExiste =
        new(TipoError.ErrorUsuarioNoExiste, "ERR01", "MensajeErrorUsuarioNoExiste");
    public static readonly DefinicionError ErrorContrasenaIncorrecta =
        new(TipoError.ErrorContrasenaIncorrecta, "ERR02", "MensajeErrorContrasenaIncorrecta");
    public static readonly DefinicionError ErrorUsuarioBloqueado =
        new(TipoError.ErrorUsuarioBloqueado, "ERR03", "MensajeErrorUsuarioBloqueado");
    public static readonly DefinicionError ErrorIntegridadCorrupta =
        new(TipoError.ErrorIntegridadCorrupta, "ERR04", "MensajeErrorIntegridadCorrupta");
    public static readonly DefinicionError ErrorConexionBaseDatos =
        new(TipoError.ErrorConexionBaseDatos, "ERR05", "MensajeErrorConexionBaseDatos");
    public static readonly DefinicionError ErrorEnvioEmail =
        new(TipoError.ErrorEnvioEmail, "ERR06", "MensajeErrorEnvioEmail");
    public static readonly DefinicionError ErrorCredencialesEmergenciaInvalidas =
        new(TipoError.ErrorCredencialesEmergenciaInvalidas, "ERR07", "MensajeErrorCredencialesEmergenciaInvalidas");
    public static readonly DefinicionError ErrorArchivoEmergenciaNoDisponible =
        new(TipoError.ErrorArchivoEmergenciaNoDisponible, "ERR08", "MensajeErrorArchivoEmergenciaNoDisponible");
    public static readonly DefinicionError ErrorSesionExpirada =
        new(TipoError.ErrorSesionExpirada, "ERR11", "MensajeErrorSesionExpirada");
    public static readonly DefinicionError ErrorUsuarioYaExiste =
        new(TipoError.ErrorUsuarioYaExiste, "ERR12", "MensajeErrorUsuarioYaExiste");
    public static readonly DefinicionError ErrorEmailYaRegistrado =
        new(TipoError.ErrorEmailYaRegistrado, "ERR13", "MensajeErrorEmailYaRegistrado");
    public static readonly DefinicionError ErrorContrasenaActualIncorrecta =
        new(TipoError.ErrorContrasenaActualIncorrecta, "ERR14", "MensajeErrorContrasenaActualIncorrecta");
    public static readonly DefinicionError ErrorClaveNoCumpleComplejidad =
        new(TipoError.ErrorClaveNoCumpleComplejidad, "ERR15", "MensajeErrorClaveNoCumpleComplejidad");
    private static readonly DefinicionError Desconocido =
        new(TipoError.ErrorUsuarioNoExiste, "ERR00", "MensajeErrorDesconocido");

    public static List<DefinicionError> ObtenerTodas()
    {
        return new List<DefinicionError>
        {
            ErrorUsuarioNoExiste,
            ErrorContrasenaIncorrecta,
            ErrorUsuarioBloqueado,
            ErrorIntegridadCorrupta,
            ErrorConexionBaseDatos,
            ErrorEnvioEmail,
            ErrorCredencialesEmergenciaInvalidas,
            ErrorArchivoEmergenciaNoDisponible,
            ErrorSesionExpirada,
            ErrorUsuarioYaExiste,
            ErrorEmailYaRegistrado,
            ErrorContrasenaActualIncorrecta,
            ErrorClaveNoCumpleComplejidad
        };
    }

    public static DefinicionError ObtenerPorTipo(TipoError tipo)
    {
        foreach (DefinicionError definicion in ObtenerTodas())
        {
            if (definicion.Tipo == tipo)
            {
                return definicion;
            }
        }

        return Desconocido;
    }
}
