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
    Activo BIT NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1),
    DVH BIGINT NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuario_NombreUsuario UNIQUE (NombreUsuario)
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

CREATE TABLE Bitacora
(
    IdBitacora INT IDENTITY(1,1) NOT NULL,
    IdUsuario INT NOT NULL,
    FechaHora DATETIME NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT (GETDATE()),
    Accion VARCHAR(50) NOT NULL,
    Criticidad VARCHAR(20) NOT NULL,
    Descripcion VARCHAR(300) NULL,
    EntidadAfectada VARCHAR(50) NULL,
    IdEntidadAfectada INT NULL,
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

INSERT INTO Familia (Nombre, Descripcion) VALUES
    ('WebMaster', 'Mantenimiento tecnico de la plataforma'),
    ('Administrador', 'Gestion de usuarios y permisos'),
    ('Comercial', 'Gestion de clientes y catalogo'),
    ('Cliente', 'Suscripciones, activos, incidentes y facturas');
GO

INSERT INTO Patente (Nombre, Descripcion) VALUES
    ('RepararBaseDatos', 'Ejecutar el modulo de reparacion de base de datos'),
    ('RealizarBackup', 'Realizar backup y restore de la base de datos'),
    ('GestionarUsuarios', 'Alta, baja y modificacion de usuarios'),
    ('GestionarFamilias', 'Alta, baja y modificacion de familias y patentes'),
    ('GestionarClientes', 'Alta, baja y modificacion de clientes'),
    ('GestionarCatalogo', 'Administrar el catalogo de paquetes'),
    ('GestionarSuscripciones', 'Contratar y administrar suscripciones'),
    ('ConsultarFacturas', 'Consultar facturas emitidas'),
    ('ReportarIncidentes', 'Reportar incidentes sobre activos');
GO

INSERT INTO FamiliaPatente (IdFamilia, IdPatente)
SELECT F.IdFamilia, P.IdPatente
FROM Familia F, Patente P
WHERE (F.Nombre = 'WebMaster' AND P.Nombre IN ('RepararBaseDatos', 'RealizarBackup'))
   OR (F.Nombre = 'Administrador' AND P.Nombre IN ('GestionarUsuarios', 'GestionarFamilias'))
   OR (F.Nombre = 'Comercial' AND P.Nombre IN ('GestionarClientes', 'GestionarCatalogo'))
   OR (F.Nombre = 'Cliente' AND P.Nombre IN ('GestionarSuscripciones', 'ConsultarFacturas', 'ReportarIncidentes'));
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
