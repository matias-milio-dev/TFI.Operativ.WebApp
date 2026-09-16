-- =============================================================
-- Operativ - Datos de demo para la rama release/autentitacion-seguridad
--
-- Agrega 15 usuarios nuevos (distintas familias, con patentes
-- individuales), actualiza el email del webmaster y le otorga
-- todas las patentes del sistema, y agrega bitacora historica
-- coherente con esos usuarios.
--
-- Contrasena de los 15 usuarios nuevos: Operativ123
-- (mismo algoritmo que CrearBaseDatos.sql: SHA-256 + salt
-- individual por usuario, via Operativ.SEC.Helpers.HashHelper)
--
-- Al final recalcula DVH/DVV de las tablas tocadas con el mismo
-- algoritmo de Operativ.DAL.Integridad.IntegridadHelper (validado
-- contra los valores ya almacenados antes de escribir este script),
-- para que la verificacion de integridad no marque nada como corrupto.
--
-- Idempotencia: NO esta pensado para correr dos veces. Antes de
-- correrlo, confirmar que los usuarios NombreUsuario listados abajo
-- todavia no existen (ver bloque de verificacion inicial).
-- =============================================================

USE OperativDb;
GO

IF EXISTS (SELECT 1 FROM Usuario WHERE NombreUsuario IN ('mfernandez','jrodriguez','lgimenez','sortiz','mgaudio','vromero','fdiaz','nsosa','lacosta','cbenitez','mtorres','dcastro','yflores','rherrera','epaz'))
BEGIN
    RAISERROR('Alguno de los usuarios de demo ya existe. Abortando para no duplicar.', 16, 1);
    RETURN;
END
GO

-- =============================================================
-- 1. Usuarios nuevos (15), familia y patentes individuales
-- =============================================================

INSERT INTO Usuario (NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, ContrasenaProvisoria, Activo) VALUES
    ('mfernandez', 'D6L4yPEvfshWiArsZdXiYuy5u+IazJokXzD27+D3NFY=', 'rXlajFwA57Q+Ju75WqezSw==', 'mariana.fernandez@operativ.com', 'Mariana Fernandez', 0, 0, 0, 1),
    ('jrodriguez', 'GTZHR9RUVdehQfNMs+UI23k+KB6PNNoWg9qnpIFAsB8=', 'txHyae9AT31O/niasFSeuQ==', 'javier.rodriguez@operativ.com', 'Javier Rodriguez', 0, 0, 0, 1),
    ('lgimenez', 'VSzZtYJar/H+/lcgD5u9VSehrsNoH5heGtbbd5PmPmU=', 'sfSojvvWADXT2JCuTcQ4bQ==', 'lucia.gimenez@operativ.com', 'Lucia Gimenez', 0, 0, 0, 1),
    ('sortiz', 'D9VfN12zp1IWGkeOm2OwCsOZkObPy2wf4vjXMjHH1iA=', 'xCII816skmQ85g2pmnaqKw==', 'sebastian.ortiz@operativ.com', 'Sebastian Ortiz', 0, 0, 0, 1),
    ('mgaudio', 'Xms5qNDrt1oAj7CR8Mf/XqGTUVgWzaZwtMWZ+2l3PC8=', 'UJeQcPwsOhLUedxVLXHPcQ==', 'gaudiomatias@gmail.com', 'Matias Gaudio', 0, 0, 0, 1),
    ('vromero', 'smJ0qS1n4h1/bDzib/jmXjbP8/lNU7DWv3trA/tohPU=', 'mlZ+4/qcOJrgaAmsNh7X9g==', 'valentina.romero@operativ.com', 'Valentina Romero', 0, 0, 0, 1),
    ('fdiaz', 'nOtFD3M0bBFiVL8xA46/9WEuGae9Py8BX3K/zuHTUho=', '8EZLjH00xIhup29c9X3Viw==', 'franco.diaz@operativ.com', 'Franco Diaz', 0, 0, 0, 1),
    ('nsosa', 'ukaAL/5q24TZysdh0/WLWWDndFLzSEx9CkmwFEDPmcE=', 'CY3RR3QRZeWmDxVohUxY8Q==', 'natalia.sosa@operativ.com', 'Natalia Sosa', 1, 3, 0, 1),
    ('lacosta', 'jheMd0Ov/Uuzfkg80RGP4pLi5xtoEUYhn/4j+jIemrI=', '40EznevEZ3CmPTmxsqP9Mw==', 'lautaro.acosta@operativ.com', 'Lautaro Acosta', 0, 0, 0, 1),
    ('cbenitez', 'QER7M/lWXr7rCb+b/aBLIX3zuRfejNePYC4uGtoCDcY=', 'cQOb1k+wKi1tIQU8Hr0soQ==', 'camila.benitez@operativ.com', 'Camila Benitez', 0, 0, 1, 1),
    ('mtorres', 'V4T4HE1j9gLfVJoM4dlOrCQON/Sh86NiYS66scKtp3g=', 'tJVBlq7f3zpAt0h6m5rofA==', 'martina.torres@operativ.com', 'Martina Torres', 0, 0, 0, 1),
    ('dcastro', 'C5iOsnjN02e933hnq3Nhp99OyPD7eT+PUms6E7mH7xM=', '3N+yRz5K9ZxSZcuQaKR/wg==', 'diego.castro@operativ.com', 'Diego Castro', 1, 0, 0, 1),
    ('yflores', 'Xm9sgG88ZzMOJuqH5JhB8ejelSWUJtVyGJOvu1PZ5qs=', 'qtYpBFPWmlizXDc1P9GD2g==', 'yamila.flores@operativ.com', 'Yamila Flores', 0, 0, 0, 1),
    ('rherrera', 'rAeFo0gmR1rsffa8Fs+Zzfb/sDCPHeP1ajRCqUykZkU=', 'Q6wjVdpuJLs6iVEHax+c0w==', 'ramiro.herrera@operativ.com', 'Ramiro Herrera', 0, 0, 0, 1),
    ('epaz', 'R0EnyXj5TfC3tpkJhWoZs/SNAqdVcPnblzj1HdpjdEk=', 'ppOAyH4670pZZccCAulu8Q==', 'emilia.paz@operativ.com', 'Emilia Paz', 0, 0, 0, 1);
