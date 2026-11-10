using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DulceLu.AccesoDatos;
using DulceLu.Entidad;

namespace DulceLu.LogicaNegocio
{
    public sealed class ErrorCatalogo : Exception
    {
        public ErrorCatalogo(string mensaje, Exception? causa = null) : base(mensaje, causa) { }
    }

    public sealed class Catalogo<T> where T : Registro
    {
        private readonly CatalogoDatos<T> datos;
        private readonly Action<T> validar;
        internal Catalogo(CatalogoDatos<T> datos, Action<T> validar)
        {
            this.datos = datos;
            this.validar = validar;
        }

        public async Task<List<T>> ListarAsync()
        {
            try { return await datos.ListarAsync().ConfigureAwait(false); }
            catch (SqlException ex) { throw GestionCatalogos.Traducir(ex); }
        }

        public async Task GuardarAsync(T registro)
        {
            validar(registro);
            try { await datos.GuardarAsync(registro).ConfigureAwait(false); }
            catch (SqlException ex) { throw GestionCatalogos.Traducir(ex); }
        }

        public async Task CambiarEstadoAsync(T registro, bool activo)
        {
            try { await datos.CambiarEstadoAsync(registro, activo).ConfigureAwait(false); }
            catch (SqlException ex) { throw GestionCatalogos.Traducir(ex); }
        }
    }

    public sealed class GestionCatalogos
    {
        private readonly Conexion conexion;
        public Catalogo<Producto> Productos { get; }
        public Catalogo<Mesa> Mesas { get; }
        public Catalogo<Cliente> Clientes { get; }
        public Catalogo<MetodoPago> MetodosPago { get; }

        public GestionCatalogos(ConfiguracionSql configuracion)
        {
            conexion = new Conexion(configuracion);
            Productos = new Catalogo<Producto>(new ProductoDatos(conexion), ValidarProducto);
            Mesas = new Catalogo<Mesa>(new MesaDatos(conexion), ValidarMesa);
            Clientes = new Catalogo<Cliente>(new ClienteDatos(conexion), ValidarCliente);
            MetodosPago = new Catalogo<MetodoPago>(new MetodoPagoDatos(conexion), ValidarMetodo);
        }

        public async Task ComprobarConexionAsync()
        {
            try { await conexion.ComprobarBaseAsync().ConfigureAwait(false); }
            catch (SqlException ex) { throw Traducir(ex); }
        }

        public async Task PrepararBaseAsync()
        {
            try { await conexion.PrepararBaseAsync().ConfigureAwait(false); }
            catch (SqlException ex) { throw Traducir(ex); }
            await ComprobarConexionAsync().ConfigureAwait(false);
        }

        private static void ValidarProducto(Producto p)
        {
            p.Codigo = Codigo(p.Codigo, "El código", 24);
            p.Nombre = Texto(p.Nombre, "El nombre", 100);
            p.Categoria = Texto(p.Categoria, "La categoría", 50);
            if (p.Precio < 0.01m || p.Precio > 99999999.99m || decimal.Round(p.Precio, 2) != p.Precio)
                throw new ErrorCatalogo("Ingresa un precio mayor que cero con hasta dos decimales.");
            if (p.Stock < 0) throw new ErrorCatalogo("El stock no puede ser negativo.");
        }

        private static void ValidarMesa(Mesa m)
        {
            m.Codigo = Codigo(m.Codigo, "El código", 16);
            m.Zona = Texto(m.Zona, "La zona", 50);
            if (m.Capacidad < 1 || m.Capacidad > 30)
                throw new ErrorCatalogo("La capacidad debe estar entre 1 y 30 personas.");
        }

        private static void ValidarCliente(Cliente c)
        {
            c.Nombre = Texto(c.Nombre, "El nombre o razón social", 100);
            c.Documento = Texto(c.Documento, "El documento", 16, opcional: true).ToUpperInvariant();
            c.Telefono = Texto(c.Telefono, "El teléfono", 24, opcional: true);
            c.Correo = Texto(c.Correo, "El correo", 120, opcional: true);

            if (c.Documento.Length > 0 && !Regex.IsMatch(c.Documento, "^[A-Z0-9-]{4,16}$"))
                throw new ErrorCatalogo("El documento debe tener entre 4 y 16 letras, números o guiones.");
            if (c.Telefono.Length > 0 && !Regex.IsMatch(c.Telefono, @"^\+?[0-9 ()-]{5,23}$|^[0-9 ()-]{24}$"))
                throw new ErrorCatalogo("Ingresa un teléfono válido, por ejemplo +51 987 654 321.");
            if (c.Correo.Length == 0) return;
            try
            {
                if (new MailAddress(c.Correo).Address != c.Correo) throw new FormatException();
            }
            catch (FormatException) { throw new ErrorCatalogo("Ingresa un correo válido, por ejemplo hola@ejemplo.pe."); }
        }

        private static void ValidarMetodo(MetodoPago m)
        {
            m.Nombre = Texto(m.Nombre, "El nombre", 50);
            m.Descripcion = Texto(m.Descripcion, "La descripción", 160, opcional: true);
        }

        private static string Codigo(string valor, string campo, int maximo)
        {
            valor = Texto(valor, campo, maximo).ToUpperInvariant();
            if (!Regex.IsMatch(valor, "^[A-Z0-9-]+$"))
                throw new ErrorCatalogo(campo + " admite letras, números y guiones.");
            return valor;
        }

        private static string Texto(string valor, string campo, int maximo, bool opcional = false)
        {
            valor = valor.Trim();
            if (!opcional && valor.Length == 0) throw new ErrorCatalogo(campo + " es obligatorio.");
            if (valor.Length > maximo) throw new ErrorCatalogo($"{campo} admite hasta {maximo} caracteres.");
            return valor;
        }

        internal static ErrorCatalogo Traducir(SqlException ex)
        {
            string mensaje;
            switch (ex.Number)
            {
                case 2601: case 2627:
                    mensaje = "Ya hay un registro con ese código, documento o nombre único.";
                    break;
                case 50001:
                    mensaje = "El registro cambió desde que lo abriste. Actualiza la lista y vuelve a intentarlo.";
                    break;
                case 4060: case 208: case 2812:
                    mensaje = "La base DulceLu no está disponible. Usa «Conexión SQL» para comprobar o preparar la base.";
                    break;
                case 229: case 262:
                    mensaje = "La cuenta de SQL Server no tiene permiso para realizar esta operación.";
                    break;
                case 50:
                    mensaje = "No se pudo iniciar LocalDB. Comprueba que SQL Server Express LocalDB esté instalado o indica otra instancia.";
                    break;
                case 50002: case 50003:
                    mensaje = ex.Message;
                    break;
                case 18456:
                    mensaje = "SQL Server rechazó el inicio de sesión. Revisa la autenticación y las credenciales.";
                    break;
                default:
                    mensaje = $"SQL Server no pudo completar la operación (código {ex.Number}). Revisa la instancia y prueba la conexión.";
                    break;
            }
            return new ErrorCatalogo(mensaje, ex);
        }
    }
}
