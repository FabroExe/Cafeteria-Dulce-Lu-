using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DulceLu.Entidad;

namespace DulceLu.AccesoDatos
{
    public abstract class CatalogoDatos<T> where T : Registro
    {
        private readonly Conexion conexion;
        private readonly string nombre;
        protected CatalogoDatos(Conexion conexion, string nombre)
        {
            this.conexion = conexion;
            this.nombre = nombre;
        }

        protected abstract T Leer(SqlDataReader fila);
        protected abstract void Parametros(SqlCommand comando, T registro);

        public async Task<List<T>> ListarAsync()
        {
            using var cn = conexion.Crear();
            using var cmd = Procedimiento(cn, "Listar");
            await cn.OpenAsync().ConfigureAwait(false);
            using var filas = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            var lista = new List<T>();
            while (await filas.ReadAsync().ConfigureAwait(false))
            {
                var registro = Leer(filas);
                registro.Id = filas.GetInt32(filas.GetOrdinal("Id"));
                registro.Activo = filas.GetBoolean(filas.GetOrdinal("Activo"));
                registro.Version = (byte[])filas["Version"];
                lista.Add(registro);
            }
            return lista;
        }

        public async Task GuardarAsync(T registro)
        {
            using var cn = conexion.Crear();
            using var cmd = Procedimiento(cn, registro.Id == 0 ? "Insertar" : "Actualizar");
            Parametros(cmd, registro);
            if (registro.Id != 0) Identificar(cmd, registro);
            await cn.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task CambiarEstadoAsync(T registro, bool activo)
        {
            using var cn = conexion.Crear();
            using var cmd = Procedimiento(cn, "CambiarEstado");
            Identificar(cmd, registro);
            cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;
            await cn.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private SqlCommand Procedimiento(SqlConnection cn, string accion) =>
            new SqlCommand($"dbo.sp{accion}{nombre}", cn) { CommandType = CommandType.StoredProcedure };

        private static void Identificar(SqlCommand cmd, T registro)
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = registro.Id;
            cmd.Parameters.Add("@Version", SqlDbType.Binary, 8).Value = registro.Version;
        }

        protected static void Texto(SqlCommand cmd, string nombre, int longitud, string valor, bool opcional = false) =>
            cmd.Parameters.Add(nombre, SqlDbType.NVarChar, longitud).Value =
                opcional && valor.Length == 0 ? (object)DBNull.Value : valor;

        protected static string Cadena(SqlDataReader fila, string columna)
        {
            int i = fila.GetOrdinal(columna);
            return fila.IsDBNull(i) ? "" : fila.GetString(i);
        }
    }

    public sealed class ProductoDatos : CatalogoDatos<Producto>
    {
        public ProductoDatos(Conexion conexion) : base(conexion, "Producto") { }
        protected override Producto Leer(SqlDataReader fila) => new Producto
        {
            Codigo = Cadena(fila, "Codigo"),
            Nombre = Cadena(fila, "Nombre"),
            Categoria = Cadena(fila, "Categoria"),
            Precio = fila.GetDecimal(fila.GetOrdinal("Precio")),
            Stock = fila.GetInt32(fila.GetOrdinal("Stock"))
        };

        protected override void Parametros(SqlCommand cmd, Producto producto)
        {
            Texto(cmd, "@Codigo", 24, producto.Codigo);
            Texto(cmd, "@Nombre", 100, producto.Nombre);
            Texto(cmd, "@Categoria", 50, producto.Categoria);
            var precio = cmd.Parameters.Add("@Precio", SqlDbType.Decimal);
            precio.Precision = 10;
            precio.Scale = 2;
            precio.Value = producto.Precio;
            cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = producto.Stock;
        }
    }

    public sealed class MesaDatos : CatalogoDatos<Mesa>
    {
        public MesaDatos(Conexion conexion) : base(conexion, "Mesa") { }
        protected override Mesa Leer(SqlDataReader fila) => new Mesa
        {
            Codigo = Cadena(fila, "Codigo"),
            Zona = Cadena(fila, "Zona"),
            Capacidad = fila.GetInt32(fila.GetOrdinal("Capacidad"))
        };

        protected override void Parametros(SqlCommand cmd, Mesa mesa)
        {
            Texto(cmd, "@Codigo", 16, mesa.Codigo);
            Texto(cmd, "@Zona", 50, mesa.Zona);
            cmd.Parameters.Add("@Capacidad", SqlDbType.Int).Value = mesa.Capacidad;
        }
    }

    public sealed class ClienteDatos : CatalogoDatos<Cliente>
    {
        public ClienteDatos(Conexion conexion) : base(conexion, "Cliente") { }
        protected override Cliente Leer(SqlDataReader fila) => new Cliente
        {
            Nombre = Cadena(fila, "Nombre"),
            Documento = Cadena(fila, "Documento"),
            Telefono = Cadena(fila, "Telefono"),
            Correo = Cadena(fila, "Correo")
        };

        protected override void Parametros(SqlCommand cmd, Cliente cliente)
        {
            Texto(cmd, "@Nombre", 100, cliente.Nombre);
            Texto(cmd, "@Documento", 16, cliente.Documento, opcional: true);
            Texto(cmd, "@Telefono", 24, cliente.Telefono, opcional: true);
            Texto(cmd, "@Correo", 120, cliente.Correo, opcional: true);
        }
    }

    public sealed class MetodoPagoDatos : CatalogoDatos<MetodoPago>
    {
        public MetodoPagoDatos(Conexion conexion) : base(conexion, "MetodoPago") { }
        protected override MetodoPago Leer(SqlDataReader fila) => new MetodoPago
        {
            Nombre = Cadena(fila, "Nombre"),
            Descripcion = Cadena(fila, "Descripcion")
        };

        protected override void Parametros(SqlCommand cmd, MetodoPago metodo)
        {
            Texto(cmd, "@Nombre", 50, metodo.Nombre);
            Texto(cmd, "@Descripcion", 160, metodo.Descripcion, opcional: true);
        }
    }
}