GO

-- IDs esperados: mfernandez=5, jrodriguez=6, lgimenez=7, sortiz=8, mgaudio=9,
-- vromero=10, fdiaz=11, nsosa=12, lacosta=13, cbenitez=14, mtorres=15,
-- dcastro=16, yflores=17, rherrera=18, epaz=19 (Usuario tenia 4 filas antes).

INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia)
SELECT U.IdUsuario, F.IdFamilia
FROM Usuario U, Familia F
WHERE (U.NombreUsuario IN ('mfernandez','jrodriguez','lgimenez','sortiz') AND F.Nombre = 'Administrador')
   OR (U.NombreUsuario IN ('mgaudio','vromero','fdiaz','nsosa') AND F.Nombre = 'Comercial')
   OR (U.NombreUsuario IN ('lacosta','cbenitez','mtorres','dcastro','yflores') AND F.Nombre = 'Cliente')
   OR (U.NombreUsuario IN ('rherrera','epaz') AND F.Nombre = 'WebMaster');
GO

INSERT INTO UsuarioPatente (IdUsuario, IdPatente)
SELECT U.IdUsuario, P.IdPatente
FROM Usuario U, Patente P
WHERE (U.NombreUsuario = 'mfernandez' AND P.Nombre IN ('RealizarBackup','ConsultarBitacora'))
   OR (U.NombreUsuario = 'jrodriguez' AND P.Nombre IN ('RestaurarBackup','GestionarClientes'))
   OR (U.NombreUsuario = 'lgimenez' AND P.Nombre IN ('GestionarCatalogo','ReportarIncidentes'))
   OR (U.NombreUsuario = 'sortiz' AND P.Nombre IN ('ConsultarFacturas','RepararBaseDatos'))
   OR (U.NombreUsuario = 'mgaudio' AND P.Nombre IN ('ConsultarUsuario','ConsultarBitacora'))
   OR (U.NombreUsuario = 'vromero' AND P.Nombre IN ('AltaUsuario','ModificacionUsuario'))
   OR (U.NombreUsuario = 'fdiaz' AND P.Nombre IN ('BajaUsuario','DesbloqueoUsuario'))
   OR (U.NombreUsuario = 'nsosa' AND P.Nombre IN ('BloqueoUsuario','AsignarPatente'))
   OR (U.NombreUsuario = 'lacosta' AND P.Nombre IN ('ConsultarUsuario','RealizarBackup'))
   OR (U.NombreUsuario = 'cbenitez' AND P.Nombre IN ('ModificacionUsuario','RestaurarBackup'))
   OR (U.NombreUsuario = 'mtorres' AND P.Nombre IN ('AltaUsuario','GestionarCatalogo'))
   OR (U.NombreUsuario = 'dcastro' AND P.Nombre IN ('BajaUsuario','GestionarClientes'))
   OR (U.NombreUsuario = 'yflores' AND P.Nombre IN ('DesbloqueoUsuario','ConsultarBitacora'))
   OR (U.NombreUsuario = 'rherrera' AND P.Nombre IN ('ConsultarUsuario','AltaUsuario'))
   OR (U.NombreUsuario = 'epaz' AND P.Nombre IN ('BajaUsuario','ModificacionUsuario'));
