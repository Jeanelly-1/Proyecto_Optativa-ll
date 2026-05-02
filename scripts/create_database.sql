-- ============================================
-- Script: Creación de BD apidelimarket
-- Tablas de seguridad + datos semilla
-- ============================================

-- 1. Crear la base de datos
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'apidelimarket')
BEGIN
    CREATE DATABASE apidelimarket;
END
GO

USE apidelimarket;
GO

-- ============================================
-- 2. Crear tablas
-- ============================================

-- Security_Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Security_Users')
BEGIN
    CREATE TABLE Security_Users (
        Id_User                  INT IDENTITY(1,1) PRIMARY KEY,
        Nombres                  NVARCHAR(150)  NOT NULL,
        Estado                   NVARCHAR(20)   NOT NULL DEFAULT 'Activo',
        Descripcion              NVARCHAR(255)  NULL,
        Password                 NVARCHAR(255)  NOT NULL,
        Mail                     NVARCHAR(150)  NULL,
        Token                    NVARCHAR(255)  NULL,
        Token_Date               DATETIME       NULL,
        Token_Expiration_Date    DATETIME       NULL,
        Password_Expiration_Date DATETIME       NULL
    );
END
GO

-- Security_Roles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Security_Roles')
BEGIN
    CREATE TABLE Security_Roles (
        Id_Role      INT IDENTITY(1,1) PRIMARY KEY,
        Descripcion  NVARCHAR(100) NOT NULL,
        Estado       NVARCHAR(20)  NOT NULL DEFAULT 'Activo'
    );
END
GO

-- Security_Permissions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Security_Permissions')
BEGIN
    CREATE TABLE Security_Permissions (
        Id_Permission INT IDENTITY(1,1) PRIMARY KEY,
        Descripcion   NVARCHAR(100) NOT NULL,
        Detalles      NVARCHAR(255) NULL,
        Estado        NVARCHAR(20)  NOT NULL DEFAULT 'Activo'
    );
END
GO

-- Security_Users_Roles (tabla puente usuario <-> rol)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Security_Users_Roles')
BEGIN
    CREATE TABLE Security_Users_Roles (
        Id_User  INT NOT NULL,
        Id_Role  INT NOT NULL,
        Estado   NVARCHAR(20) NOT NULL DEFAULT 'Activo',
        PRIMARY KEY (Id_User, Id_Role),
        FOREIGN KEY (Id_User) REFERENCES Security_Users(Id_User),
        FOREIGN KEY (Id_Role) REFERENCES Security_Roles(Id_Role)
    );
END
GO

-- Security_Permissions_Roles (tabla puente rol <-> permiso)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Security_Permissions_Roles')
BEGIN
    CREATE TABLE Security_Permissions_Roles (
        Id_Role       INT NOT NULL,
        Id_Permission INT NOT NULL,
        Estado        NVARCHAR(20) NOT NULL DEFAULT 'Activo',
        PRIMARY KEY (Id_Role, Id_Permission),
        FOREIGN KEY (Id_Role)       REFERENCES Security_Roles(Id_Role),
        FOREIGN KEY (Id_Permission) REFERENCES Security_Permissions(Id_Permission)
    );
END
GO

-- Tbl_Profile
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tbl_Profile')
BEGIN
    CREATE TABLE Tbl_Profile (
        Id_Perfil            INT IDENTITY(1,1) PRIMARY KEY,
        Nombres              NVARCHAR(150) NULL,
        Apellidos            NVARCHAR(150) NULL,
        FechaNac             DATETIME      NULL,
        IdUser               INT           NOT NULL,
        Sexo                 NVARCHAR(10)  NULL,
        Foto                 NVARCHAR(500) NULL,
        DNI                  NVARCHAR(20)  NULL,
        Correo_institucional NVARCHAR(150) NULL,
        Estado               NVARCHAR(20)  NOT NULL DEFAULT 'Activo',
        FOREIGN KEY (IdUser) REFERENCES Security_Users(Id_User)
    );
