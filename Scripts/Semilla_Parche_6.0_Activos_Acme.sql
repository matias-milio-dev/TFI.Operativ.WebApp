-- =============================================================
-- Operativ - Parche 6.0
-- Semilla de demostracion del dashboard "Mis activos" (Cliente).
--
-- Lleva a Acme Soluciones SRL a 24 activos (los 4 existentes + 20
-- nuevos) para que el reporte muestre 3 paginas de 8 registros:
--     13 Disponibles / 8 Asignados / 3 En reparacion.
--
-- Es idempotente: un activo se inserta solo si su numero de serie no
-- existe. Al final recalcula DVH/DVV de Activo con el mismo algoritmo
-- de Operativ.DAL.Integridad.IntegridadHelper.
-- =============================================================

USE OperativDb;
GO

INSERT INTO Activo (Nombre, Modelo, NumeroSerie, Especificaciones, IdPaquete, IdCliente, Estado, Habilitado)
SELECT V.Nombre, V.Modelo, V.NumeroSerie, V.Especificaciones, P.IdPaquete, C.IdCliente, V.Estado, 1
FROM (VALUES
    ('Notebook Desarrollo 03', 'Dell Latitude 5540',    'DL5540-AR-0003',   'Intel Core i7-1355U, 32 GB RAM, SSD 1 TB NVMe',  'Desarrollador .NET',                          'Disponible'),
    ('Notebook Desarrollo 04', 'Dell Latitude 5540',    'DL5540-AR-0004',   'Intel Core i7-1355U, 32 GB RAM, SSD 1 TB NVMe',  'Desarrollador .NET',                          'Asignado'),
    ('Notebook Desarrollo 05', 'Lenovo ThinkPad E14',   'LTE14-AR-0101',    'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB',     'Desarrollador .NET',                          'Disponible'),
    ('Notebook Desarrollo 06', 'Lenovo ThinkPad E14',   'LTE14-AR-0102',    'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB',     'Desarrollador .NET',                          'Asignado'),
    ('Notebook Infra 02',      'Lenovo ThinkPad P16s',  'LTP16S-AR-0008',   'Intel Core i7-1360P, 64 GB RAM, SSD 2 TB NVMe',  'Desarrollador .NET con privilegios elevados', 'Disponible'),
    ('Notebook Infra 03',      'Lenovo ThinkPad P16s',  'LTP16S-AR-0009',   'Intel Core i7-1360P, 64 GB RAM, SSD 2 TB NVMe',  'Desarrollador .NET con privilegios elevados', 'EnReparacion'),
    ('Notebook QA 02',         'HP ProBook 450 G10',    'HPPB450-AR-0013',  'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB NVMe', 'QA Automation',                              'Asignado'),
    ('Notebook QA 03',         'HP ProBook 450 G10',    'HPPB450-AR-0014',  'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB NVMe', 'QA Automation',                              'Disponible'),
    ('Notebook QA 04',         'HP ProBook 450 G10',    'HPPB450-AR-0015',  'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB NVMe', 'QA Automation',                              'Disponible'),
    ('Notebook Analisis 02',   'Lenovo ThinkPad E14',   'LTE14-AR-0103',    'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB',     'Business Analyst',                            'Asignado'),
    ('Notebook Analisis 03',   'Lenovo ThinkPad E14',   'LTE14-AR-0104',    'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB',     'Business Analyst',                            'Disponible'),
    ('Notebook Analisis 04',   'HP ProBook 450 G10',    'HPPB450-AR-0016',  'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB NVMe', 'Business Analyst',                           'Disponible'),
    ('Notebook Soporte 01',    'Dell Latitude 5540',    'DL5540-AR-0005',   'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB',     'Business Analyst',                            'Disponible'),
    ('Notebook Soporte 02',    'Dell Latitude 5540',    'DL5540-AR-0006',   'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB',     'Business Analyst',                            'EnReparacion'),
    ('Notebook Frontend 02',   'MacBook Air M3',        'MBA-M3-AR-0101',   'Apple M3, 16 GB RAM unificada, SSD 512 GB',      'Desarrollador Frontend React/Angular',        'Asignado'),
    ('Notebook Frontend 03',   'MacBook Air M3',        'MBA-M3-AR-0102',   'Apple M3, 16 GB RAM unificada, SSD 512 GB',      'Desarrollador Frontend React/Angular',        'Disponible'),
    ('Notebook Frontend 04',   'MacBook Air M3',        'MBA-M3-AR-0103',   'Apple M3, 16 GB RAM unificada, SSD 512 GB',      'Desarrollador Frontend React/Angular',        'EnReparacion'),
    ('Notebook Frontend 05',   'MacBook Pro 14 M3',     'MBP14-M3-AR-0001', 'Apple M3 Pro, 18 GB RAM unificada, SSD 512 GB',  'Desarrollador Frontend React/Angular',        'Disponible'),
    ('Notebook Desarrollo 07', 'Dell Precision 5680',   'DP5680-AR-0001',   'Intel Core i9-13900H, 64 GB RAM, SSD 2 TB NVMe', 'Desarrollador .NET con privilegios elevados', 'Asignado'),
    ('Notebook Desarrollo 08', 'Dell Precision 5680',   'DP5680-AR-0002',   'Intel Core i9-13900H, 64 GB RAM, SSD 2 TB NVMe', 'Desarrollador .NET con privilegios elevados', 'Disponible')
) AS V (Nombre, Modelo, NumeroSerie, Especificaciones, NombrePaquete, Estado)
INNER JOIN Paquete P ON P.Nombre = V.NombrePaquete
INNER JOIN Cliente C ON C.RazonSocial = 'Acme Soluciones SRL'
WHERE NOT EXISTS (SELECT 1 FROM Activo A WHERE A.NumeroSerie = V.NumeroSerie);
GO

WITH Posiciones AS
(
    SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
),
Cadenas AS
(
    SELECT IdActivo,
           CAST(IdActivo AS VARCHAR(20)) + '|' + Nombre + '|' + Modelo + '|' + NumeroSerie + '|'
           + ISNULL(Especificaciones, '') + '|' + CAST(IdPaquete AS VARCHAR(20)) + '|'
           + CAST(IdCliente AS VARCHAR(20)) + '|' + Estado + '|' + CAST(Habilitado AS VARCHAR(1)) AS Cadena
    FROM Activo
),
Digitos AS
(
    SELECT C.IdActivo, X.Dvh
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
UPDATE A SET DVH = D.Dvh
FROM Activo A
INNER JOIN Digitos D ON D.IdActivo = A.IdActivo;
GO

UPDATE DigitosVerticales
SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM Activo), 0), FechaCalculo = GETDATE()
WHERE NombreTabla = 'Activo';
GO
