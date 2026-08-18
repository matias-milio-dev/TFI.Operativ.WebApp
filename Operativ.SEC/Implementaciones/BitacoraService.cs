using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones
{
    public class BitacoraService : IBitacoraService
    {
        private readonly IBitacoraRepositorio bitacoraRepositorio;

        public BitacoraService()
        {
            FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
            bitacoraRepositorio = fabricaRepositorio.CrearBitacoraRepositorio();
        }

        private const int LongitudMaximaDescripcion = 300;

        public void Registrar(int? idUsuario, TipoAccionBitacora accion)
        {
            Registrar(idUsuario, accion, null);
        }

        public void Registrar(int? idUsuario, TipoAccionBitacora accion, string detalleAdicional)
        {
            string descripcion = GetDescripcion(accion);

            if (!string.IsNullOrEmpty(detalleAdicional))
            {
                descripcion = descripcion + ": " + detalleAdicional;

                if (descripcion.Length > LongitudMaximaDescripcion)
                {
                    descripcion = descripcion.Substring(0, LongitudMaximaDescripcion);
                }
            }

            Bitacora entrada = new Bitacora
            {
                IdUsuario = idUsuario,
                Accion = accion,
                Criticidad = GetCriticidad(accion),
                Descripcion = descripcion
            };

            bitacoraRepositorio.Registrar(entrada);
        }

        private CriticidadBitacora GetCriticidad(TipoAccionBitacora accion)
        {
            switch (accion)
            {
                case TipoAccionBitacora.LoginExitoso:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.LoginBloqueado:
                    return CriticidadBitacora.Critico;
                case TipoAccionBitacora.RecuperacionContrasena:
                    return CriticidadBitacora.Advertencia;
                case TipoAccionBitacora.CierreSesion:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.IntentoLoginFallido:
                    return CriticidadBitacora.Critico;
                case TipoAccionBitacora.AltaUsuario:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.ModificacionUsuario:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.BajaUsuario:
                    return CriticidadBitacora.Advertencia;
                case TipoAccionBitacora.DesbloqueoUsuario:
                    return CriticidadBitacora.Advertencia;
                case TipoAccionBitacora.CambioClave:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.ReparacionEmergenciaBaseDatos:
                    return CriticidadBitacora.Critico;
                case TipoAccionBitacora.IntegridadCorrupta:
                    return CriticidadBitacora.Critico;
                default:
                    return CriticidadBitacora.Informativo;
            }
        }

        private string GetDescripcion(TipoAccionBitacora accion)
        {
            switch (accion)
            {
                case TipoAccionBitacora.LoginExitoso:
                    return "Inicio de sesión exitoso";
                case TipoAccionBitacora.LoginBloqueado:
                    return string.Format("Usuario bloqueado tras {0} intentos fallidos", ConfiguracionAplicacion.IntentosMaximosLogin);
                case TipoAccionBitacora.RecuperacionContrasena:
                    return "Contraseña restablecida por recuperación";
                case TipoAccionBitacora.CierreSesion:
                    return "Cierre de sesión";
                case TipoAccionBitacora.IntentoLoginFallido:
                    return "Login con credenciales invalidas";
                case TipoAccionBitacora.AltaUsuario:
                    return "Alta de usuario";
                case TipoAccionBitacora.ModificacionUsuario:
                    return "Modificación de datos de usuario";
                case TipoAccionBitacora.BajaUsuario:
                    return "Baja lógica de usuario";
                case TipoAccionBitacora.DesbloqueoUsuario:
                    return "Desbloqueo de usuario";
                case TipoAccionBitacora.CambioClave:
                    return "Cambio de contraseña por autogestión";
                case TipoAccionBitacora.ReparacionEmergenciaBaseDatos:
                    return "Base de datos reparada mediante acceso de emergencia del Web Master";
                case TipoAccionBitacora.IntegridadCorrupta:
                    return "Se detectó una alteración en la integridad de los datos del sistema";
                default:
                    return string.Empty;
            }
        }
    }
}