GO

-- =============================================================
-- 2. Webmaster: nuevo email y todas las patentes del sistema
-- =============================================================

UPDATE Usuario SET Email = 'gaudiomatias@live.com' WHERE NombreUsuario = 'webmaster';
GO

INSERT INTO UsuarioPatente (IdUsuario, IdPatente)
SELECT U.IdUsuario, P.IdPatente
FROM Usuario U, Patente P
WHERE U.NombreUsuario = 'webmaster'
  AND NOT EXISTS (SELECT 1 FROM UsuarioPatente UP WHERE UP.IdUsuario = U.IdUsuario AND UP.IdPatente = P.IdPatente);
GO

-- =============================================================
-- 3. Bitacora: eventos especificos coherentes con los usuarios
--    (@Hoy es DATETIME, no DATE: DATEADD(MINUTE, ...) no se puede
--    aplicar sobre DATE. Todo el bloque queda en un solo batch
--    porque las variables locales no sobreviven a un GO.)
-- =============================================================

DECLARE @Hoy DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);

INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion)
SELECT U.IdUsuario, DATEADD(MINUTE, T.Minuto, DATEADD(DAY, T.Offset, @Hoy)), 'AltaUsuario', 'Informativo', 'Alta de usuario'
FROM (VALUES
    ('mfernandez', -13, 555), ('jrodriguez', -13, 605), ('lgimenez', -12, 690), ('sortiz', -12, 840),
    ('mgaudio', -3, 540), ('vromero', -11, 615), ('fdiaz', -10, 660), ('nsosa', -9, 585),
    ('lacosta', -8, 800), ('cbenitez', -7, 545), ('mtorres', -6, 900), ('dcastro', -5, 630),
    ('yflores', -4, 885), ('rherrera', -2, 570), ('epaz', -1, 675)
) AS T (NombreUsuario, Offset, Minuto)
INNER JOIN Usuario U ON U.NombreUsuario = T.NombreUsuario;

INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion)
SELECT U.IdUsuario, DATEADD(MINUTE, T.Minuto, DATEADD(DAY, T.Offset, @Hoy)), 'AsignacionPatente', 'Advertencia', T.Descripcion
FROM (VALUES
    ('mfernandez', -13, 580, 'Asignacion de patentes individuales a un usuario: RealizarBackup, ConsultarBitacora'),
    ('jrodriguez', -12, 560, 'Asignacion de patentes individuales a un usuario: RestaurarBackup, GestionarClientes'),
    ('lgimenez', -12, 710, 'Asignacion de patentes individuales a un usuario: GestionarCatalogo, ReportarIncidentes'),
    ('sortiz', -11, 550, 'Asignacion de patentes individuales a un usuario: ConsultarFacturas, RepararBaseDatos'),
    ('mgaudio', -3, 565, 'Asignacion de patentes individuales a un usuario: ConsultarUsuario, ConsultarBitacora'),
    ('vromero', -10, 940, 'Asignacion de patentes individuales a un usuario: AltaUsuario, ModificacionUsuario'),
    ('fdiaz', -9, 605, 'Asignacion de patentes individuales a un usuario: BajaUsuario, DesbloqueoUsuario'),
    ('nsosa', -8, 570, 'Asignacion de patentes individuales a un usuario: BloqueoUsuario, AsignarPatente'),
    ('lacosta', -7, 855, 'Asignacion de patentes individuales a un usuario: ConsultarUsuario, RealizarBackup'),
    ('cbenitez', -6, 650, 'Asignacion de patentes individuales a un usuario: ModificacionUsuario, RestaurarBackup'),
    ('mtorres', -5, 555, 'Asignacion de patentes individuales a un usuario: AltaUsuario, GestionarCatalogo'),
    ('dcastro', -4, 680, 'Asignacion de patentes individuales a un usuario: BajaUsuario, GestionarClientes'),
    ('yflores', -3, 965, 'Asignacion de patentes individuales a un usuario: DesbloqueoUsuario, ConsultarBitacora'),
    ('rherrera', -2, 900, 'Asignacion de patentes individuales a un usuario: ConsultarUsuario, AltaUsuario'),
    ('epaz', -1, 945, 'Asignacion de patentes individuales a un usuario: BajaUsuario, ModificacionUsuario'),
    ('webmaster', -2, 1020, 'Asignacion de patentes individuales a un usuario: todas las patentes del sistema')
) AS T (NombreUsuario, Offset, Minuto, Descripcion)
INNER JOIN Usuario U ON U.NombreUsuario = T.NombreUsuario;

