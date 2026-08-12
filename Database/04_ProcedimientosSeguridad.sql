/* ==============================================================================
   Operativ - Script 04: Seguridad (Usuario, Familia, Patente, UsuarioFamilia,
   FamiliaPatente, UsuarioPatente, Bitacora).

   Punto 8 de MEJORA-Requerimientos-Operativ.md: se revierte el uso de
   procedimientos almacenados. Todo el acceso a estas tablas se resuelve con
   CommandText parametrizado desde Operativ.DAL (UsuarioDAL, FamiliaDAL,
   PatenteDAL, PermisosDAL, BitacoraDAL); el calculo de DVH/DVV se resuelve en
   Operativ.Comun.IntegridadHelper y Operativ.DAL.SistemaDAL. Este script no
   crea objetos: se conserva solo como referencia historica de la version
   basada en SPs.
   ============================================================================== */
USE Operativ;
GO

PRINT 'Script 04: sin procedimientos almacenados (ver Operativ.DAL para el acceso a datos vigente).';
GO
