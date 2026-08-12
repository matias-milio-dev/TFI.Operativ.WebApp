using System.Collections.Generic;
using Operativ.BE;
using Operativ.BLL.Patrones;
using Operativ.Comun;
using Operativ.DAL;

namespace Operativ.BLL
{
    public interface IFamiliaBLL
    {
        List<Familia> Listar();
        Familia Obtener(int idFamilia);
        int Alta(string nombre, string descripcion);
        void Modificar(int idFamilia, string nombre, string descripcion);
        void Baja(int idFamilia);
    }

    public class FamiliaBLL : IFamiliaBLL
    {
        private readonly IFamiliaDAL _familiaDAL = FabricaDAL.Instancia.CrearFamiliaDAL();
        private readonly IPatenteDAL _patenteDAL = FabricaDAL.Instancia.CrearPatenteDAL();
        private readonly IBitacoraBLL _bitacoraBLL = FabricaBLL.Instancia.CrearBitacoraBLL();

        public List<Familia> Listar()
        {
            return _familiaDAL.Listar();
        }

        public Familia Obtener(int idFamilia)
        {
            var familia = _familiaDAL.ObtenerPorId(idFamilia);
            if (familia == null) throw new ExcepcionNegocio(CodigosError.ErrorFamiliaOPatenteInexistente);
            familia.Patentes = _patenteDAL.Listar().FindAll(p => true);
            return familia;
        }

        public int Alta(string nombre, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);

            int idFamiliaNueva = _familiaDAL.Insertar(new Familia { Nombre = nombre, Descripcion = descripcion });
            _bitacoraBLL.Registrar("ALTA", "Familia", idFamiliaNueva.ToString(), $"Alta de familia '{nombre}'.", "ADVERTENCIA");
            return idFamiliaNueva;
        }

        public void Modificar(int idFamilia, string nombre, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ExcepcionNegocio(CodigosError.ErrorCampoObligatorioNoInformado);

            _familiaDAL.Modificar(new Familia { IdFamilia = idFamilia, Nombre = nombre, Descripcion = descripcion });
            _bitacoraBLL.Registrar("MODIFICACION", "Familia", idFamilia.ToString(), "Modificación de familia.", "ADVERTENCIA");
        }

        public void Baja(int idFamilia)
        {
            _familiaDAL.Baja(idFamilia);
            _bitacoraBLL.Registrar("BAJA", "Familia", idFamilia.ToString(), "Baja lógica de familia.", "GRAVE");
        }
    }
}
