using Operativ.BE.Enums;

namespace Operativ.BLL.Contratos
{
    public interface IBitacoraNegocio
    {
        void Registrar(int idUsuario, TipoAccionBitacora accion);
    }
}
