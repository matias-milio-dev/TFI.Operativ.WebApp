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

        public void Registrar(int idUsuario, TipoAccionBitacora accion)
        {
            Bitacora entrada = new Bitacora
            {
                IdUsuario = idUsuario,
                Accion = accion,
                Criticidad = GetCriticidad(accion),
                Descripcion = GetDescripcion(accion)
            };

            bitacoraRepositorio.Registrar(entrada);
        }

        public void Registrar(int idUsuarioEjecutor, TipoAccionBitacora accion, string entidadAfectada, int? idEntidadAfectada, string detalle)
        {
            Bitacora entrada = new Bitacora
            {
                IdUsuario = idUsuarioEjecutor,
                Accion = accion,
                Criticidad = GetCriticidad(accion),
                Descripcion = detalle,
                EntidadAfectada = entidadAfectada,
                IdEntidadAfectada = idEntidadAfectada
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
                case TipoAccionBitacora.Alta:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.Modificacion:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.Baja:
                    return CriticidadBitacora.Advertencia;
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
                default:
                    return string.Empty;
            }
        }
    }
}
