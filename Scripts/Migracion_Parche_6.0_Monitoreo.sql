-- =============================================================
-- Operativ - Parche 6.0
-- Migracion incremental: patente MonitorearActivos (dashboard de
-- monitoreo de activos del Cliente).
--
-- Agrega la patente y se la asigna a la familia Cliente. No borra ni
-- recrea la base. Es idempotente (se puede correr de nuevo).
--
-- Al final recalcula DVH/DVV de Patente y FamiliaPatente con el mismo
-- algoritmo de Operativ.DAL.Integridad.IntegridadHelper, para que la
-- verificacion de integridad no marque una alteracion falsa.
-- =============================================================

USE OperativDb;
GO

IF NOT EXISTS (SELECT 1 FROM Patente WHERE Nombre = 'MonitorearActivos')
BEGIN
    INSERT INTO Patente (Nombre, Descripcion)
    VALUES ('MonitorearActivos', 'Permite monitorear el estado de los activos de la empresa.');
END
GO

INSERT INTO FamiliaPatente (IdFamilia, IdPatente)
SELECT F.IdFamilia, P.IdPatente
FROM Familia F, Patente P
WHERE F.Nombre = 'Cliente'
  AND P.Nombre = 'MonitorearActivos'
  AND NOT EXISTS (SELECT 1 FROM FamiliaPatente FP WHERE FP.IdFamilia = F.IdFamilia AND FP.IdPatente = P.IdPatente);
GO

WITH Posiciones AS
(
    SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
),
Cadenas AS
(
    SELECT IdPatente, CAST(IdPatente AS VARCHAR(20)) + '|' + Nombre + '|' + Descripcion AS Cadena FROM Patente
),
Digitos AS
(
    SELECT C.IdPatente, X.Dvh
    FROM Cadenas C
    CROSS APPLY (
        SELECT SUM(V.Valor) AS Dvh
        FROM (
            SELECT CAST(UNICODE(SUBSTRING(C.Cadena, N.Posicion, 1)) AS BIGINT) * N.Posicion AS Valor
            FROM Posiciones N
            WHERE N.Posicion <= DATALENGTH(C.Cadena)
        ) V
    ) X
)
UPDATE P SET DVH = D.Dvh
FROM Patente P
INNER JOIN Digitos D ON D.IdPatente = P.IdPatente;
GO

WITH Posiciones AS
(
    SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
),
Cadenas AS
(
    SELECT IdFamilia, IdPatente, CAST(IdFamilia AS VARCHAR(20)) + '|' + CAST(IdPatente AS VARCHAR(20)) AS Cadena FROM FamiliaPatente
),
Digitos AS
(
    SELECT C.IdFamilia, C.IdPatente, X.Dvh
    FROM Cadenas C
    CROSS APPLY (
        SELECT SUM(V.Valor) AS Dvh
        FROM (
            SELECT CAST(UNICODE(SUBSTRING(C.Cadena, N.Posicion, 1)) AS BIGINT) * N.Posicion AS Valor
            FROM Posiciones N
            WHERE N.Posicion <= DATALENGTH(C.Cadena)
        ) V
    ) X
)
UPDATE FP SET DVH = D.Dvh
FROM FamiliaPatente FP
INNER JOIN Digitos D ON D.IdFamilia = FP.IdFamilia AND D.IdPatente = FP.IdPatente;
GO

UPDATE DigitosVerticales
SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM Patente), 0), FechaCalculo = GETDATE()
WHERE NombreTabla = 'Patente';
GO

UPDATE DigitosVerticales
SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM FamiliaPatente), 0), FechaCalculo = GETDATE()
WHERE NombreTabla = 'FamiliaPatente';
GO
