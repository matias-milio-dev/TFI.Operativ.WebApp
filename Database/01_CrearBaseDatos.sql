/* ==============================================================================
   Operativ - Plataforma de Gesti�n Operativa
   Script 01: Creaci�n de base de datos, tablas, claves y �ndices
   Motor: Microsoft SQL Server (Express / Developer)
   ==============================================================================
   Convenci�n de nombres en espa�ol seg�n Requerimientos T�cnicos punto 2.
   Cada tabla cr�tica incluye columna DVH (d�gito verificador horizontal).
   El DVV (d�gito verificador vertical) se calcula por tabla y se guarda
   en Operativ.dbo.DigitoVerificadorTabla (script 03).
   ============================================================================== */

USE master;
GO

IF DB_ID('Operativ') IS NULL
BEGIN
    CREATE DATABASE Operativ;
END
GO

ALTER DATABASE Operativ SET RECOVERY SIMPLE;
GO

USE Operativ;
GO

/* ------------------------------------------------------------------
   Catalogos / tablas de apoyo (no forman parte del listado minimo del
   documento pero se agregan para normalizar segun el punto 4 del
   documento de requerimientos, que habilita catalogos auxiliares).
   ------------------------------------------------------------------ */

IF OBJECT_ID('dbo.Perfil', 'U') IS NOT NULL DROP TABLE dbo.Perfil;
GO
CREATE TABLE dbo.Perfil
(
    IdPerfil        INT IDENTITY(1,1)   NOT NULL,
    Codigo          VARCHAR(20)         NOT NULL,   -- WEBMASTER, ADMINISTRADOR, COMERCIAL, CLIENTE
    Nombre          NVARCHAR(50)        NOT NULL,
    CONSTRAINT PK_Perfil PRIMARY KEY CLUSTERED (IdPerfil),
    CONSTRAINT UQ_Perfil_Codigo UNIQUE (Codigo)
);
GO

IF OBJECT_ID('dbo.EstadoSuscripcion', 'U') IS NOT NULL DROP TABLE dbo.EstadoSuscripcion;
GO
CREATE TABLE dbo.EstadoSuscripcion
(
    IdEstadoSuscripcion INT IDENTITY(1,1) NOT NULL,
    Codigo              VARCHAR(20)       NOT NULL,  -- ACTIVA, VENCIDA, CANCELADA, PENDIENTE_PAGO
    Nombre              NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_EstadoSuscripcion PRIMARY KEY CLUSTERED (IdEstadoSuscripcion),
    CONSTRAINT UQ_EstadoSuscripcion_Codigo UNIQUE (Codigo)
);
GO

IF OBJECT_ID('dbo.MedioPago', 'U') IS NOT NULL DROP TABLE dbo.MedioPago;
GO
CREATE TABLE dbo.MedioPago
(
    IdMedioPago     INT IDENTITY(1,1)   NOT NULL,
    Codigo          VARCHAR(20)         NOT NULL,   -- TARJETA, TRANSFERENCIA, EFECTIVO
    Nombre          NVARCHAR(50)        NOT NULL,
    CONSTRAINT PK_MedioPago PRIMARY KEY CLUSTERED (IdMedioPago),
    CONSTRAINT UQ_MedioPago_Codigo UNIQUE (Codigo)
);
GO

IF OBJECT_ID('dbo.CategoriaIncidente', 'U') IS NOT NULL DROP TABLE dbo.CategoriaIncidente;
GO
CREATE TABLE dbo.CategoriaIncidente
(
    IdCategoriaIncidente INT IDENTITY(1,1) NOT NULL,
    Codigo                VARCHAR(20)      NOT NULL,  -- HARDWARE, SOFTWARE, RED, SEGURIDAD, OTRO
    Nombre                NVARCHAR(50)     NOT NULL,
    CONSTRAINT PK_CategoriaIncidente PRIMARY KEY CLUSTERED (IdCategoriaIncidente),
    CONSTRAINT UQ_CategoriaIncidente_Codigo UNIQUE (Codigo)
);
GO

