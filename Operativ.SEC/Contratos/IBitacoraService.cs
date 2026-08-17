using Operativ.BE.Enums;

namespace Operativ.SEC.Contratos
{
    public interface IBitacoraService
    {
        void Registrar(int idUsuario, TipoAccionBitacora accion);

        void Registrar(int idUsuarioEjecutor, TipoAccionBitacora accion, string entidadAfectada, int? idEntidadAfectada, string detalle);
    }
}
