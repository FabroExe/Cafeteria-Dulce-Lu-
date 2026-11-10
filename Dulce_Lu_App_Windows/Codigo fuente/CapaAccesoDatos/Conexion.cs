using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DulceLu.Entidad;

namespace DulceLu.AccesoDatos
{
    public sealed class Conexion
    {
        public const string BaseDatos = "DulceLu";
        private readonly ConfiguracionSql configuracion;

        public Conexion(ConfiguracionSql configuracion)
        {
            this.configuracion = configuracion;
        }

        public SqlConnection Crear(string baseDatos = BaseDatos)
        {
            var cadena = new SqlConnectionStringBuilder
            {
                DataSource = configuracion.Servidor,
                InitialCatalog = baseDatos,
                IntegratedSecurity = configuracion.AutenticacionWindows,
                Encrypt = true,
                TrustServerCertificate = configuracion.ConfiarCertificado,
                ConnectTimeout = 5,
                ApplicationName = "Dulce Lu"
            };

            if (!configuracion.AutenticacionWindows)
            {
                cadena.UserID = configuracion.Usuario;
                cadena.Password = configuracion.Clave;
            }
            return new SqlConnection(cadena.ConnectionString);
        }

        public async Task ComprobarBaseAsync()
        {
            using var conexion = Crear();
            await conexion.OpenAsync().ConfigureAwait(false);
            using var cmd = new SqlCommand("SELECT VersionEsquema FROM dbo.Configuracion WHERE Id = 1", conexion);
            if (!Equals(await cmd.ExecuteScalarAsync().ConfigureAwait(false), 1))
                throw new InvalidOperationException("La base de datos requiere el esquema de Dulce Lu versión 1.");
        }

        public async Task PrepararBaseAsync()
        {
            using var conexion = Crear("master");
            await conexion.OpenAsync().ConfigureAwait(false);
            using var lector = new StreamReader(typeof(Conexion).Assembly.GetManifestResourceStream("DulceLu.CrearBase.sql")
                ?? throw new InvalidOperationException("No se encontró el script de instalación."));
            var script = await lector.ReadToEndAsync().ConfigureAwait(false);
            foreach (var lote in Regex.Split(script, @"^[ \t]*GO[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(lote)) continue;
                using var comando = new SqlCommand(lote, conexion) { CommandTimeout = 60 };
                await comando.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}