IF OBJECT_ID('dbo.Criticidad', 'U') IS NOT NULL DROP TABLE dbo.Criticidad;
GO
CREATE TABLE dbo.Criticidad
(
    IdCriticidad    INT IDENTITY(1,1)   NOT NULL,
    Codigo          VARCHAR(20)         NOT NULL,   -- INFORMATIVA, ADVERTENCIA, GRAVE, CRITICA
    Nombre          NVARCHAR(50)        NOT NULL,
    CONSTRAINT PK_Criticidad PRIMARY KEY CLUSTERED (IdCriticidad),
    CONSTRAINT UQ_Criticidad_Codigo UNIQUE (Codigo)
);
GO

/* ------------------------------------------------------------------
   Seguridad / Usuarios / Permisos (RBAC composite: Familia agrupa Patentes)
   ------------------------------------------------------------------ */

IF OBJECT_ID('dbo.Usuario', 'U') IS NOT NULL DROP TABLE dbo.Usuario;
GO
CREATE TABLE dbo.Usuario
(
    IdUsuario           INT IDENTITY(1,1)   NOT NULL,
    NombreUsuario        VARCHAR(50)         NOT NULL,
    NombreCompleto        NVARCHAR(150)       NOT NULL,
    CorreoElectronico    VARCHAR(150)        NOT NULL,
    ClaveHash            VARBINARY(64)       NOT NULL,   -- SHA-256 (32 bytes) sobre clave+salt
    ClaveSalt             VARBINARY(32)       NOT NULL,   -- salt unico por usuario
    IdPerfil             INT                 NOT NULL,
    CantidadIntentosFallidos TINYINT         NOT NULL CONSTRAINT DF_Usuario_Intentos DEFAULT (0),
    Bloqueado             BIT                 NOT NULL CONSTRAINT DF_Usuario_Bloqueado DEFAULT (0),
    ClaveTemporal          BIT                 NOT NULL CONSTRAINT DF_Usuario_ClaveTemp DEFAULT (0),
    Activo                 BIT                 NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1),
    IdiomaPreferido        VARCHAR(5)          NOT NULL CONSTRAINT DF_Usuario_Idioma DEFAULT ('es'),
    FechaCreacion          DATETIME2           NOT NULL CONSTRAINT DF_Usuario_FechaCreacion DEFAULT (SYSDATETIME()),
    FechaUltimoLogin       DATETIME2           NULL,
    DVH                    VARBINARY(32)       NULL,
    CONSTRAINT PK_Usuario PRIMARY KEY CLUSTERED (IdUsuario),
    CONSTRAINT UQ_Usuario_NombreUsuario UNIQUE (NombreUsuario),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (CorreoElectronico),
    CONSTRAINT FK_Usuario_Perfil FOREIGN KEY (IdPerfil) REFERENCES dbo.Perfil (IdPerfil)
);
GO

IF OBJECT_ID('dbo.Familia', 'U') IS NOT NULL DROP TABLE dbo.Familia;
GO
CREATE TABLE dbo.Familia
(
    IdFamilia       INT IDENTITY(1,1)   NOT NULL,
    Nombre          NVARCHAR(100)       NOT NULL,
    Descripcion     NVARCHAR(300)       NULL,
    Activo          BIT                 NOT NULL CONSTRAINT DF_Familia_Activo DEFAULT (1),
    DVH             VARBINARY(32)       NULL,
    CONSTRAINT PK_Familia PRIMARY KEY CLUSTERED (IdFamilia),
    CONSTRAINT UQ_Familia_Nombre UNIQUE (Nombre)
);
GO

