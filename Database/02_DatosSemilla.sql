/* ==============================================================================
   Operativ - Script 02: Datos semilla (cat�logos, patentes, familias, usuario
   inicial Web Master)
   ============================================================================== */
USE Operativ;
GO

/* ---------------- Cat�logos ---------------- */
INSERT INTO dbo.Perfil (Codigo, Nombre) VALUES
    ('WEBMASTER', N'Web Master'),
    ('ADMINISTRADOR', N'Administrador'),
    ('COMERCIAL', N'Comercial'),
    ('CLIENTE', N'Cliente');
GO

INSERT INTO dbo.Criticidad (Codigo, Nombre) VALUES
    ('INFORMATIVA', N'Informativa'),
    ('ADVERTENCIA', N'Advertencia'),
    ('GRAVE', N'Grave'),
    ('CRITICA', N'Cr�tica');
GO

INSERT INTO dbo.EstadoSuscripcion (Codigo, Nombre) VALUES
    ('ACTIVA', N'Activa'),
    ('VENCIDA', N'Vencida'),
    ('CANCELADA', N'Cancelada'),
    ('PENDIENTE_PAGO', N'Pendiente de pago');
GO

INSERT INTO dbo.MedioPago (Codigo, Nombre) VALUES
    ('TARJETA', N'Tarjeta de cr�dito/d�bito'),
    ('TRANSFERENCIA', N'Transferencia bancaria'),
    ('EFECTIVO', N'Efectivo');
GO

INSERT INTO dbo.CategoriaIncidente (Codigo, Nombre) VALUES
    ('HARDWARE', N'Hardware'),
    ('SOFTWARE', N'Software'),
    ('RED', N'Red'),
    ('SEGURIDAD', N'Seguridad'),
    ('OTRO', N'Otro');
GO

/* ---------------- Patentes (permisos at�micos por m�dulo) ---------------- */
INSERT INTO dbo.Patente (Codigo, Nombre, Modulo) VALUES
    ('USUARIO_LISTAR',        N'Listar usuarios',            'Usuarios'),
    ('USUARIO_ALTA',          N'Alta de usuario',             'Usuarios'),
    ('USUARIO_MODIFICAR',     N'Modificar usuario',           'Usuarios'),
    ('USUARIO_BAJA',          N'Baja de usuario',             'Usuarios'),
    ('FAMILIA_ABM',           N'ABM de familias',             'Familias'),
    ('PATENTE_ASIGNAR',       N'Asignar/remover patentes',    'Familias'),
    ('BITACORA_CONSULTAR',    N'Consultar bit�cora',          'Bitacora'),
    ('BASEDATOS_REPARAR',     N'Reparar base de datos',       'Sistema'),
    ('BASEDATOS_BACKUP',      N'Backup / Restore',            'Sistema'),
    ('CLIENTE_ABM',           N'ABM de clientes',             'Clientes'),
    ('PAQUETE_ABM',           N'ABM de paquetes',             'Paquetes'),
    ('SUSCRIPCION_ABM',       N'Alta/baja de suscripci�n',    'Suscripciones'),
    ('SUSCRIPCION_PAGAR',     N'Registrar pago de suscripci�n','Suscripciones'),
    ('FACTURA_CONSULTAR',     N'Consultar facturas',          'Facturas'),
    ('ACTIVO_ABM',            N'ABM de activos',              'Activos'),
    ('INCIDENTE_ALTA',        N'Alta de incidente',           'Incidentes'),
    ('INCIDENTE_CONSULTAR',   N'Consulta de incidentes',      'Incidentes'),
    ('MONITOREO_DASHBOARD',   N'Dashboard de monitoreo',      'Monitoreo');
GO

/* ---------------- Familias (agrupan patentes - patron Composite) -------- */
INSERT INTO dbo.Familia (Nombre, Descripcion) VALUES
    (N'Administraci�n de Usuarios', N'ABM de usuarios, familias y patentes'),
    (N'Operaci�n Comercial', N'Clientes, paquetes, suscripciones, pagos y facturas'),
    (N'Soporte T�cnico', N'Activos e incidentes'),
    (N'Auditor�a y Sistema', N'Bit�cora, reparaci�n e integridad de base de datos');
GO

INSERT INTO dbo.FamiliaPatente (IdFamilia, IdPatente)
SELECT f.IdFamilia, p.IdPatente
FROM dbo.Familia f
CROSS JOIN dbo.Patente p
WHERE (f.Nombre = N'Administraci�n de Usuarios' AND p.Codigo IN ('USUARIO_LISTAR','USUARIO_ALTA','USUARIO_MODIFICAR','USUARIO_BAJA','FAMILIA_ABM','PATENTE_ASIGNAR'))
   OR (f.Nombre = N'Operaci�n Comercial' AND p.Codigo IN ('CLIENTE_ABM','PAQUETE_ABM','SUSCRIPCION_ABM','SUSCRIPCION_PAGAR','FACTURA_CONSULTAR'))
   OR (f.Nombre = N'Soporte T�cnico' AND p.Codigo IN ('ACTIVO_ABM','INCIDENTE_ALTA','INCIDENTE_CONSULTAR','MONITOREO_DASHBOARD'))
   OR (f.Nombre = N'Auditor�a y Sistema' AND p.Codigo IN ('BITACORA_CONSULTAR','BASEDATOS_REPARAR','BASEDATOS_BACKUP'));
GO

/* ---------------- Usuario inicial Web Master ----------------------------
   Usuario:  webmaster
   Clave:    Admin123!  (DEBE cambiarse en el primer login - ClaveTemporal=1)
   El salt se genera con CRYPT_GEN_RANDOM y el hash con SHA2_256 sobre
   (salt + clave), replicando el algoritmo de Operativ.SEC.AyudanteHash. */
DECLARE @Salt VARBINARY(32) = CRYPT_GEN_RANDOM(32);
DECLARE @Clave NVARCHAR(50) = N'Admin123!';
DECLARE @Hash VARBINARY(64) = HASHBYTES('SHA2_256', @Salt + CONVERT(VARBINARY(200), @Clave));
DECLARE @IdPerfilWebMaster INT = (SELECT IdPerfil FROM dbo.Perfil WHERE Codigo = 'WEBMASTER');

INSERT INTO dbo.Usuario (NombreUsuario, NombreCompleto, CorreoElectronico, ClaveHash, ClaveSalt, IdPerfil, ClaveTemporal, IdiomaPreferido)
VALUES ('webmaster', N'Administrador del Sistema', 'webmaster@operativ.local', @Hash, @Salt, @IdPerfilWebMaster, 1, 'es');

INSERT INTO dbo.UsuarioFamilia (IdUsuario, IdFamilia)
SELECT (SELECT IdUsuario FROM dbo.Usuario WHERE NombreUsuario = 'webmaster'), IdFamilia
FROM dbo.Familia;
GO

/* ---------------- Paquetes de ejemplo ---------------- */
INSERT INTO dbo.Paquete (Nombre, Descripcion, PrecioBase, CantidadActivosIncluidos) VALUES
    (N'B�sico', N'Monitoreo b�sico de hasta 5 activos', 15000.00, 5),
    (N'Profesional', N'Monitoreo avanzado de hasta 20 activos con soporte prioritario', 45000.00, 20),
    (N'Enterprise', N'Monitoreo ilimitado con SLA dedicado', 120000.00, 100);
GO

PRINT 'Script 02 ejecutado: datos semilla insertados correctamente.';
GO
