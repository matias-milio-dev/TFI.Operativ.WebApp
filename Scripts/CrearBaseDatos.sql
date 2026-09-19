-- =============================================================
-- Operativ - Entrega Oficial 1
-- Script de creacion de base de datos y datos semilla
--
-- Contrasena de todos los usuarios semilla: Operativ123
-- (guardada como hash SHA-256 + salt individual por usuario,
-- generados con el mismo algoritmo de Operativ.SEC.Helpers.HashHelper)
-- =============================================================

IF DB_ID('OperativDb') IS NOT NULL
BEGIN
    ALTER DATABASE OperativDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE OperativDb;
END
GO

CREATE DATABASE OperativDb;
GO

USE OperativDb;
GO

CREATE TABLE Cliente
(
    IdCliente INT IDENTITY(1,1) NOT NULL,
    RazonSocial VARCHAR(150) NOT NULL,
    Cuit VARCHAR(13) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Cliente_Activo DEFAULT (1),
    DVH BIGINT NULL,
    CONSTRAINT PK_Cliente PRIMARY KEY (IdCliente),
    CONSTRAINT UQ_Cliente_Cuit UNIQUE (Cuit)
);
GO

CREATE TABLE [Plan]
(
    IdPlan INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(500) NOT NULL,
    PrecioAnual DECIMAL(12,2) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Plan_Activo DEFAULT (1),
    DVH BIGINT NULL,
    CONSTRAINT PK_Plan PRIMARY KEY (IdPlan),
    CONSTRAINT UQ_Plan_Nombre UNIQUE (Nombre)
);
GO

-- Suscripcion no tiene columna de baja logica: la baja es el estado 'Cancelada'.
-- El estado 'Vencida' NO se persiste, se deriva al leer.
-- Ver Plan_Parche_5.0_Operativ.md seccion 1.3.
CREATE TABLE Suscripcion
(
    IdSuscripcion INT IDENTITY(1,1) NOT NULL,
    IdCliente INT NOT NULL,
    IdPlan INT NOT NULL,
    PrecioAnual DECIMAL(12,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    FechaAlta DATETIME NOT NULL CONSTRAINT DF_Suscripcion_FechaAlta DEFAULT (GETDATE()),
    FechaFinTrial DATETIME NOT NULL,
    FechaPago DATETIME NULL,
    FechaVencimiento DATETIME NULL,
    FechaCancelacion DATETIME NULL,
    MedioPago VARCHAR(20) NULL,
    CodigoComprobante VARCHAR(20) NULL,
    -- FechaPago, FechaVencimiento, MedioPago y CodigoComprobante los completa el pago
    -- (parche 5.1). Se crean ahora para no tener que hacer un ALTER TABLE despues.
    DVH BIGINT NULL,
    CONSTRAINT PK_Suscripcion PRIMARY KEY (IdSuscripcion),
    CONSTRAINT FK_Suscripcion_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente (IdCliente),
    CONSTRAINT FK_Suscripcion_Plan FOREIGN KEY (IdPlan) REFERENCES [Plan] (IdPlan)
);
GO

-- Indice filtrado y no UNIQUE comun: en SQL Server un UNIQUE trata los NULL como iguales y
-- admite uno solo, asi que un UNIQUE comun dejaria una unica suscripcion sin pagar en toda la
-- base. El filtro aplica la unicidad solo a las que ya tienen comprobante (parche 5.1).
-- Un indice filtrado exige QUOTED_IDENTIFIER ON; sqlcmd lo trae apagado por defecto.
SET QUOTED_IDENTIFIER ON;
GO

CREATE UNIQUE INDEX UQ_Suscripcion_CodigoComprobante
    ON Suscripcion (CodigoComprobante)
    WHERE CodigoComprobante IS NOT NULL;
GO

-- Para una base ya creada con una version anterior de este script, aplicar en su lugar:
-- ALTER TABLE Usuario ADD ContrasenaProvisoria BIT NOT NULL CONSTRAINT DF_Usuario_ContrasenaProvisoria DEFAULT (0);
-- ALTER TABLE Usuario ADD IdCliente INT NULL;
-- ALTER TABLE Usuario ADD CONSTRAINT FK_Usuario_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente (IdCliente);
CREATE TABLE Usuario
(
    IdUsuario INT IDENTITY(1,1) NOT NULL,
    NombreUsuario VARCHAR(50) NOT NULL,
    Contrasena VARCHAR(200) NOT NULL,
    Salt VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    NombreCompleto VARCHAR(150) NOT NULL,
    Bloqueado BIT NOT NULL CONSTRAINT DF_Usuario_Bloqueado DEFAULT (0),
    IntentosFallidos INT NOT NULL CONSTRAINT DF_Usuario_IntentosFallidos DEFAULT (0),
    ContrasenaProvisoria BIT NOT NULL CONSTRAINT DF_Usuario_ContrasenaProvisoria DEFAULT (0),
    Activo BIT NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1),
    IdCliente INT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuario_NombreUsuario UNIQUE (NombreUsuario),
    CONSTRAINT FK_Usuario_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente (IdCliente)
);
GO

