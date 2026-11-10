-- Dulce Lu | Ejecutar sobre SQL Server 2016 SP1 o posterior.
-- Repetir este script conserva los datos y actualiza los procedimientos de la versión 1.
USE [master];
GO
IF DB_ID(N'DulceLu') IS NULL
BEGIN TRY
    CREATE DATABASE [DulceLu];
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() <> 1801 THROW;
END CATCH;
GO
USE [DulceLu];
GO
IF DB_NAME() <> N'DulceLu'
    THROW 50003, N'Selecciona la base DulceLu antes de instalar el esquema.', 1;

SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Bloqueo int;
    EXEC @Bloqueo = sys.sp_getapplock
        @Resource = N'DulceLu.Esquema', @LockMode = 'Exclusive', @LockTimeout = 10000;
    IF @Bloqueo < 0
        THROW 50002, N'No se pudo reservar la actualización de Dulce Lu.', 1;

    IF OBJECT_ID(N'dbo.Configuracion', N'U') IS NULL
        CREATE TABLE dbo.Configuracion (
            Id int PRIMARY KEY CHECK (Id = 1),
            VersionEsquema int NOT NULL
        );
    IF EXISTS (SELECT 1 FROM dbo.Configuracion WHERE VersionEsquema <> 1)
        THROW 50003, N'La versión de esquema existente requiere otra versión de Dulce Lu.', 1;

    IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Producto (
            Id int IDENTITY(1,1) CONSTRAINT PK_Producto PRIMARY KEY,
            Codigo nvarchar(24) NOT NULL,
            Nombre nvarchar(100) NOT NULL,
            Categoria nvarchar(50) NOT NULL,
            Precio decimal(10,2) NOT NULL CHECK (Precio > 0),
            Stock int NOT NULL CHECK (Stock >= 0),
            Activo bit NOT NULL CONSTRAINT DF_Producto_Activo DEFAULT (1),
            Version rowversion NOT NULL
        );
        CREATE UNIQUE INDEX UX_Producto_Codigo ON dbo.Producto (Codigo);
    END;

    IF OBJECT_ID(N'dbo.Mesa', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Mesa (
            Id int IDENTITY(1,1) CONSTRAINT PK_Mesa PRIMARY KEY,
            Codigo nvarchar(16) NOT NULL,
            Zona nvarchar(50) NOT NULL,
            Capacidad int NOT NULL CHECK (Capacidad BETWEEN 1 AND 30),
            Activo bit NOT NULL CONSTRAINT DF_Mesa_Activo DEFAULT (1),
            Version rowversion NOT NULL
        );
        CREATE UNIQUE INDEX UX_Mesa_Codigo ON dbo.Mesa (Codigo);
    END;

    IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Cliente (
            Id int IDENTITY(1,1) CONSTRAINT PK_Cliente PRIMARY KEY,
            Nombre nvarchar(100) NOT NULL,
            Documento nvarchar(16) NULL,
            Telefono nvarchar(24) NULL,
            Correo nvarchar(120) NULL,
            Activo bit NOT NULL CONSTRAINT DF_Cliente_Activo DEFAULT (1),
            Version rowversion NOT NULL
        );
        CREATE UNIQUE INDEX UX_Cliente_Documento ON dbo.Cliente (Documento) WHERE Documento IS NOT NULL;
    END;

    IF OBJECT_ID(N'dbo.MetodoPago', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.MetodoPago (
            Id int IDENTITY(1,1) CONSTRAINT PK_MetodoPago PRIMARY KEY,
            Nombre nvarchar(50) NOT NULL,
            Descripcion nvarchar(160) NULL,
            Activo bit NOT NULL CONSTRAINT DF_MetodoPago_Activo DEFAULT (1),
            Version rowversion NOT NULL
        );
        CREATE UNIQUE INDEX UX_MetodoPago_Nombre ON dbo.MetodoPago (Nombre);
    END;

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spListarProducto
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Codigo, Nombre, Categoria, Precio, Stock, Activo, Version
        FROM dbo.Producto
        ORDER BY Nombre, Codigo;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spInsertarProducto
        @Codigo nvarchar(24),
        @Nombre nvarchar(100),
        @Categoria nvarchar(50),
        @Precio decimal(10,2),
        @Stock int
    AS
    BEGIN
        SET NOCOUNT ON;
        INSERT INTO dbo.Producto (Codigo, Nombre, Categoria, Precio, Stock)
        VALUES (@Codigo, @Nombre, @Categoria, @Precio, @Stock);
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spActualizarProducto
        @Id int,
        @Version binary(8),
        @Codigo nvarchar(24),
        @Nombre nvarchar(100),
        @Categoria nvarchar(50),
        @Precio decimal(10,2),
        @Stock int
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.Producto
        SET Codigo = @Codigo,
            Nombre = @Nombre,
            Categoria = @Categoria,
            Precio = @Precio,
            Stock = @Stock
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spCambiarEstadoProducto
        @Id int,
        @Version binary(8),
        @Activo bit
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.Producto
        SET Activo = @Activo
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spListarMesa
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Codigo, Zona, Capacidad, Activo, Version
        FROM dbo.Mesa
        ORDER BY Codigo;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spInsertarMesa
        @Codigo nvarchar(16),
        @Zona nvarchar(50),
        @Capacidad int
    AS
    BEGIN
        SET NOCOUNT ON;
        INSERT INTO dbo.Mesa (Codigo, Zona, Capacidad)
        VALUES (@Codigo, @Zona, @Capacidad);
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spActualizarMesa
        @Id int,
        @Version binary(8),
        @Codigo nvarchar(16),
        @Zona nvarchar(50),
        @Capacidad int
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.Mesa
        SET Codigo = @Codigo,
            Zona = @Zona,
            Capacidad = @Capacidad
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spCambiarEstadoMesa
        @Id int,
        @Version binary(8),
        @Activo bit
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.Mesa
        SET Activo = @Activo
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spListarCliente
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Nombre, Documento, Telefono, Correo, Activo, Version
        FROM dbo.Cliente
        ORDER BY Nombre, Id;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spInsertarCliente
        @Nombre nvarchar(100),
        @Documento nvarchar(16),
        @Telefono nvarchar(24),
        @Correo nvarchar(120)
    AS
    BEGIN
        SET NOCOUNT ON;
        INSERT INTO dbo.Cliente (Nombre, Documento, Telefono, Correo)
        VALUES (@Nombre, @Documento, @Telefono, @Correo);
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spActualizarCliente
        @Id int,
        @Version binary(8),
        @Nombre nvarchar(100),
        @Documento nvarchar(16),
        @Telefono nvarchar(24),
        @Correo nvarchar(120)
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.Cliente
        SET Nombre = @Nombre,
            Documento = @Documento,
            Telefono = @Telefono,
            Correo = @Correo
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spCambiarEstadoCliente
        @Id int,
        @Version binary(8),
        @Activo bit
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.Cliente
        SET Activo = @Activo
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spListarMetodoPago
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Nombre, Descripcion, Activo, Version
        FROM dbo.MetodoPago
        ORDER BY Nombre;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spInsertarMetodoPago
        @Nombre nvarchar(50),
        @Descripcion nvarchar(160)
    AS
    BEGIN
        SET NOCOUNT ON;
        INSERT INTO dbo.MetodoPago (Nombre, Descripcion)
        VALUES (@Nombre, @Descripcion);
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spActualizarMetodoPago
        @Id int,
        @Version binary(8),
        @Nombre nvarchar(50),
        @Descripcion nvarchar(160)
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.MetodoPago
        SET Nombre = @Nombre,
            Descripcion = @Descripcion
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    EXEC sys.sp_executesql N'
    CREATE OR ALTER PROCEDURE dbo.spCambiarEstadoMetodoPago
        @Id int,
        @Version binary(8),
        @Activo bit
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.MetodoPago
        SET Activo = @Activo
        WHERE Id = @Id AND Version = @Version;
        IF @@ROWCOUNT = 0
            THROW 50001, N''El registro ya cambió.'', 1;
    END;
    ';

    IF NOT EXISTS (SELECT 1 FROM dbo.Configuracion WHERE Id = 1)
        INSERT INTO dbo.Configuracion (Id, VersionEsquema) VALUES (1, 1);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
