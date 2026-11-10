using System;

namespace DulceLu.Entidad
{
    public abstract class Registro
    {
        public int Id { get; set; }
        public bool Activo { get; set; } = true;
        public byte[] Version { get; set; } = Array.Empty<byte>();
    }

    public sealed class Producto : Registro
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    public sealed class Mesa : Registro
    {
        public string Codigo { get; set; } = "";
        public string Zona { get; set; } = "";
        public int Capacidad { get; set; }
    }

    public sealed class Cliente : Registro
    {
        public string Nombre { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Correo { get; set; } = "";
    }

    public sealed class MetodoPago : Registro
    {
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    public sealed class ConfiguracionSql
    {
        public string Servidor { get; }
        public bool AutenticacionWindows { get; }
        public string Usuario { get; }
        public string Clave { get; }
        public bool ConfiarCertificado { get; }

        public ConfiguracionSql(string servidor, bool windows = true, string usuario = "",
            string clave = "", bool confiarCertificado = false)
        {
            Servidor = servidor.Trim();
            AutenticacionWindows = windows;
            Usuario = usuario.Trim();
            Clave = clave;
            ConfiarCertificado = confiarCertificado;
        }
    }
}