-- Bloqueo automatico de nsosa tras 3 intentos fallidos (hace 2 dias)
-- y bloqueo manual de dcastro por un administrador (ayer).
INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion) VALUES
    ((SELECT IdUsuario FROM Usuario WHERE NombreUsuario = 'nsosa'), DATEADD(MINUTE, 840, DATEADD(DAY, -2, @Hoy)), 'IntentoLoginFallido', 'Critico', 'Login con credenciales invalidas'),
    ((SELECT IdUsuario FROM Usuario WHERE NombreUsuario = 'nsosa'), DATEADD(MINUTE, 843, DATEADD(DAY, -2, @Hoy)), 'IntentoLoginFallido', 'Critico', 'Login con credenciales invalidas'),
    ((SELECT IdUsuario FROM Usuario WHERE NombreUsuario = 'nsosa'), DATEADD(MINUTE, 846, DATEADD(DAY, -2, @Hoy)), 'IntentoLoginFallido', 'Critico', 'Login con credenciales invalidas'),
    ((SELECT IdUsuario FROM Usuario WHERE NombreUsuario = 'nsosa'), DATEADD(MINUTE, 848, DATEADD(DAY, -2, @Hoy)), 'LoginBloqueado', 'Critico', 'Usuario bloqueado tras 3 intentos fallidos'),
    ((SELECT IdUsuario FROM Usuario WHERE NombreUsuario = 'dcastro'), DATEADD(MINUTE, 660, DATEADD(DAY, -1, @Hoy)), 'BloqueoManualUsuario', 'Advertencia', 'Bloqueo manual de usuario por administrador');

INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion)
SELECT U.IdUsuario, DATEADD(MINUTE, T.Minuto, DATEADD(DAY, T.Offset, @Hoy)), 'CambioClave', 'Informativo', 'Cambio de contrasena por autogestion'
FROM (VALUES ('admin', -9, 610), ('cliente', -6, 555), ('cbenitez', -1, 900), ('yflores', -3, 610)) AS T (NombreUsuario, Offset, Minuto)
INNER JOIN Usuario U ON U.NombreUsuario = T.NombreUsuario;

INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion)
SELECT U.IdUsuario, DATEADD(MINUTE, T.Minuto, DATEADD(DAY, T.Offset, @Hoy)), 'RecuperacionContrasena', 'Advertencia', 'Contrasena restablecida por recuperacion'
FROM (VALUES ('comercial', -10, 545), ('fdiaz', -7, 900), ('epaz', -1, 610)) AS T (NombreUsuario, Offset, Minuto)
INNER JOIN Usuario U ON U.NombreUsuario = T.NombreUsuario;

INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion) VALUES
    (NULL, DATEADD(MINUTE, 130, DATEADD(DAY, -12, @Hoy)), 'BackupBaseDatos', 'Advertencia', 'Backup de la base de datos generado bajo demanda: C:\OperativBackups\OperativDb_20260901_020000.bak'),
    (NULL, DATEADD(MINUTE, 130, DATEADD(DAY, -8, @Hoy)), 'BackupBaseDatos', 'Advertencia', 'Backup de la base de datos generado bajo demanda: C:\OperativBackups\OperativDb_20260905_020000.bak'),
    (NULL, DATEADD(MINUTE, 130, DATEADD(DAY, -4, @Hoy)), 'BackupBaseDatos', 'Advertencia', 'Backup de la base de datos generado bajo demanda: C:\OperativBackups\OperativDb_20260909_020000.bak'),
    (NULL, DATEADD(MINUTE, 130, @Hoy), 'BackupBaseDatos', 'Advertencia', 'Backup de la base de datos generado bajo demanda: C:\OperativBackups\OperativDb_hoy_020000.bak'),
    (NULL, DATEADD(MINUTE, 200, DATEADD(DAY, -8, @Hoy)), 'RestoreBaseDatos', 'Critico', 'Restore de la base de datos ejecutado bajo demanda: C:\OperativBackups\OperativDb_20260905_020000.bak'),
    (NULL, DATEADD(MINUTE, 200, DATEADD(DAY, -1, @Hoy)), 'RestoreBaseDatos', 'Critico', 'Restore de la base de datos ejecutado bajo demanda: C:\OperativBackups\OperativDb_hoy_020000.bak'),
    (NULL, DATEADD(MINUTE, 615, DATEADD(DAY, -1, @Hoy)), 'ReparacionEmergenciaBaseDatos', 'Critico', 'Base de datos reparada mediante acceso de emergencia del Web Master');

INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion)
SELECT U.IdUsuario, DATEADD(MINUTE, T.Minuto, DATEADD(DAY, T.Offset, @Hoy)), 'IntegridadCorrupta', 'Critico', T.Descripcion
FROM (VALUES
    ('admin', -6, 560, 'Se detecto una alteracion en la integridad de los datos del sistema: tabla Usuario'),
    ('mgaudio', -4, 900, 'Se detecto una alteracion en la integridad de los datos del sistema: tabla Bitacora'),
    ('mtorres', -1, 550, 'Se detecto una alteracion en la integridad de los datos del sistema: tabla FamiliaPatente')
) AS T (NombreUsuario, Offset, Minuto, Descripcion)
INNER JOIN Usuario U ON U.NombreUsuario = T.NombreUsuario;
GO

-- =============================================================
-- 4. Bitacora: ruido de uso diario (logins/logouts + fallidos)
--    generado sobre los 19 usuarios existentes (id 1 a 19),
--    distribuido en los ultimos 13 dias.
-- =============================================================

DECLARE @HoyRuido DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);

WITH Numeros AS
(
    SELECT TOP (35) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N FROM sys.all_objects
),
Sesiones AS
(
    SELECT
        N,
        ((N - 1) % 19) + 1 AS IdUsuario,
        (N * 5) % 14 AS DiaOffset,
        480 + ((N * 37) % 600) AS MinutoLogin,
        5 + ((N * 13) % 50) AS DuracionMinutos
    FROM Numeros
)
INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion)
SELECT IdUsuario, DATEADD(MINUTE, MinutoLogin, DATEADD(DAY, -DiaOffset, @HoyRuido)), 'LoginExitoso', 'Informativo', 'Inicio de sesion exitoso'
FROM Sesiones
UNION ALL
SELECT IdUsuario, DATEADD(MINUTE, MinutoLogin + DuracionMinutos, DATEADD(DAY, -DiaOffset, @HoyRuido)), 'CierreSesion', 'Informativo', 'Cierre de sesion'
FROM Sesiones;
GO

DECLARE @HoyFallidos DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);

WITH Numeros AS
(
    SELECT TOP (6) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N FROM sys.all_objects
)
INSERT INTO Bitacora (IdUsuario, FechaHora, Accion, Criticidad, Descripcion)
SELECT
    ((N * 7) % 19) + 1,
    DATEADD(MINUTE, 400 + ((N * 53) % 500), DATEADD(DAY, -((N * 3) % 13), @HoyFallidos)),
    'IntentoLoginFallido', 'Critico', 'Login con credenciales invalidas'
FROM Numeros;
GO

-- =============================================================
-- 5. Recalculo de DVH (filas nuevas o modificadas) y DVV
--    Mismo algoritmo que Operativ.DAL.Integridad.IntegridadHelper,
--    ya validado contra los DVH almacenados antes de este script.
-- =============================================================

