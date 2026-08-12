namespace Operativ.DAL
{
    public interface IFabricaDAL
    {
        IActivoDAL CrearActivoDAL();
        IBitacoraDAL CrearBitacoraDAL();
        IClienteDAL CrearClienteDAL();
        IFacturaDAL CrearFacturaDAL();
        IFamiliaDAL CrearFamiliaDAL();
        IIncidenteDAL CrearIncidenteDAL();
        IPagoDAL CrearPagoDAL();
        IPaqueteDAL CrearPaqueteDAL();
        IPatenteDAL CrearPatenteDAL();
        IPermisosDAL CrearPermisosDAL();
        ISistemaDAL CrearSistemaDAL();
        ISuscripcionDAL CrearSuscripcionDAL();
        IUsuarioDAL CrearUsuarioDAL();
    }
}
