using System;

namespace Operativ.DAL
{
    public sealed class FabricaDAL : IFabricaDAL
    {
        private static readonly Lazy<FabricaDAL> _instancia = new Lazy<FabricaDAL>(() => new FabricaDAL());

        public static IFabricaDAL Instancia => _instancia.Value;

        private FabricaDAL()
        {
        }

        public IActivoDAL CrearActivoDAL() => new ActivoDAL();
        public IBitacoraDAL CrearBitacoraDAL() => new BitacoraDAL();
        public IClienteDAL CrearClienteDAL() => new ClienteDAL();
        public IFacturaDAL CrearFacturaDAL() => new FacturaDAL();
        public IFamiliaDAL CrearFamiliaDAL() => new FamiliaDAL();
        public IIncidenteDAL CrearIncidenteDAL() => new IncidenteDAL();
        public IPagoDAL CrearPagoDAL() => new PagoDAL();
        public IPaqueteDAL CrearPaqueteDAL() => new PaqueteDAL();
        public IPatenteDAL CrearPatenteDAL() => new PatenteDAL();
        public IPermisosDAL CrearPermisosDAL() => new PermisosDAL();
        public ISistemaDAL CrearSistemaDAL() => new SistemaDAL();
        public ISuscripcionDAL CrearSuscripcionDAL() => new SuscripcionDAL();
        public IUsuarioDAL CrearUsuarioDAL() => new UsuarioDAL();
    }
}