WITH Posiciones AS
(
    SELECT TOP (4000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
),
Cadenas AS
(
    SELECT IdUsuario,
        CAST(IdUsuario AS VARCHAR(20)) + '|' + NombreUsuario + '|' + Contrasena + '|' + Salt + '|' + Email + '|' + NombreCompleto + '|' +
        CAST(CAST(Bloqueado AS INT) AS VARCHAR(1)) + '|' + CAST(IntentosFallidos AS VARCHAR(20)) + '|' +
        CAST(CAST(ContrasenaProvisoria AS INT) AS VARCHAR(1)) + '|' + CAST(CAST(Activo AS INT) AS VARCHAR(1)) AS Cadena
    FROM Usuario
    WHERE IdUsuario = 1 OR IdUsuario >= 5
),
Digitos AS
(
    SELECT C.IdUsuario, X.Dvh
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
UPDATE U SET DVH = D.Dvh
FROM Usuario U
INNER JOIN Digitos D ON D.IdUsuario = U.IdUsuario;
GO

WITH Posiciones AS
(
    SELECT TOP (4000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
),
Cadenas AS
(
    SELECT IdUsuario, IdFamilia, CAST(IdUsuario AS VARCHAR(20)) + '|' + CAST(IdFamilia AS VARCHAR(20)) AS Cadena
    FROM UsuarioFamilia
    WHERE DVH IS NULL
),
Digitos AS
(
    SELECT C.IdUsuario, C.IdFamilia, X.Dvh
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
UPDATE UF SET DVH = D.Dvh
FROM UsuarioFamilia UF
INNER JOIN Digitos D ON D.IdUsuario = UF.IdUsuario AND D.IdFamilia = UF.IdFamilia;
GO

WITH Posiciones AS
(
    SELECT TOP (4000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
),
Cadenas AS
(
    SELECT IdUsuario, IdPatente, CAST(IdUsuario AS VARCHAR(20)) + '|' + CAST(IdPatente AS VARCHAR(20)) AS Cadena
    FROM UsuarioPatente
    WHERE DVH IS NULL
),
Digitos AS
(
    SELECT C.IdUsuario, C.IdPatente, X.Dvh
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
UPDATE UP SET DVH = D.Dvh
FROM UsuarioPatente UP
INNER JOIN Digitos D ON D.IdUsuario = UP.IdUsuario AND D.IdPatente = UP.IdPatente;
GO

WITH Posiciones AS
(
    SELECT TOP (4000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Posicion FROM sys.all_objects
),
Cadenas AS
(
    SELECT IdBitacora,
        CAST(IdBitacora AS VARCHAR(20)) + '|' + ISNULL(CAST(IdUsuario AS VARCHAR(20)), '') + '|' +
        CONVERT(VARCHAR(23), FechaHora, 121) + '|' + Accion + '|' + Criticidad + '|' + ISNULL(Descripcion, '') AS Cadena
    FROM Bitacora
    WHERE DVH IS NULL
),
Digitos AS
(
    SELECT C.IdBitacora, X.Dvh
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
UPDATE B SET DVH = D.Dvh
FROM Bitacora B
INNER JOIN Digitos D ON D.IdBitacora = B.IdBitacora;
GO

UPDATE DigitosVerticales SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM Usuario), 0), FechaCalculo = GETDATE() WHERE NombreTabla = 'Usuario';
GO
UPDATE DigitosVerticales SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM UsuarioFamilia), 0), FechaCalculo = GETDATE() WHERE NombreTabla = 'UsuarioFamilia';
GO
UPDATE DigitosVerticales SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM UsuarioPatente), 0), FechaCalculo = GETDATE() WHERE NombreTabla = 'UsuarioPatente';
GO
UPDATE DigitosVerticales SET ValorDVV = ISNULL((SELECT SUM(DVH) FROM Bitacora), 0), FechaCalculo = GETDATE() WHERE NombreTabla = 'Bitacora';
GO

-- =============================================================
-- 6. Verificacion
-- =============================================================

SELECT 'Usuario' AS Tabla, COUNT(*) AS Filas FROM Usuario
UNION ALL SELECT 'UsuarioFamilia', COUNT(*) FROM UsuarioFamilia
UNION ALL SELECT 'UsuarioPatente', COUNT(*) FROM UsuarioPatente
UNION ALL SELECT 'Bitacora', COUNT(*) FROM Bitacora;
GO
