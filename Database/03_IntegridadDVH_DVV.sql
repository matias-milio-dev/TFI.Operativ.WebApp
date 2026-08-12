/* ==============================================================================
   Operativ - Script 03: Integridad de datos (DVH / DVV)

   Punto 8 de MEJORA-Requerimientos-Operativ.md: el calculo de DVH (por fila)
   y DVV (por tabla) se revierte de SPs/funcion SQL a codigo de aplicacion:
   - Operativ.Comun.IntegridadHelper: formato de valores y SHA-256 sobre la
     concatenacion deterministica de columnas (equivalente a lo que hacia
     dbo.fn_CalcularDigitoVerificador + HASHBYTES('SHA2_256', ...)).
   - Operativ.DAL.SistemaDAL: RepararBaseDatos() recalcula DVH/DVV de las 14
     tablas criticas; VerificarIntegridad()/VerificarIntegridadLogin() las
     verifican. Cada DAL de escritura (UsuarioDAL, ClienteDAL, etc.) actualiza
     el DVH de la fila afectada y el DVV de su tabla en la misma transaccion.

   Este script no crea objetos. El calculo inicial de DVH/DVV sobre los datos
   semilla se hace en el script 07 (ejecutado una sola vez, antes del primer
   login, ya que en ese momento la aplicacion todavia no tiene sesion activa
   para invocar "Reparar Base de Datos" desde la UI).
   ============================================================================== */
USE Operativ;
GO

PRINT 'Script 03: sin procedimientos ni funciones almacenadas (ver Operativ.Comun.IntegridadHelper y Operativ.DAL.SistemaDAL).';
GO
