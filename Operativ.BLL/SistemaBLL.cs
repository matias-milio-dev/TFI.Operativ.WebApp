using System.Data;
using Operativ.BLL.Patrones;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface ISistemaBLL
    {
        DataTable RepararBaseDatos();
        DataTable VerificarIntegridad();
        void RealizarBackup(string rutaDestino);
        void RealizarRestore(string rutaOrigen);
    }

    public class SistemaBLL : ISistemaBLL
    {
        private readonly ISistemaDAL _sistemaDAL = FabricaDAL.Instancia.CrearSistemaDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public DataTable RepararBaseDatos()
        {
            DataTable resultado = _sistemaDAL.RepararBaseDatos();
            _bitacoraBLL.Registrar("REPARACION", "BaseDatos", null, "Reparación de integridad DVH/DVV ejecutada.", "CRITICA");
            return resultado;
        }

        public DataTable VerificarIntegridad()
        {
            return _sistemaDAL.VerificarIntegridad();
        }

        public void RealizarBackup(string rutaDestino)
        {
            _sistemaDAL.RealizarBackup(rutaDestino);
            _bitacoraBLL.Registrar("BACKUP", "BaseDatos", null, $"Backup generado en '{rutaDestino}'.", "CRITICA");
        }

        public void RealizarRestore(string rutaOrigen)
        {
            _sistemaDAL.RealizarRestore(rutaOrigen);
            _bitacoraBLL.Registrar("RESTORE", "BaseDatos", null, $"Restore ejecutado desde '{rutaOrigen}'.", "CRITICA");
        }
    }
}
