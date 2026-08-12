namespace Operativ.BLL.Patrones
{
    public interface IFabricaBLL
    {
        IActivoBLL CrearActivoBLL();
        IBitacoraBLL CrearBitacoraBLL();
        ICatalogoBLL CrearCatalogoBLL();
        IClienteBLL CrearClienteBLL();
        IFacturaBLL CrearFacturaBLL();
        IFamiliaBLL CrearFamiliaBLL();
        IIncidenteBLL CrearIncidenteBLL();
        IMonitoreoBLL CrearMonitoreoBLL();
        IPagoBLL CrearPagoBLL();
        IPaqueteBLL CrearPaqueteBLL();
        IPermisosBLL CrearPermisosBLL();
        ISistemaBLL CrearSistemaBLL();
        ISuscripcionBLL CrearSuscripcionBLL();
        IUsuarioBLL CrearUsuarioBLL();
    }
}