IF OBJECT_ID('dbo.Patente', 'U') IS NOT NULL DROP TABLE dbo.Patente;
GO
CREATE TABLE dbo.Patente
(
    IdPatente       INT IDENTITY(1,1)   NOT NULL,
    Codigo          VARCHAR(50)         NOT NULL,   -- ej: USUARIO_ALTA, BITACORA_CONSULTAR
    Nombre          NVARCHAR(100)       NOT NULL,
    Descripcion     NVARCHAR(300)       NULL,
    Modulo          NVARCHAR(50)        NOT NULL,
    Activo          BIT                 NOT NULL CONSTRAINT DF_Patente_Activo DEFAULT (1),
    DVH             VARBINARY(32)       NULL,
    CONSTRAINT PK_Patente PRIMARY KEY CLUSTERED (IdPatente),
    CONSTRAINT UQ_Patente_Codigo UNIQUE (Codigo)
);
GO

IF OBJECT_ID('dbo.UsuarioFamilia', 'U') IS NOT NULL DROP TABLE dbo.UsuarioFamilia;
GO
CREATE TABLE dbo.UsuarioFamilia
(
    IdUsuarioFamilia INT IDENTITY(1,1)  NOT NULL,
    IdUsuario         INT                NOT NULL,
    IdFamilia         INT                NOT NULL,
    FechaAsignacion   DATETIME2          NOT NULL CONSTRAINT DF_UsuarioFamilia_Fecha DEFAULT (SYSDATETIME()),
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_UsuarioFamilia PRIMARY KEY CLUSTERED (IdUsuarioFamilia),
    CONSTRAINT UQ_UsuarioFamilia UNIQUE (IdUsuario, IdFamilia),
    CONSTRAINT FK_UsuarioFamilia_Usuario FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (IdUsuario),
    CONSTRAINT FK_UsuarioFamilia_Familia FOREIGN KEY (IdFamilia) REFERENCES dbo.Familia (IdFamilia)
);
GO

IF OBJECT_ID('dbo.FamiliaPatente', 'U') IS NOT NULL DROP TABLE dbo.FamiliaPatente;
GO
CREATE TABLE dbo.FamiliaPatente
(
    IdFamiliaPatente INT IDENTITY(1,1)  NOT NULL,
    IdFamilia         INT                NOT NULL,
    IdPatente         INT                NOT NULL,
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_FamiliaPatente PRIMARY KEY CLUSTERED (IdFamiliaPatente),
    CONSTRAINT UQ_FamiliaPatente UNIQUE (IdFamilia, IdPatente),
    CONSTRAINT FK_FamiliaPatente_Familia FOREIGN KEY (IdFamilia) REFERENCES dbo.Familia (IdFamilia),
    CONSTRAINT FK_FamiliaPatente_Patente FOREIGN KEY (IdPatente) REFERENCES dbo.Patente (IdPatente)
);
GO

/* Asignacion directa de patente a usuario (permiso individual), ademas de
   la asignacion via familia -> soporta el CU "Asignar/Remover Patente". */
IF OBJECT_ID('dbo.UsuarioPatente', 'U') IS NOT NULL DROP TABLE dbo.UsuarioPatente;
GO
CREATE TABLE dbo.UsuarioPatente
(
    IdUsuarioPatente INT IDENTITY(1,1)  NOT NULL,
    IdUsuario         INT                NOT NULL,
    IdPatente         INT                NOT NULL,
    FechaAsignacion   DATETIME2          NOT NULL CONSTRAINT DF_UsuarioPatente_Fecha DEFAULT (SYSDATETIME()),
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_UsuarioPatente PRIMARY KEY CLUSTERED (IdUsuarioPatente),
    CONSTRAINT UQ_UsuarioPatente UNIQUE (IdUsuario, IdPatente),
    CONSTRAINT FK_UsuarioPatente_Usuario FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (IdUsuario),
    CONSTRAINT FK_UsuarioPatente_Patente FOREIGN KEY (IdPatente) REFERENCES dbo.Patente (IdPatente)
);
GO

/* ------------------------------------------------------------------
   Bitacora (auditoria inmutable)
   ------------------------------------------------------------------ */

