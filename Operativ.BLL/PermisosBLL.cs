using System.Collections.Generic;
using Operativ.BE;
using Operativ.BLL.Patrones;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface IPermisosBLL
    {
        List<Patente> ListarTodasLasPatentes();
        List<int> ListarIdsPatentesDeFamilia(int idFamilia);
        List<Patente> ListarPatentesNoAsignadas(int idFamilia);
        void AsignarPatenteAFamilia(int idFamilia, int idPatente);
        void RemoverPatenteDeFamilia(int idFamilia, int idPatente);
        void AsignarUsuarioAFamilia(int idUsuario, int idFamilia);
        void RemoverUsuarioDeFamilia(int idUsuario, int idFamilia);
        void AsignarPatenteDirectaAUsuario(int idUsuario, int idPatente);
        void RemoverPatenteDirectaDeUsuario(int idUsuario, int idPatente);
    }

    public class PermisosBLL : IPermisosBLL
    {
        private readonly IPatenteDAL _patenteDAL = FabricaDAL.Instancia.CrearPatenteDAL();
        private readonly IPermisosDAL _permisosDAL = FabricaDAL.Instancia.CrearPermisosDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public List<Patente> ListarTodasLasPatentes()
        {
            return _patenteDAL.Listar();
        }

        public List<int> ListarIdsPatentesDeFamilia(int idFamilia)
        {
            var tabla = _permisosDAL.ListarPatentesDeFamilia(idFamilia);
            var ids = new List<int>();
            foreach (System.Data.DataRow fila in tabla.Rows)
            {
                ids.Add((int)fila["IdPatente"]);
            }
            return ids;
        }

        public List<Patente> ListarPatentesNoAsignadas(int idFamilia)
        {
            return _patenteDAL.ListarNoAsignadasAFamilia(idFamilia);
        }

        public void AsignarPatenteAFamilia(int idFamilia, int idPatente)
        {
            _permisosDAL.AsignarFamiliaAPatente(idFamilia, idPatente);
            _bitacoraBLL.Registrar("ASIGNACION", "FamiliaPatente", $"{idFamilia}-{idPatente}", "Patente asignada a familia.", "ADVERTENCIA");
        }

        public void RemoverPatenteDeFamilia(int idFamilia, int idPatente)
        {
            _permisosDAL.RemoverFamiliaDePatente(idFamilia, idPatente);
            _bitacoraBLL.Registrar("REMOCION", "FamiliaPatente", $"{idFamilia}-{idPatente}", "Patente removida de familia.", "ADVERTENCIA");
        }

        public void AsignarUsuarioAFamilia(int idUsuario, int idFamilia)
        {
            _permisosDAL.AsignarUsuarioAFamilia(idUsuario, idFamilia);
            _bitacoraBLL.Registrar("ASIGNACION", "UsuarioFamilia", $"{idUsuario}-{idFamilia}", "Usuario asignado a familia.", "ADVERTENCIA");
        }

        public void RemoverUsuarioDeFamilia(int idUsuario, int idFamilia)
        {
            _permisosDAL.RemoverUsuarioDeFamilia(idUsuario, idFamilia);
            _bitacoraBLL.Registrar("REMOCION", "UsuarioFamilia", $"{idUsuario}-{idFamilia}", "Usuario removido de familia.", "ADVERTENCIA");
        }

        public void AsignarPatenteDirectaAUsuario(int idUsuario, int idPatente)
        {
            _permisosDAL.AsignarPatenteDirectaAUsuario(idUsuario, idPatente);
            _bitacoraBLL.Registrar("ASIGNACION", "UsuarioPatente", $"{idUsuario}-{idPatente}", "Patente asignada directamente a usuario.", "ADVERTENCIA");
        }

        public void RemoverPatenteDirectaDeUsuario(int idUsuario, int idPatente)
        {
            _permisosDAL.RemoverPatenteDirectaDeUsuario(idUsuario, idPatente);
            _bitacoraBLL.Registrar("REMOCION", "UsuarioPatente", $"{idUsuario}-{idPatente}", "Patente directa removida de usuario.", "ADVERTENCIA");
        }
    }
}
