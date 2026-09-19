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
    public static readonly DefinicionError FalloNoManejadoGenerico =
        new(TipoError.FalloNoManejadoGenerico, "ERR16", "MensajeErrorFalloNoManejadoGenerico");
    public static readonly DefinicionError ErrorUltimoUsuarioDeFamilia =
        new(TipoError.ErrorUltimoUsuarioDeFamilia, "ERR17", "MensajeErrorUltimoUsuarioDeFamilia");
    public static readonly DefinicionError ErrorPatenteYaAsignada =
        new(TipoError.ErrorPatenteYaAsignada, "ERR18", "MensajeErrorPatenteYaAsignada");
    public static readonly DefinicionError ErrorPatenteNoAsignada =
        new(TipoError.ErrorPatenteNoAsignada, "ERR19", "MensajeErrorPatenteNoAsignada");
    public static readonly DefinicionError ErrorSinPermiso =
        new(TipoError.ErrorSinPermiso, "ERR20", "MensajeErrorSinPermiso");
    public static readonly DefinicionError ErrorOperacionBackupFallida =
        new(TipoError.ErrorOperacionBackupFallida, "ERR21", "MensajeErrorOperacionBackupFallida");
    public static readonly DefinicionError ErrorArchivoBackupNoExiste =
        new(TipoError.ErrorArchivoBackupNoExiste, "ERR22", "MensajeErrorArchivoBackupNoExiste");
    public static readonly DefinicionError ErrorPaqueteNoExiste =
        new(TipoError.ErrorPaqueteNoExiste, "ERR23", "MensajeErrorPaqueteNoExiste");
    public static readonly DefinicionError ErrorPaqueteYaExiste =
        new(TipoError.ErrorPaqueteYaExiste, "ERR24", "MensajeErrorPaqueteYaExiste");
    public static readonly DefinicionError ErrorPaqueteSinProgramas =
        new(TipoError.ErrorPaqueteSinProgramas, "ERR25", "MensajeErrorPaqueteSinProgramas");
    public static readonly DefinicionError ErrorActivoNoExiste =
        new(TipoError.ErrorActivoNoExiste, "ERR26", "MensajeErrorActivoNoExiste");
    public static readonly DefinicionError ErrorActivoYaExiste =
        new(TipoError.ErrorActivoYaExiste, "ERR27", "MensajeErrorActivoYaExiste");
    public static readonly DefinicionError ErrorClienteNoExiste =
        new(TipoError.ErrorClienteNoExiste, "ERR28", "MensajeErrorClienteNoExiste");
    public static readonly DefinicionError ErrorClienteYaExiste =
        new(TipoError.ErrorClienteYaExiste, "ERR29", "MensajeErrorClienteYaExiste");
    public static readonly DefinicionError ErrorUsuarioClienteSinEmpresa =
        new(TipoError.ErrorUsuarioClienteSinEmpresa, "ERR30", "MensajeErrorUsuarioClienteSinEmpresa");
    public static readonly DefinicionError ErrorUsuarioNoDisponibleParaEmpresa =
        new(TipoError.ErrorUsuarioNoDisponibleParaEmpresa, "ERR31", "MensajeErrorUsuarioNoDisponibleParaEmpresa");
    public static readonly DefinicionError ErrorIncidenteNoExiste =
        new(TipoError.ErrorIncidenteNoExiste, "ERR32", "MensajeErrorIncidenteNoExiste");
    public static readonly DefinicionError ErrorIncidenteYaCerrado =
        new(TipoError.ErrorIncidenteYaCerrado, "ERR33", "MensajeErrorIncidenteYaCerrado");
    public static readonly DefinicionError ErrorSinActivosParaIncidente =
        new(TipoError.ErrorSinActivosParaIncidente, "ERR34", "MensajeErrorSinActivosParaIncidente");
    public static readonly DefinicionError ErrorSuscripcionNoExiste =
        new(TipoError.ErrorSuscripcionNoExiste, "ERR35", "MensajeErrorSuscripcionNoExiste");
    public static readonly DefinicionError ErrorSuscripcionVigenteExistente =
        new(TipoError.ErrorSuscripcionVigenteExistente, "ERR36", "MensajeErrorSuscripcionVigenteExistente");
    public static readonly DefinicionError ErrorSinSuscripcionActiva =
        new(TipoError.ErrorSinSuscripcionActiva, "ERR37", "MensajeErrorSinSuscripcionActiva");
    public static readonly DefinicionError ErrorGeneracionResumenSuscripcion =
        new(TipoError.ErrorGeneracionResumenSuscripcion, "ERR38", "MensajeErrorGeneracionResumenSuscripcion");
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
            ErrorClaveNoCumpleComplejidad,
            ErrorUltimoUsuarioDeFamilia,
            ErrorPatenteYaAsignada,
            ErrorPatenteNoAsignada,
            ErrorSinPermiso,
            ErrorOperacionBackupFallida,
            ErrorArchivoBackupNoExiste,
            ErrorPaqueteNoExiste,
            ErrorPaqueteYaExiste,
            ErrorPaqueteSinProgramas,
            ErrorActivoNoExiste,
            ErrorActivoYaExiste,
            ErrorClienteNoExiste,
            ErrorClienteYaExiste,
            ErrorUsuarioClienteSinEmpresa,
            ErrorUsuarioNoDisponibleParaEmpresa,
            ErrorIncidenteNoExiste,
            ErrorIncidenteYaCerrado,
            ErrorSinActivosParaIncidente,
            ErrorSuscripcionNoExiste,
            ErrorSuscripcionVigenteExistente,
            ErrorSinSuscripcionActiva,
            ErrorGeneracionResumenSuscripcion,
            FalloNoManejadoGenerico
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