CREATE TABLE Familia
(
    IdFamilia INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(300) NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_Familia PRIMARY KEY (IdFamilia),
    CONSTRAINT UQ_Familia_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE Patente
(
    IdPatente INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(300) NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_Patente PRIMARY KEY (IdPatente),
    CONSTRAINT UQ_Patente_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE UsuarioFamilia
(
    IdUsuario INT NOT NULL,
    IdFamilia INT NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_UsuarioFamilia PRIMARY KEY (IdUsuario, IdFamilia),
    CONSTRAINT FK_UsuarioFamilia_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario (IdUsuario),
    CONSTRAINT FK_UsuarioFamilia_Familia FOREIGN KEY (IdFamilia) REFERENCES Familia (IdFamilia)
);
GO

CREATE TABLE UsuarioPatente
(
    IdUsuario INT NOT NULL,
    IdPatente INT NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_UsuarioPatente PRIMARY KEY (IdUsuario, IdPatente),
    CONSTRAINT FK_UsuarioPatente_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario (IdUsuario),
    CONSTRAINT FK_UsuarioPatente_Patente FOREIGN KEY (IdPatente) REFERENCES Patente (IdPatente)
);
GO

CREATE TABLE FamiliaPatente
(
    IdFamilia INT NOT NULL,
    IdPatente INT NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_FamiliaPatente PRIMARY KEY (IdFamilia, IdPatente),
    CONSTRAINT FK_FamiliaPatente_Familia FOREIGN KEY (IdFamilia) REFERENCES Familia (IdFamilia),
    CONSTRAINT FK_FamiliaPatente_Patente FOREIGN KEY (IdPatente) REFERENCES Patente (IdPatente)
);
GO

CREATE TABLE FamiliaFamilia
(
    IdFamiliaPadre INT NOT NULL,
    IdFamiliaHija INT NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_FamiliaFamilia PRIMARY KEY (IdFamiliaPadre, IdFamiliaHija),
    CONSTRAINT FK_FamiliaFamilia_Padre FOREIGN KEY (IdFamiliaPadre) REFERENCES Familia (IdFamilia),
    CONSTRAINT FK_FamiliaFamilia_Hija FOREIGN KEY (IdFamiliaHija) REFERENCES Familia (IdFamilia)
);
GO

-- Para una base ya creada con una version anterior de este script, aplicar en su lugar:
-- ALTER TABLE Bitacora ALTER COLUMN IdUsuario INT NULL;
CREATE TABLE Bitacora
(
    IdBitacora INT IDENTITY(1,1) NOT NULL,
    IdUsuario INT NULL,
    FechaHora DATETIME NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT (GETDATE()),
    Accion VARCHAR(50) NOT NULL,
    Criticidad VARCHAR(20) NOT NULL,
    Descripcion VARCHAR(300) NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_Bitacora PRIMARY KEY (IdBitacora),
    CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario (IdUsuario)
);
GO

CREATE TABLE DigitosVerticales
(
    IdDigitoVertical INT IDENTITY(1,1) NOT NULL,
    NombreTabla VARCHAR(100) NOT NULL,
    ValorDVV BIGINT NOT NULL,
    FechaCalculo DATETIME NOT NULL CONSTRAINT DF_DigitosVerticales_FechaCalculo DEFAULT (GETDATE()),
    CONSTRAINT PK_DigitosVerticales PRIMARY KEY (IdDigitoVertical),
    CONSTRAINT UQ_DigitosVerticales_NombreTabla UNIQUE (NombreTabla)
);
GO

CREATE TABLE Programa
(
    IdPrograma INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(300) NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_Programa PRIMARY KEY (IdPrograma),
    CONSTRAINT UQ_Programa_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE Paquete
(
    IdPaquete INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(300) NOT NULL,
    TipoPermiso VARCHAR(20) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Paquete_Activo DEFAULT (1),
    DVH BIGINT NULL,
    CONSTRAINT PK_Paquete PRIMARY KEY (IdPaquete),
    CONSTRAINT UQ_Paquete_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE PaquetePrograma
(
    IdPaquete INT NOT NULL,
    IdPrograma INT NOT NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_PaquetePrograma PRIMARY KEY (IdPaquete, IdPrograma),
    CONSTRAINT FK_PaquetePrograma_Paquete FOREIGN KEY (IdPaquete) REFERENCES Paquete (IdPaquete),
    CONSTRAINT FK_PaquetePrograma_Programa FOREIGN KEY (IdPrograma) REFERENCES Programa (IdPrograma)
);
GO

-- El bit de baja logica se llama Habilitado y no Activo porque la entidad C# es la
-- clase Activo, y C# no permite un miembro con el mismo nombre que su tipo contenedor.
-- Para una base ya creada con una version anterior de este script, aplicar en su lugar:
-- ALTER TABLE Activo ADD IdCliente INT NULL;
-- UPDATE Activo SET IdCliente = (SELECT MIN(IdCliente) FROM Cliente) WHERE IdCliente IS NULL;
-- ALTER TABLE Activo ALTER COLUMN IdCliente INT NOT NULL;
-- ALTER TABLE Activo ADD CONSTRAINT FK_Activo_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente (IdCliente);
CREATE TABLE Activo
(
    IdActivo INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Modelo VARCHAR(100) NOT NULL,
    NumeroSerie VARCHAR(50) NOT NULL,
    Especificaciones VARCHAR(500) NULL,
    IdPaquete INT NOT NULL,
    IdCliente INT NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    Habilitado BIT NOT NULL CONSTRAINT DF_Activo_Habilitado DEFAULT (1),
    DVH BIGINT NULL,
    CONSTRAINT PK_Activo PRIMARY KEY (IdActivo),
    CONSTRAINT UQ_Activo_NumeroSerie UNIQUE (NumeroSerie),
    CONSTRAINT FK_Activo_Paquete FOREIGN KEY (IdPaquete) REFERENCES Paquete (IdPaquete),
    CONSTRAINT FK_Activo_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente (IdCliente)
);
GO

-- Incidente no tiene columna de baja logica a proposito: es un registro de auditoria del
-- activo y se cierra cambiando de estado, nunca se da de baja.
-- Ver Plan_Parche_4.3_Operativ.md seccion 0.
CREATE TABLE Incidente
(
    IdIncidente INT IDENTITY(1,1) NOT NULL,
    NumeroIncidente VARCHAR(20) NOT NULL,
    IdActivo INT NOT NULL,
    Descripcion VARCHAR(500) NOT NULL,
    Categoria VARCHAR(20) NOT NULL,
    Prioridad VARCHAR(20) NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    FechaAlta DATETIME NOT NULL CONSTRAINT DF_Incidente_FechaAlta DEFAULT (GETDATE()),
    FechaCierre DATETIME NULL,
    ComentarioResolucion VARCHAR(500) NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_Incidente PRIMARY KEY (IdIncidente),
    CONSTRAINT UQ_Incidente_NumeroIncidente UNIQUE (NumeroIncidente),
    CONSTRAINT FK_Incidente_Activo FOREIGN KEY (IdActivo) REFERENCES Activo (IdActivo)
);
GO

INSERT INTO Familia (Nombre, Descripcion) VALUES
    ('WebMaster', 'Mantenimiento tecnico de la plataforma'),
    ('Administrador', 'Gestion de usuarios y permisos'),
    ('Comercial', 'Gestion de clientes y catalogo'),
    ('Cliente', 'Suscripciones, activos, incidentes y facturas');
GO

INSERT INTO Patente (Nombre, Descripcion) VALUES
    ('RepararBaseDatos', 'Permite ejecutar el modulo de reparacion de la base de datos.'),
    ('RealizarBackup', 'Permite generar backups de la base de datos.'),
    ('RestaurarBackup', 'Permite restaurar la base de datos desde un backup existente.'),
    ('ConsultarUsuario', 'Permite ver el listado de usuarios de la plataforma.'),
    ('AltaUsuario', 'Permite crear nuevos usuarios en la plataforma.'),
    ('BajaUsuario', 'Permite dar de baja usuarios.'),
    ('ModificacionUsuario', 'Permite modificar los datos de un usuario.'),
    ('DesbloqueoUsuario', 'Permite desbloquear usuarios bloqueados.'),
    ('BloqueoUsuario', 'Permite bloquear usuarios.'),
    ('AsignarPatente', 'Permite asignar patentes a usuarios.'),
    ('RemoverPatente', 'Permite quitar patentes asignadas a un usuario.'),
    ('GestionarFamilias', 'Permite dar de alta, baja y modificar familias y sus patentes.'),
    ('GestionarClientes', 'Permite dar de alta, baja y modificar clientes.'),
    ('GestionarCatalogo', 'Permite administrar el catalogo de paquetes.'),
    ('GestionarActivos', 'Permite dar de alta, baja y modificar activos del inventario.'),
    ('GestionarSuscripciones', 'Permite contratar y administrar suscripciones.'),
    ('ConsultarSuscripciones', 'Permite consultar las suscripciones de todas las empresas cliente.'),
    ('ConsultarFacturas', 'Permite consultar las facturas emitidas.'),
    ('ReportarIncidentes', 'Permite reportar incidentes sobre activos.'),
    ('CerrarIncidente', 'Permite cerrar incidentes reportados por los clientes.'),
    ('ConsultarBitacora', 'Permite consultar los registros de actividad del sistema.');
GO

INSERT INTO FamiliaPatente (IdFamilia, IdPatente)
SELECT F.IdFamilia, P.IdPatente
FROM Familia F, Patente P
WHERE (F.Nombre = 'WebMaster' AND P.Nombre IN ('RepararBaseDatos', 'RealizarBackup', 'RestaurarBackup', 'ConsultarBitacora'))
   OR (F.Nombre = 'Administrador' AND P.Nombre IN ('ConsultarUsuario', 'AltaUsuario', 'BajaUsuario', 'ModificacionUsuario', 'DesbloqueoUsuario', 'BloqueoUsuario', 'AsignarPatente', 'RemoverPatente', 'GestionarFamilias'))
   OR (F.Nombre = 'Comercial' AND P.Nombre IN ('GestionarClientes', 'GestionarCatalogo', 'GestionarActivos', 'CerrarIncidente', 'ConsultarSuscripciones'))
   OR (F.Nombre = 'Cliente' AND P.Nombre IN ('GestionarSuscripciones', 'ConsultarFacturas', 'ReportarIncidentes'));
GO

INSERT INTO Cliente (RazonSocial, Cuit, Email, Activo) VALUES
    ('Acme Soluciones SRL', '30-71234567-4', 'contacto@acmesoluciones.com', 1),
    ('Nordex Logistica SA', '30-70987654-2', 'sistemas@nordexlogistica.com', 1);
GO

INSERT INTO [Plan] (Nombre, Descripcion, PrecioAnual, Activo) VALUES
    ('Operativ Base', 'Gestion de clientes, activos y paquetes de configuracion. Reporte y seguimiento de incidentes.', 200.00, 1),
    ('Operativ Pro', 'Todo lo incluido en Base, mas monitoreo en tiempo real y dashboard operativo.', 450.00, 1),
    ('Operativ Enterprise', 'Todo lo incluido en Pro, mas reportes avanzados e integraciones externas via Web Services.', 850.00, 1);
GO

-- Acme arranca con una suscripcion activa para que los incidentes semilla sigan siendo validos
-- una vez que AltaIncidente valida suscripcion vigente. Nordex queda sin suscripcion a proposito:
-- es el caso de prueba del alta y de esa validacion.
INSERT INTO Suscripcion (IdCliente, IdPlan, PrecioAnual, Estado, FechaAlta, FechaFinTrial, FechaPago, FechaVencimiento, MedioPago, CodigoComprobante)
SELECT C.IdCliente, P.IdPlan, P.PrecioAnual, 'Activa', GETDATE(), DATEADD(DAY, 30, GETDATE()), GETDATE(), DATEADD(YEAR, 1, GETDATE()), 'Transferencia', 'PAG-2026-00001'
FROM Cliente C, [Plan] P
WHERE C.RazonSocial = 'Acme Soluciones SRL' AND P.Nombre = 'Operativ Pro';
GO

INSERT INTO Usuario (NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, Activo) VALUES
    ('webmaster', 'WoBMmTsCakGUgk+pb9QhUs6TFDBQiz4l+CPaTasHDr4=', 'VzyZhW06zBFF+8F6U04Org==', 'webmaster@operativ.com', 'Walter Master', 0, 0, 1),
    ('admin', 'ODXNcwhp7cNHbAIXMB8CsTNakZ1JHnsC4ZB83ZPmy3s=', 'wt9gBTzByiuIIWRG9nu/Kw==', 'admin@operativ.com', 'Ana Dominguez', 0, 0, 1),
    ('comercial', '+Cq7n+SO3v6M8fxVzLiYWrw80oUBkCMejMYs9XYKbBc=', 'GZhBdZBNPPpRXtUvbH1jOA==', 'comercial@operativ.com', 'Carlos Mercado', 0, 0, 1),
    ('cliente', 'EwUchM5KrKeutndQDFdjULornohk+jv7Pqp67gSlGKo=', 'revGUyEuhPGbYIPZy/730A==', 'cliente@operativ.com', 'Clara Klein', 0, 0, 1);
GO

INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia)
SELECT U.IdUsuario, F.IdFamilia
FROM Usuario U, Familia F
WHERE (U.NombreUsuario = 'webmaster' AND F.Nombre = 'WebMaster')
   OR (U.NombreUsuario = 'admin' AND F.Nombre = 'Administrador')
   OR (U.NombreUsuario = 'comercial' AND F.Nombre = 'Comercial')
   OR (U.NombreUsuario = 'cliente' AND F.Nombre = 'Cliente');
GO

UPDATE U
SET U.IdCliente = C.IdCliente
FROM Usuario U, Cliente C
WHERE U.NombreUsuario = 'cliente' AND C.RazonSocial = 'Acme Soluciones SRL';
GO

INSERT INTO Programa (Nombre, Descripcion) VALUES
    ('Windows 11 Pro', 'Licencia de sistema operativo Windows 11 Professional.'),
    ('Office 365', 'Suite de ofimatica y colaboracion de Microsoft.'),
    ('Google Chrome', 'Navegador web.'),
    ('Git', 'Sistema de control de versiones distribuido.'),
    ('Visual Studio 2022', 'IDE principal para desarrollo .NET.'),
    ('Visual Studio Code', 'Editor de codigo liviano y multiplataforma.'),
    ('.NET Framework 4.8', 'Runtime y herramientas de .NET Framework 4.8.'),
    ('.NET 10 SDK', 'SDK de .NET 10 para desarrollo multiplataforma.'),
    ('SQL Server Developer', 'Motor de base de datos SQL Server edicion Developer.'),
    ('Azure Data Studio', 'Herramienta de administracion y consulta de bases de datos.'),
    ('Node.js LTS', 'Runtime de JavaScript con npm incluido.'),
    ('Docker Desktop', 'Plataforma de contenedores para desarrollo local.'),
    ('Postman', 'Cliente para probar y documentar APIs.'),
    ('GitHub Copilot', 'Licencia de asistente de programacion con IA.'),
    ('JetBrains Rider', 'IDE alternativo para desarrollo .NET.');
GO

INSERT INTO Paquete (Nombre, Descripcion, TipoPermiso, Activo) VALUES
    ('Desarrollador .NET', 'Imagen base para desarrollo backend sobre .NET.', 'Normales', 1),
    ('Desarrollador .NET con privilegios elevados', 'Imagen de desarrollo .NET con permisos de administrador local para tareas de infraestructura.', 'Elevados', 1),
    ('QA Automation', 'Imagen para automatizacion de pruebas y validacion de APIs.', 'Normales', 1),
    ('Business Analyst', 'Imagen para analisis funcional y documentacion, sin herramientas de desarrollo.', 'Minimos', 1),
    ('Desarrollador Frontend React/Angular', 'Imagen para desarrollo frontend con stack de JavaScript.', 'Normales', 1);
GO

INSERT INTO PaquetePrograma (IdPaquete, IdPrograma)
SELECT PA.IdPaquete, PR.IdPrograma
FROM Paquete PA, Programa PR
WHERE (PA.Nombre = 'Desarrollador .NET' AND PR.Nombre IN ('Windows 11 Pro', 'Office 365', 'Google Chrome', 'Git', 'Visual Studio 2022', '.NET Framework 4.8', '.NET 10 SDK', 'SQL Server Developer', 'GitHub Copilot'))
   OR (PA.Nombre = 'Desarrollador .NET con privilegios elevados' AND PR.Nombre IN ('Windows 11 Pro', 'Office 365', 'Google Chrome', 'Git', 'Visual Studio 2022', '.NET Framework 4.8', '.NET 10 SDK', 'SQL Server Developer', 'Azure Data Studio', 'Docker Desktop', 'GitHub Copilot', 'JetBrains Rider'))
   OR (PA.Nombre = 'QA Automation' AND PR.Nombre IN ('Windows 11 Pro', 'Office 365', 'Google Chrome', 'Git', 'Visual Studio Code', 'Node.js LTS', 'Postman', 'Docker Desktop'))
   OR (PA.Nombre = 'Business Analyst' AND PR.Nombre IN ('Windows 11 Pro', 'Office 365', 'Google Chrome'))
   OR (PA.Nombre = 'Desarrollador Frontend React/Angular' AND PR.Nombre IN ('Windows 11 Pro', 'Office 365', 'Google Chrome', 'Git', 'Visual Studio Code', 'Node.js LTS', 'GitHub Copilot'));
GO

INSERT INTO Activo (Nombre, Modelo, NumeroSerie, Especificaciones, IdPaquete, IdCliente, Estado, Habilitado)
SELECT V.Nombre, V.Modelo, V.NumeroSerie, V.Especificaciones, P.IdPaquete, C.IdCliente, V.Estado, 1
FROM (VALUES
    ('Notebook Desarrollo 01', 'Dell Latitude 5540', 'DL5540-AR-0001', 'Intel Core i7-1355U, 32 GB RAM, SSD 1 TB NVMe, pantalla 15.6" FHD', 'Desarrollador .NET', 'Acme Soluciones SRL', 'Asignado'),
    ('Notebook Desarrollo 02', 'Dell Latitude 5540', 'DL5540-AR-0002', 'Intel Core i7-1355U, 32 GB RAM, SSD 1 TB NVMe, pantalla 15.6" FHD', 'Desarrollador .NET', 'Acme Soluciones SRL', 'Disponible'),
    ('Notebook Infra 01', 'Lenovo ThinkPad P16s', 'LTP16S-AR-0007', 'Intel Core i7-1360P, 64 GB RAM, SSD 2 TB NVMe, GPU RTX A500', 'Desarrollador .NET con privilegios elevados', 'Acme Soluciones SRL', 'Asignado'),
    ('Notebook QA 01', 'HP ProBook 450 G10', 'HPPB450-AR-0012', 'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB NVMe', 'QA Automation', 'Acme Soluciones SRL', 'Disponible'),
    ('Notebook Analisis 01', 'Lenovo ThinkPad E14', 'LTE14-AR-0031', 'Intel Core i5-1335U, 16 GB RAM, SSD 512 GB', 'Business Analyst', 'Nordex Logistica SA', 'Asignado'),
    ('Notebook Frontend 01', 'MacBook Air M3', 'MBA-M3-AR-0044', 'Apple M3, 16 GB RAM unificada, SSD 512 GB', 'Desarrollador Frontend React/Angular', 'Nordex Logistica SA', 'EnReparacion')
) AS V (Nombre, Modelo, NumeroSerie, Especificaciones, NombrePaquete, RazonSocial, Estado)
INNER JOIN Paquete P ON P.Nombre = V.NombrePaquete
INNER JOIN Cliente C ON C.RazonSocial = V.RazonSocial;
GO

INSERT INTO Incidente (NumeroIncidente, IdActivo, Descripcion, Categoria, Prioridad, Estado, FechaAlta, FechaCierre, ComentarioResolucion)
SELECT V.NumeroIncidente, A.IdActivo, V.Descripcion, V.Categoria, V.Prioridad, V.Estado, V.FechaAlta, V.FechaCierre, V.ComentarioResolucion
FROM (VALUES
    ('INC-2026-00001', 'DL5540-AR-0001', 'La notebook no reconoce el docking station y se desconecta la red cableada de forma intermitente.', 'Hardware', 'Alta', 'Abierto', GETDATE(), NULL, NULL),
    ('INC-2026-00002', 'LTP16S-AR-0007', 'Visual Studio no abre despues de la ultima actualizacion del paquete.', 'Software', 'Media', 'Cerrado', GETDATE(), GETDATE(), 'Se reinstalo Visual Studio 2022 y se valido con el usuario.')
) AS V (NumeroIncidente, NumeroSerie, Descripcion, Categoria, Prioridad, Estado, FechaAlta, FechaCierre, ComentarioResolucion)
INNER JOIN Activo A ON A.NumeroSerie = V.NumeroSerie;
GO

-- Los Stored Procedures de backup/restore viven en master, no en OperativDb: RESTORE DATABASE
-- necesita acceso exclusivo a OperativDb, y la sesion que lo ejecuta no puede estar conectada
-- a la base que esta restaurando. Ver Plan_Parche_2.1_Operativ.md seccion 1.1.
USE master;
GO

IF OBJECT_ID('dbo.uspBackupOperativDb', 'P') IS NOT NULL
    DROP PROCEDURE dbo.uspBackupOperativDb;
GO

CREATE PROCEDURE dbo.uspBackupOperativDb
    @RutaArchivo NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BACKUP DATABASE OperativDb TO DISK = @RutaArchivo WITH INIT, STATS = 10;
END
GO

IF OBJECT_ID('dbo.uspRestoreOperativDb', 'P') IS NOT NULL
    DROP PROCEDURE dbo.uspRestoreOperativDb;
GO

CREATE PROCEDURE dbo.uspRestoreOperativDb
    @RutaArchivo NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    ALTER DATABASE OperativDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

    BEGIN TRY
        RESTORE DATABASE OperativDb FROM DISK = @RutaArchivo WITH REPLACE, STATS = 10;
    END TRY
    BEGIN CATCH
        ALTER DATABASE OperativDb SET MULTI_USER;
        THROW;
    END CATCH

    ALTER DATABASE OperativDb SET MULTI_USER;
END
GO

USE OperativDb;
GO
