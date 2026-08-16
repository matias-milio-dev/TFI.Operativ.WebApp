using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BLL.Configuracion;
using Operativ.BLL.Contratos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;

namespace Operativ.BLL.Implementaciones
{
    public class BitacoraNegocio : IBitacoraNegocio
    {
        private readonly IBitacoraRepositorio bitacoraRepositorio;

        public BitacoraNegocio()
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
                default:
                    return string.Empty;
            }
        }
    }
}