IF OBJECT_ID('dbo.Bitacora', 'U') IS NOT NULL DROP TABLE dbo.Bitacora;
GO
CREATE TABLE dbo.Bitacora
(
    IdBitacora      BIGINT IDENTITY(1,1) NOT NULL,
    IdUsuario        INT                  NULL,       -- NULL permitido: login fallido con usuario inexistente
    FechaHora        DATETIME2            NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT (SYSDATETIME()),
    Accion           VARCHAR(50)          NOT NULL,   -- LOGIN, LOGOUT, ALTA, BAJA, MODIFICACION, BLOQUEO, REPARACION, etc.
    EntidadAfectada  VARCHAR(50)          NOT NULL,   -- nombre de tabla/entidad
    IdEntidadAfectada VARCHAR(50)         NULL,
    Descripcion      NVARCHAR(500)        NULL,
    IdCriticidad     INT                  NOT NULL,
    DireccionIP       VARCHAR(45)          NULL,
    DVH               VARBINARY(32)        NULL,
    CONSTRAINT PK_Bitacora PRIMARY KEY CLUSTERED (IdBitacora),
    CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (IdUsuario),
    CONSTRAINT FK_Bitacora_Criticidad FOREIGN KEY (IdCriticidad) REFERENCES dbo.Criticidad (IdCriticidad)
);
GO
CREATE NONCLUSTERED INDEX IX_Bitacora_FechaHora ON dbo.Bitacora (FechaHora DESC);
CREATE NONCLUSTERED INDEX IX_Bitacora_Usuario ON dbo.Bitacora (IdUsuario);
CREATE NONCLUSTERED INDEX IX_Bitacora_Accion ON dbo.Bitacora (Accion);
GO

/* ------------------------------------------------------------------
   Negocio: Cliente / Paquete / Suscripcion / Pago / Factura / Activo / Incidente
   ------------------------------------------------------------------ */

IF OBJECT_ID('dbo.Cliente', 'U') IS NOT NULL DROP TABLE dbo.Cliente;
GO
CREATE TABLE dbo.Cliente
(
    IdCliente        INT IDENTITY(1,1)  NOT NULL,
    Cuit              VARCHAR(13)        NOT NULL,   -- formato NN-NNNNNNNN-N
    RazonSocial       NVARCHAR(150)      NOT NULL,
    CorreoElectronico VARCHAR(150)       NOT NULL,
    Telefono          VARCHAR(30)        NULL,
    Direccion         NVARCHAR(200)      NULL,
    IdUsuario         INT                NULL,       -- usuario de portal asociado al cliente (perfil Cliente)
    Activo            BIT                NOT NULL CONSTRAINT DF_Cliente_Activo DEFAULT (1),
    FechaAlta         DATETIME2          NOT NULL CONSTRAINT DF_Cliente_FechaAlta DEFAULT (SYSDATETIME()),
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_Cliente PRIMARY KEY CLUSTERED (IdCliente),
    CONSTRAINT UQ_Cliente_Cuit UNIQUE (Cuit),
    CONSTRAINT FK_Cliente_Usuario FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (IdUsuario)
);
GO

IF OBJECT_ID('dbo.Paquete', 'U') IS NOT NULL DROP TABLE dbo.Paquete;
GO
CREATE TABLE dbo.Paquete
(
    IdPaquete        INT IDENTITY(1,1)  NOT NULL,
    Nombre            NVARCHAR(100)      NOT NULL,
    Descripcion       NVARCHAR(400)      NULL,
    PrecioBase        DECIMAL(12,2)      NOT NULL,
    CantidadActivosIncluidos INT         NOT NULL CONSTRAINT DF_Paquete_CantActivos DEFAULT (0),
    Activo            BIT                NOT NULL CONSTRAINT DF_Paquete_Activo DEFAULT (1),
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_Paquete PRIMARY KEY CLUSTERED (IdPaquete),
    CONSTRAINT UQ_Paquete_Nombre UNIQUE (Nombre)
);
GO

