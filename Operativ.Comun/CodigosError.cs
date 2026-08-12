using System.Collections.Generic;

namespace Operativ.Comun
{
    public static class CodigosError
    {
        public const string ErrorUsuarioOClaveIncorrectos = "ERR01";
        public const string ErrorIntentoFallidoLogin = "ERR02";
        public const string ErrorUsuarioBloqueado = "ERR03";
        public const string ErrorUsuarioInactivo = "ERR04";
        public const string ErrorCampoObligatorioNoInformado = "ERR05";
        public const string ErrorFormatoDatoInvalido = "ERR06";
        public const string ErrorRegistroNoEncontrado = "ERR07";
        public const string ErrorConexionBaseDatos = "ERR08";
        public const string ErrorInesperadoSistema = "ERR09";
        public const string ErrorAccesoDenegado = "ERR10";
        public const string ErrorSesionExpirada = "ERR11";
        public const string ErrorUsuarioOCorreoYaRegistrado = "ERR12";
        public const string ErrorClaveNoCumpleRequisitosMinimos = "ERR13";
        public const string ErrorClavesNoCoinciden = "ERR14";
        public const string ErrorEnvioCorreoRecuperacionFallido = "ERR15";
        public const string ErrorFamiliaOPatenteInexistente = "ERR16";
        public const string ErrorActivoSinSuscripcionActiva = "ERR17";
        public const string ErrorCuitClienteDuplicado = "ERR18";
        public const string ErrorPaqueteInactivo = "ERR19";
        public const string ErrorRutaBackupInvalida = "ERR20";
        public const string ErrorRutaRestoreInvalida = "ERR21";
        public const string ErrorIntegridadCorrupta = "ERR22";
        public const string ErrorGeneracionXmlServicioFallida = "ERR23";
        public const string ErrorSuscripcionVencidaOCancelada = "ERR24";
        public const string ErrorMedioPagoNoSoportado = "ERR25";
        public const string ErrorOperacionNoPermitidaParaPerfil = "ERR26";

        private static readonly Dictionary<string, TipoCriticidadError> _clasificacion = new Dictionary<string, TipoCriticidadError>
        {
            { ErrorUsuarioOClaveIncorrectos, TipoCriticidadError.Advertencia },
            { ErrorIntentoFallidoLogin, TipoCriticidadError.Advertencia },
            { ErrorUsuarioBloqueado, TipoCriticidadError.Grave },
            { ErrorUsuarioInactivo, TipoCriticidadError.Grave },
            { ErrorCampoObligatorioNoInformado, TipoCriticidadError.Advertencia },
            { ErrorFormatoDatoInvalido, TipoCriticidadError.Advertencia },
            { ErrorRegistroNoEncontrado, TipoCriticidadError.Advertencia },
            { ErrorConexionBaseDatos, TipoCriticidadError.Critico },
            { ErrorInesperadoSistema, TipoCriticidadError.Critico },
            { ErrorAccesoDenegado, TipoCriticidadError.Grave },
            { ErrorSesionExpirada, TipoCriticidadError.Advertencia },
            { ErrorUsuarioOCorreoYaRegistrado, TipoCriticidadError.Advertencia },
            { ErrorClaveNoCumpleRequisitosMinimos, TipoCriticidadError.Advertencia },
            { ErrorClavesNoCoinciden, TipoCriticidadError.Advertencia },
            { ErrorEnvioCorreoRecuperacionFallido, TipoCriticidadError.Grave },
            { ErrorFamiliaOPatenteInexistente, TipoCriticidadError.Advertencia },
            { ErrorActivoSinSuscripcionActiva, TipoCriticidadError.Advertencia },
            { ErrorCuitClienteDuplicado, TipoCriticidadError.Advertencia },
            { ErrorPaqueteInactivo, TipoCriticidadError.Advertencia },
            { ErrorRutaBackupInvalida, TipoCriticidadError.Grave },
            { ErrorRutaRestoreInvalida, TipoCriticidadError.Grave },
            { ErrorIntegridadCorrupta, TipoCriticidadError.Critico },
            { ErrorGeneracionXmlServicioFallida, TipoCriticidadError.Grave },
            { ErrorSuscripcionVencidaOCancelada, TipoCriticidadError.Advertencia },
            { ErrorMedioPagoNoSoportado, TipoCriticidadError.Advertencia },
            { ErrorOperacionNoPermitidaParaPerfil, TipoCriticidadError.Grave },
        };

        public static TipoCriticidadError ObtenerCriticidad(string codigoError)
        {
            return _clasificacion.TryGetValue(codigoError, out var tipo) ? tipo : TipoCriticidadError.Grave;
        }
    }
}
