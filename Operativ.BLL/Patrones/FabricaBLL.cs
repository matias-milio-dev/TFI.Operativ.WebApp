using System;

namespace Operativ.BLL.Patrones
{
    public sealed class FabricaBLL : IFabricaBLL
    {
        private static readonly Lazy<FabricaBLL> _instancia = new Lazy<FabricaBLL>(() => new FabricaBLL());

        public static IFabricaBLL Instancia => _instancia.Value;

        private FabricaBLL()
        {
        }

        public IActivoBLL CrearActivoBLL() => new ActivoBLL();
        public IBitacoraBLL CrearBitacoraBLL() => new BitacoraBLL();
        public ICatalogoBLL CrearCatalogoBLL() => new CatalogoBLL();
        public IClienteBLL CrearClienteBLL() => new ClienteBLL();
        public IFacturaBLL CrearFacturaBLL() => new FacturaBLL();
        public IFamiliaBLL CrearFamiliaBLL() => new FamiliaBLL();
        public IIncidenteBLL CrearIncidenteBLL() => new IncidenteBLL();
        public IMonitoreoBLL CrearMonitoreoBLL() => new MonitoreoBLL();
        public IPagoBLL CrearPagoBLL() => new PagoBLL();
        public IPaqueteBLL CrearPaqueteBLL() => new PaqueteBLL();
        public IPermisosBLL CrearPermisosBLL() => new PermisosBLL();
        public ISistemaBLL CrearSistemaBLL() => new SistemaBLL();
        public ISuscripcionBLL CrearSuscripcionBLL() => new SuscripcionBLL();
        public IUsuarioBLL CrearUsuarioBLL() => new UsuarioBLL();
    }
}