END
GO

-- ============================================
-- 3. Datos semilla (seed data)
-- ============================================

-- 3.1 Permisos (coinciden con el enum Permissions del proyecto C#)
IF NOT EXISTS (SELECT 1 FROM Security_Permissions)
BEGIN
    INSERT INTO Security_Permissions (Descripcion, Detalles, Estado) VALUES
        ('CanViewUsers',      'Permite ver la lista de usuarios',       'Activo'),
        ('CanEditUsers',      'Permite crear y editar usuarios',        'Activo'),
        ('CanDeleteUsers',    'Permite eliminar usuarios',              'Activo'),
        ('CanViewReports',    'Permite ver reportes',                   'Activo'),
        ('CanGenerateReports','Permite generar reportes',               'Activo');
END
GO

-- 3.2 Roles
IF NOT EXISTS (SELECT 1 FROM Security_Roles)
BEGIN
    INSERT INTO Security_Roles (Descripcion, Estado) VALUES
        ('Administrador', 'Activo'),
        ('Usuario',       'Activo');
END
GO

-- 3.3 Asignar TODOS los permisos al rol Administrador (Id_Role = 1)
IF NOT EXISTS (SELECT 1 FROM Security_Permissions_Roles)
BEGIN
    INSERT INTO Security_Permissions_Roles (Id_Role, Id_Permission, Estado)
    SELECT 1, Id_Permission, 'Activo' FROM Security_Permissions;

    -- Al rol Usuario solo permisos de lectura (CanViewUsers, CanViewReports)
    INSERT INTO Security_Permissions_Roles (Id_Role, Id_Permission, Estado)
    SELECT 2, Id_Permission, 'Activo' FROM Security_Permissions
    WHERE Descripcion IN ('CanViewUsers', 'CanViewReports');
END
GO

-- 3.4 Usuario administrador por defecto
--     Nombre de usuario: admin
--     Contraseña:        admin123 (almacenada como hash SHA256)
IF NOT EXISTS (SELECT 1 FROM Security_Users)
BEGIN
    INSERT INTO Security_Users (Nombres, Estado, Descripcion, Password, Mail)
    VALUES (
        'admin',
        'Activo',
        'Usuario administrador del sistema',
        -- SHA256 de 'admin123' en mayúsculas hexadecimales
        UPPER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', N'admin123'), 2)),
        'admin@apidelimarket.com'
    );

    -- Usuario de prueba normal
    INSERT INTO Security_Users (Nombres, Estado, Descripcion, Password, Mail)
    VALUES (
        'testuser',
        'Activo',
        'Usuario de prueba con permisos limitados',
        UPPER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', N'testpassword'), 2)),
        'test@apidelimarket.com'
    );
END
GO

-- 3.5 Asignar roles a usuarios
IF NOT EXISTS (SELECT 1 FROM Security_Users_Roles)
BEGIN
    -- admin (Id_User=1) -> Administrador (Id_Role=1)
    INSERT INTO Security_Users_Roles (Id_User, Id_Role, Estado) VALUES (1, 1, 'Activo');
    -- testuser (Id_User=2) -> Usuario (Id_Role=2)
    INSERT INTO Security_Users_Roles (Id_User, Id_Role, Estado) VALUES (2, 2, 'Activo');
END
GO

-- 3.6 Perfiles
IF NOT EXISTS (SELECT 1 FROM Tbl_Profile)
BEGIN
    INSERT INTO Tbl_Profile (Nombres, Apellidos, IdUser, Estado) VALUES
        ('Admin',    'Sistema',  1, 'Activo'),
        ('Test',     'User',     2, 'Activo');
END
GO

PRINT '✅ Base de datos apidelimarket creada exitosamente con datos semilla.';
PRINT '   - Usuario admin:    admin / admin123';
PRINT '   - Usuario prueba:   testuser / testpassword';
GO
