/* ==============================================================================
   Operativ - Script 06: Rutinas de Backup / Restore (Web Master)
   Punto 8 de MEJORA-Requerimientos-Operativ.md: unicos procedimientos
   almacenados que sobreviven en toda la solucion, invocables exclusivamente
   desde Operativ.DAL.SistemaDAL (RealizarBackup/RealizarRestore) para el
   perfil Web Master (patente BASEDATOS_BACKUP), validado en Operativ.BLL
   antes de ejecutar.
   ============================================================================== */
USE Operativ;
GO

IF OBJECT_ID('dbo.sp_Sistema_RealizarBackup', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_Sistema_RealizarBackup;
GO
CREATE PROCEDURE dbo.sp_Sistema_RealizarBackup
    @RutaDestino NVARCHAR(260)
AS
BEGIN
    SET NOCOUNT ON;

    IF @RutaDestino IS NULL OR LEN(@RutaDestino) = 0
    BEGIN
        RAISERROR(N'Ruta de backup invalida.', 16, 1);
        RETURN;
    END

    DECLARE @Sql NVARCHAR(MAX) = N'BACKUP DATABASE [Operativ] TO DISK = N''' + REPLACE(@RutaDestino, '''', '''''') + N''' WITH INIT, COMPRESSION, STATS = 10;';
    EXEC sp_executesql @Sql;
END
GO

IF OBJECT_ID('dbo.sp_Sistema_RealizarRestore', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_Sistema_RealizarRestore;
GO
CREATE PROCEDURE dbo.sp_Sistema_RealizarRestore
    @RutaOrigen NVARCHAR(260)
AS
BEGIN
    SET NOCOUNT ON;

    IF @RutaOrigen IS NULL OR LEN(@RutaOrigen) = 0
    BEGIN
        RAISERROR(N'Ruta de restore invalida.', 16, 1);
        RETURN;
    END

    DECLARE @Sql NVARCHAR(MAX) = N'
        ALTER DATABASE [Operativ] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        RESTORE DATABASE [Operativ] FROM DISK = N''' + REPLACE(@RutaOrigen, '''', '''''') + N''' WITH REPLACE, STATS = 10;
        ALTER DATABASE [Operativ] SET MULTI_USER;';

    EXEC('USE master; ' + @Sql);
END
GO

PRINT 'Script 06 ejecutado: rutinas de backup/restore creadas correctamente.';
GO