IF OBJECT_ID('dbo.Suscripcion', 'U') IS NOT NULL DROP TABLE dbo.Suscripcion;
GO
CREATE TABLE dbo.Suscripcion
(
    IdSuscripcion     INT IDENTITY(1,1) NOT NULL,
    IdCliente          INT               NOT NULL,
    IdPaquete          INT               NOT NULL,
    IdEstadoSuscripcion INT              NOT NULL,
    FechaInicio        DATE              NOT NULL,
    FechaVencimiento    DATE              NOT NULL,
    PrecioAcordado      DECIMAL(12,2)     NOT NULL,   -- resultado de la Strategy de cotizacion
    EstrategiaAplicada   VARCHAR(50)       NULL,       -- nombre de la estrategia usada (trazabilidad)
    FechaAlta            DATETIME2         NOT NULL CONSTRAINT DF_Suscripcion_FechaAlta DEFAULT (SYSDATETIME()),
    DVH                  VARBINARY(32)     NULL,
    CONSTRAINT PK_Suscripcion PRIMARY KEY CLUSTERED (IdSuscripcion),
    CONSTRAINT FK_Suscripcion_Cliente FOREIGN KEY (IdCliente) REFERENCES dbo.Cliente (IdCliente),
    CONSTRAINT FK_Suscripcion_Paquete FOREIGN KEY (IdPaquete) REFERENCES dbo.Paquete (IdPaquete),
    CONSTRAINT FK_Suscripcion_Estado FOREIGN KEY (IdEstadoSuscripcion) REFERENCES dbo.EstadoSuscripcion (IdEstadoSuscripcion)
);
GO
CREATE NONCLUSTERED INDEX IX_Suscripcion_Cliente ON dbo.Suscripcion (IdCliente);
GO

IF OBJECT_ID('dbo.Pago', 'U') IS NOT NULL DROP TABLE dbo.Pago;
GO
CREATE TABLE dbo.Pago
(
    IdPago           INT IDENTITY(1,1)  NOT NULL,
    IdSuscripcion     INT                NOT NULL,
    IdMedioPago       INT                NOT NULL,
    Monto             DECIMAL(12,2)      NOT NULL,
    FechaPago         DATETIME2          NOT NULL CONSTRAINT DF_Pago_Fecha DEFAULT (SYSDATETIME()),
    ReferenciaExterna VARCHAR(100)       NULL,        -- id de pasarela de pago, si aplica
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_Pago PRIMARY KEY CLUSTERED (IdPago),
    CONSTRAINT FK_Pago_Suscripcion FOREIGN KEY (IdSuscripcion) REFERENCES dbo.Suscripcion (IdSuscripcion),
    CONSTRAINT FK_Pago_MedioPago FOREIGN KEY (IdMedioPago) REFERENCES dbo.MedioPago (IdMedioPago)
);
GO

IF OBJECT_ID('dbo.Factura', 'U') IS NOT NULL DROP TABLE dbo.Factura;
GO
CREATE TABLE dbo.Factura
(
    IdFactura        INT IDENTITY(1,1)  NOT NULL,
    IdPago            INT                NOT NULL,
    NumeroFactura     VARCHAR(20)        NOT NULL,   -- 0001-00000001
    FechaEmision      DATETIME2          NOT NULL CONSTRAINT DF_Factura_Fecha DEFAULT (SYSDATETIME()),
    MontoTotal        DECIMAL(12,2)      NOT NULL,
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_Factura PRIMARY KEY CLUSTERED (IdFactura),
    CONSTRAINT UQ_Factura_Numero UNIQUE (NumeroFactura),
    CONSTRAINT FK_Factura_Pago FOREIGN KEY (IdPago) REFERENCES dbo.Pago (IdPago)
);
GO

