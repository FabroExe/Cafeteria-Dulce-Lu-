-- Opcional. Datos ficticios para probar los mantenedores.
USE [DulceLu];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = N'CAF-001')
    EXEC dbo.spInsertarProducto N'CAF-001', N'Latte de vainilla', N'Cafés', 11.50, 24;
IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = N'POS-001')
    EXEC dbo.spInsertarProducto N'POS-001', N'Torta de chocolate', N'Pastelería', 14.00, 8;
IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = N'PAN-001')
    EXEC dbo.spInsertarProducto N'PAN-001', N'Croissant de almendras', N'Panadería', 9.50, 12;

IF NOT EXISTS (SELECT 1 FROM dbo.Mesa WHERE Codigo = N'M-01')
    EXEC dbo.spInsertarMesa N'M-01', N'Salón', 2;
IF NOT EXISTS (SELECT 1 FROM dbo.Mesa WHERE Codigo = N'M-02')
    EXEC dbo.spInsertarMesa N'M-02', N'Ventanal', 4;
IF NOT EXISTS (SELECT 1 FROM dbo.Mesa WHERE Codigo = N'M-03')
    EXEC dbo.spInsertarMesa N'M-03', N'Terraza', 6;

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE Documento = N'DEMO-001')
    EXEC dbo.spInsertarCliente N'Mariana Ríos', N'DEMO-001', NULL, N'mariana@example.com';
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE Documento = N'DEMO-002')
    EXEC dbo.spInsertarCliente N'Álvaro Paredes', N'DEMO-002', NULL, N'alvaro@example.com';

IF NOT EXISTS (SELECT 1 FROM dbo.MetodoPago WHERE Nombre = N'Efectivo')
    EXEC dbo.spInsertarMetodoPago N'Efectivo', N'Soles peruanos.';
IF NOT EXISTS (SELECT 1 FROM dbo.MetodoPago WHERE Nombre = N'Tarjeta')
    EXEC dbo.spInsertarMetodoPago N'Tarjeta', N'Tarjeta de crédito o débito.';
IF NOT EXISTS (SELECT 1 FROM dbo.MetodoPago WHERE Nombre = N'Yape')
    EXEC dbo.spInsertarMetodoPago N'Yape', N'Pago por QR.';
COMMIT TRANSACTION;
GO
