-- =============================================================
-- Operativ - Parche 2.0
-- Migracion incremental del catalogo de patentes de usuario.
--
-- Reemplaza la patente gruesa 'GestionarUsuarios' por el set fino
-- por caso de uso y le asigna las nuevas a la familia Administrador.
-- No borra ni recrea la base: conserva usuarios, bitacora y el resto
-- de los datos ya cargados. Es idempotente (se puede correr de nuevo).
--
-- Al final recalcula DVH/DVV de las tablas tocadas con el mismo
-- algoritmo de Operativ.DAL.Integridad.IntegridadHelper, para que la
-- verificacion de integridad no marque una alteracion falsa.
-- =============================================================

USE OperativDb;
GO

INSERT INTO Patente (Nombre, Descripcion)
SELECT nuevas.Nombre, nuevas.Descripcion
FROM (VALUES
    ('ConsultarUsuario', 'Consultar el listado de usuarios'),
    ('AltaUsuario', 'Dar de alta usuarios nuevos'),
    ('BajaUsuario', 'Dar de baja usuarios existentes'),
    ('ModificacionUsuario', 'Modificar los datos de un usuario'),
    ('DesbloqueoUsuario', 'Desbloquear usuarios bloqueados'),
    ('BloqueoUsuario', 'Bloquear usuarios manualmente'),
    ('AsignarPatente', 'Asignar patentes individuales a un usuario'),
    ('RemoverPatente', 'Quitar patentes individuales de un usuario')
) AS nuevas (Nombre, Descripcion)
WHERE NOT EXISTS (SELECT 1 FROM Patente P WHERE P.Nombre = nuevas.Nombre);
GO

DELETE FP
FROM FamiliaPatente FP
INNER JOIN Patente P ON P.IdPatente = FP.IdPatente
WHERE P.Nombre = 'GestionarUsuarios';
GO

DELETE UP
FROM UsuarioPatente UP
INNER JOIN Patente P ON P.IdPatente = UP.IdPatente
WHERE P.Nombre = 'GestionarUsuarios';
GO

DELETE FROM Patente WHERE Nombre = 'GestionarUsuarios';
GO

INSERT INTO FamiliaPatente (IdFamilia, IdPatente)
SELECT F.IdFamilia, P.IdPatente
FROM Familia F, Patente P
WHERE F.Nombre = 'Administrador'
  AND P.Nombre IN ('ConsultarUsuario', 'AltaUsuario', 'BajaUsuario', 'ModificacionUsuario', 'DesbloqueoUsuario', 'BloqueoUsuario', 'AsignarPatente', 'RemoverPatente')
  AND NOT EXISTS (SELECT 1 FROM FamiliaPatente FP WHERE FP.IdFamilia = F.IdFamilia AND FP.IdPatente = P.IdPatente);
GO

WITH Posiciones AS
(
    SELECT TOP (4000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
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
    SELECT TOP (4000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
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

UPDATE DigitosVerticales
SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM UsuarioPatente), 0), FechaCalculo = GETDATE()
WHERE NombreTabla = 'UsuarioPatente';
GO
