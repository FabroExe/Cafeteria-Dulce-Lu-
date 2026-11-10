using System;
using System.Configuration;
using System.Globalization;
using System.Windows.Forms;
using DulceLu.Entidad;

namespace DulceLu.Presentacion
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var cultura = CultureInfo.GetCultureInfo("es-PE");
            CultureInfo.CurrentCulture = cultura;
            CultureInfo.CurrentUICulture = cultura;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var servidor = ConfigurationManager.AppSettings["SqlServer"] ?? @"(localdb)\MSSQLLocalDB";
            var local = servidor.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);
            Application.Run(new VentanaPrincipal(new ConfiguracionSql(servidor, confiarCertificado: local)));
        }
    }
}
