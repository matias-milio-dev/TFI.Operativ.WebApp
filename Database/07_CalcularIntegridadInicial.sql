/* ==============================================================================
   Operativ - Script 07: Calculo inicial de DVH/DVV
   Debe ejecutarse una unica vez despues de cargar los datos semilla (02) y
   antes del primer login.

   Punto 8 de MEJORA-Requerimientos-Operativ.md: la aplicacion ya no usa
   procedimientos almacenados para esto (ver Operativ.DAL.SistemaDAL.
   RepararBaseDatos, invocable luego desde Administracion > Reparar Base de
   Datos). Este script es la unica excepcion practica: se ejecuta una sola
   vez, fuera de la aplicacion (sqlcmd/SSMS), como paso de instalacion, dado
   que en ese momento todavia no existe ninguna sesion autenticada capaz de
   invocar esa pantalla. No queda ningun objeto persistido en la base: es SQL
   de uso unico, no un procedimiento almacenado reutilizable por la app.
   ============================================================================== */
USE Operativ;
GO

DECLARE @Tablas TABLE (NombreTabla VARCHAR(100), NombreColumnaId VARCHAR(100));
INSERT INTO @Tablas (NombreTabla, NombreColumnaId) VALUES
    ('Usuario', 'IdUsuario'), ('Familia', 'IdFamilia'), ('Patente', 'IdPatente'),
    ('UsuarioFamilia', 'IdUsuarioFamilia'), ('FamiliaPatente', 'IdFamiliaPatente'),
    ('UsuarioPatente', 'IdUsuarioPatente'), ('Bitacora', 'IdBitacora'),
    ('Cliente', 'IdCliente'), ('Paquete', 'IdPaquete'), ('Suscripcion', 'IdSuscripcion'),
    ('Pago', 'IdPago'), ('Factura', 'IdFactura'), ('Activo', 'IdActivo'),
    ('Incidente', 'IdIncidente');

DECLARE @NombreTabla VARCHAR(100), @NombreColumnaId VARCHAR(100);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT NombreTabla, NombreColumnaId FROM @Tablas;
OPEN cur;
FETCH NEXT FROM cur INTO @NombreTabla, @NombreColumnaId;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ListaColumnas NVARCHAR(MAX) = (
        SELECT STRING_AGG(QUOTENAME(c.name), N' + ''|'' + ') WITHIN GROUP (ORDER BY c.column_id)
        FROM sys.columns c
        WHERE c.object_id = OBJECT_ID('dbo.' + @NombreTabla) AND c.name <> 'DVH'
    );

    DECLARE @SqlDVH NVARCHAR(MAX) = N'
        UPDATE t SET DVH = HASHBYTES(''SHA2_256'', CONCAT(' + @ListaColumnas + N'))
        FROM dbo.' + QUOTENAME(@NombreTabla) + N' AS t;';
    EXEC sp_executesql @SqlDVH;

    DECLARE @SqlDVV NVARCHAR(MAX) = N'
        DECLARE @Concat NVARCHAR(MAX);
        SELECT @Concat = STRING_AGG(CONVERT(VARCHAR(64), DVH, 2), '''') WITHIN GROUP (ORDER BY ' + QUOTENAME(@NombreColumnaId) + N')
        FROM dbo.' + QUOTENAME(@NombreTabla) + N';

        DECLARE @DVV VARBINARY(32) = HASHBYTES(''SHA2_256'', ISNULL(@Concat, N''''));

        MERGE dbo.DigitoVerificadorTabla AS destino
        USING (SELECT ''' + @NombreTabla + N''' AS NombreTabla) AS origen
        ON destino.NombreTabla = origen.NombreTabla
        WHEN MATCHED THEN UPDATE SET ValorDVV = @DVV, FechaCalculo = SYSDATETIME()
        WHEN NOT MATCHED THEN INSERT (NombreTabla, ValorDVV) VALUES (origen.NombreTabla, @DVV);';
    EXEC sp_executesql @SqlDVV;

    FETCH NEXT FROM cur INTO @NombreTabla, @NombreColumnaId;
END
CLOSE cur;
DEALLOCATE cur;
GO

PRINT 'Script 07 ejecutado: DVH/DVV inicial calculado para todas las tablas criticas.';
GO