IF OBJECT_ID('dbo.Activo', 'U') IS NOT NULL DROP TABLE dbo.Activo;
GO
CREATE TABLE dbo.Activo
(
    IdActivo         INT IDENTITY(1,1)  NOT NULL,
    IdCliente         INT                NOT NULL,
    IdSuscripcion     INT                NULL,
    Nombre            NVARCHAR(100)      NOT NULL,
    TipoActivo        VARCHAR(30)        NOT NULL,   -- SERVIDOR, PC, RED, APLICACION, OTRO
    Identificador     VARCHAR(100)       NULL,       -- serie / hostname / IP
    Activo1           BIT                NOT NULL CONSTRAINT DF_Activo_Activo DEFAULT (1),
    FechaAlta         DATETIME2          NOT NULL CONSTRAINT DF_Activo_FechaAlta DEFAULT (SYSDATETIME()),
    DVH               VARBINARY(32)      NULL,
    CONSTRAINT PK_Activo PRIMARY KEY CLUSTERED (IdActivo),
    CONSTRAINT FK_Activo_Cliente FOREIGN KEY (IdCliente) REFERENCES dbo.Cliente (IdCliente),
    CONSTRAINT FK_Activo_Suscripcion FOREIGN KEY (IdSuscripcion) REFERENCES dbo.Suscripcion (IdSuscripcion)
);
GO
CREATE NONCLUSTERED INDEX IX_Activo_Cliente ON dbo.Activo (IdCliente);
GO

IF OBJECT_ID('dbo.Incidente', 'U') IS NOT NULL DROP TABLE dbo.Incidente;
GO
CREATE TABLE dbo.Incidente
(
    IdIncidente       INT IDENTITY(1,1) NOT NULL,
    IdActivo           INT               NOT NULL,
    IdCategoriaIncidente INT             NOT NULL,
    Descripcion         NVARCHAR(500)     NOT NULL,
    Prioridad            VARCHAR(10)       NOT NULL,  -- BAJA, MEDIA, ALTA, URGENTE
    Estado                VARCHAR(20)       NOT NULL CONSTRAINT DF_Incidente_Estado DEFAULT ('ABIERTO'),
    FechaAlta             DATETIME2         NOT NULL CONSTRAINT DF_Incidente_FechaAlta DEFAULT (SYSDATETIME()),
    FechaCierre           DATETIME2         NULL,
    RutaXmlGenerado        VARCHAR(260)      NULL,      -- incidente_ID.xml (IncidentesService)
    DVH                    VARBINARY(32)     NULL,
    CONSTRAINT PK_Incidente PRIMARY KEY CLUSTERED (IdIncidente),
    CONSTRAINT FK_Incidente_Activo FOREIGN KEY (IdActivo) REFERENCES dbo.Activo (IdActivo),
    CONSTRAINT FK_Incidente_Categoria FOREIGN KEY (IdCategoriaIncidente) REFERENCES dbo.CategoriaIncidente (IdCategoriaIncidente)
);
GO
CREATE NONCLUSTERED INDEX IX_Incidente_Activo ON dbo.Incidente (IdActivo);
GO

/* ------------------------------------------------------------------
   Tabla de control de DVV (digito verificador vertical) por tabla,
   soporte al CU-001-002 Reparar Base de Datos (punto 10.8.3).
   ------------------------------------------------------------------ */
IF OBJECT_ID('dbo.DigitoVerificadorTabla', 'U') IS NOT NULL DROP TABLE dbo.DigitoVerificadorTabla;
GO
CREATE TABLE dbo.DigitoVerificadorTabla
(
    IdDigitoVerificadorTabla INT IDENTITY(1,1) NOT NULL,
    NombreTabla                VARCHAR(100)     NOT NULL,
    ValorDVV                   VARBINARY(32)    NOT NULL,
    FechaCalculo                DATETIME2        NOT NULL CONSTRAINT DF_DVT_Fecha DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_DigitoVerificadorTabla PRIMARY KEY CLUSTERED (IdDigitoVerificadorTabla),
    CONSTRAINT UQ_DigitoVerificadorTabla_Tabla UNIQUE (NombreTabla)
);
GO

PRINT 'Script 01 ejecutado: base de datos y tablas creadas correctamente.';
GO
