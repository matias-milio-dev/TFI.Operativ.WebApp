/* ==============================================================================
   Operativ - Script 05: Negocio (Cliente, Paquete, Suscripcion, Pago, Factura,
   Activo, Incidente, Monitoreo).

   Punto 8 de MEJORA-Requerimientos-Operativ.md: se revierte el uso de
   procedimientos almacenados. Todo el acceso a estas tablas se resuelve con
   CommandText parametrizado desde Operativ.DAL (ClienteDAL, PaqueteDAL,
   SuscripcionDAL, PagoDAL, FacturaDAL, ActivoDAL, IncidenteDAL, SistemaDAL).
   Este script no crea objetos: se conserva solo como referencia historica de
   la version basada en SPs.
   ============================================================================== */
USE Operativ;
GO

PRINT 'Script 05: sin procedimientos almacenados (ver Operativ.DAL para el acceso a datos vigente).';
GO
