# Dulce Lu · Gestión de cafetería

Aplicación de escritorio para Windows, en C# y Windows Forms sobre .NET Framework 4.8.

## Abrir la aplicación

1. Extrae todo el ZIP en una carpeta. Abre **Abrir Dulce Lu.cmd** o ejecuta **Aplicacion/DulceLu.exe**.
2. En **Conexión SQL**, indica tu instancia. El valor inicial es `(localdb)\MSSQLLocalDB`. Si usas SQL Server Express y tu instancia se llama SQLEXPRESS, escribe `.\SQLEXPRESS`.
3. Usa **Preparar base** la primera vez. Crea la base `DulceLu` y sus procedimientos en la instancia indicada. Esta acción requiere permisos para crear la base y sus objetos. Para una base ya preparada, usa **Probar y conectar**.
4. Abre un mantenedor y elige **Nuevo**. Selecciona una fila para editarla o inhabilitarla. Filtra por **Inactivos** para reactivarla.

Los mantenedores requieren una instancia accesible de SQL Server 2016 SP1 o posterior. La interfaz de demostración se puede explorar antes de conectar. La app admite autenticación de Windows y de SQL Server. La configuración del diálogo se usa durante la sesión. Para cambiar el servidor predeterminado, edita la clave `SqlServer` en `Aplicacion/DulceLu.exe.config`. La contraseña ingresada en el diálogo queda en memoria durante la sesión.

La conexión solicita cifrado. Para una instancia local con certificado autofirmado puedes marcar **Confiar en el certificado de esta instancia local**. Para un servidor con un certificado válido, deja esa opción desmarcada.

También puedes ejecutar **01_CrearDulceLu.sql** desde SQL Server Management Studio con una cuenta autorizada. **02_DatosDemo.sql** agrega registros ficticios para probar los mantenedores. Los scripts están en **Codigo fuente/BaseDatos**. Para una cuenta de uso diario basta con acceso a la base, permiso de lectura sobre `dbo.Configuracion` y permiso de ejecución de los procedimientos de catálogo.

- [Instalación y uso de SQL Server Express LocalDB](https://learn.microsoft.com/es-es/sql/database-engine/configure-windows/sql-server-express-localdb)
- [SQL Server Management Studio](https://learn.microsoft.com/es-es/ssms/install/install)

## Alcance

| Sección | Qué incluye |
| --- | --- |
| Productos | Código, nombre, categoría, precio en soles y stock. |
| Mesas | Código, zona y capacidad. |
| Clientes | Nombre o razón social, documento, teléfono y correo. |
| Métodos de pago | Nombre y descripción. |
| Operación | Pedidos, reservaciones, ventas y caja, cocina y producción. Sus listas y formularios están identificados como demostraciones y usan datos ficticios. |

Los cuatro mantenedores guardan sus cambios en SQL Server. La baja es lógica y reversible. El código de producto o mesa, el documento de cliente cuando se informa y el nombre del método de pago son únicos. Para comprobar la persistencia, crea un registro, edítalo, inhabilítalo y vuelve a abrir la app. Conecta a la misma instancia y búscalo en **Inactivos**. Al reactivarlo, aparecerá de nuevo en **Activos**.

## Código fuente

Abre **Codigo fuente/DulceLu.sln** en Visual Studio 2022 con **Desarrollo de escritorio de .NET** y el paquete de destino de .NET Framework 4.8. Define **CapaPresentacion** como proyecto de inicio si Visual Studio te lo solicita. También puedes compilar con el SDK de .NET y el Developer Pack de .NET Framework 4.8.

```powershell
dotnet build DulceLu.sln -c Release -p:RestoreConfigFile=NuGet.Config
```

La solución usa cuatro proyectos. **CapaPresentacion** contiene los formularios y se comunica con **CapaLogicaNegocio**, que valida los datos y solicita las operaciones a **CapaAccesoDatos**. La capa de acceso llama a procedimientos almacenados con parámetros mediante ADO.NET. **CapaEntidad** define los registros compartidos. La presentación, la lógica y los datos usan estas entidades.

El alcance se basa en los cinco casos de uso de Dulce Lu y la separación de capas sigue la guía del mantenedor de clientes de MOANSO.
