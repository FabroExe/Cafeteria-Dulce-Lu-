using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DulceLu.Entidad;
using DulceLu.LogicaNegocio;
using static DulceLu.Presentacion.Diseno;

namespace DulceLu.Presentacion
{
    internal sealed class DialogoConexion : Form
    {
        private readonly TextBox servidor = Caja("Instancia de SQL Server");
        private readonly ComboBox autenticacion = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Autenticación"
        };
        private readonly TextBox usuario = Caja("Usuario");
        private readonly TextBox clave = Caja("Contraseña");
        private readonly CheckBox confianza = new CheckBox { Text = "Confiar en el certificado de esta instancia local", AutoSize = true };
        private readonly Button comprobar = Boton("Probar y conectar", principal: true, ancho: 149);
        private readonly Button preparar = Boton("Preparar base", ancho: 129);
        private readonly Button cerrar = Boton("Cerrar", ancho: 79);
        private readonly Label aviso = Etiqueta("Si es la primera vez, prepara la base DulceLu en tu instancia.", Peque, Suave);
        private bool ocupado;
        public ConfiguracionSql Configuracion { get; private set; }

        public DialogoConexion(ConfiguracionSql actual)
        {
            Configuracion = actual;
            Text = "Conexión SQL Server";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Fondo;
            Font = Diseno.Texto;
            ClientSize = new Size(502, 400);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(21, 17, 21, 16) };
            Controls.Add(panel);
            var filas = Filas();
            panel.Controls.Add(filas);
            AgregarFila(filas, Etiqueta("CONFIGURACIÓN  /  BASE DULCELU", Micro, Acento), SizeType.Absolute, 20);
            AgregarFila(filas, Etiqueta("Conexión SQL Server", Subtitulo), SizeType.Absolute, 34);
            AgregarFila(filas, Etiqueta("Usa LocalDB, SQL Server Express o la instancia de tu equipo.", Peque, Suave), SizeType.Absolute, 30);

            servidor.Text = actual.Servidor;
            Pista(servidor, @"Ej. (localdb)\MSSQLLocalDB o .\SQLEXPRESS");
            Campo(filas, "Servidor o instancia", servidor);

            autenticacion.Items.AddRange(new object[] { "Autenticación de Windows", "Autenticación de SQL Server" });
            autenticacion.SelectedIndex = actual.AutenticacionWindows ? 0 : 1;
            Campo(filas, "Autenticación", autenticacion);

            usuario.Text = actual.Usuario;
            clave.Text = actual.Clave;
            clave.UseSystemPasswordChar = true;
            var credenciales = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            credenciales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            credenciales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            var izq = Filas();
            izq.Margin = new Padding(0, 0, 8, 0);
            Campo(izq, "Usuario", usuario);
            var der = Filas();
            Campo(der, "Contraseña", clave);
            credenciales.Controls.Add(izq, 0, 0);
            credenciales.Controls.Add(der, 1, 0);
            AgregarFila(filas, credenciales, SizeType.Absolute, 54);

            confianza.Checked = actual.ConfiarCertificado;
            confianza.ForeColor = Tinta;
            confianza.Font = Peque;
            confianza.Margin = Padding.Empty;
            AgregarFila(filas, confianza, SizeType.Absolute, 30);
            aviso.AutoSize = false;
            aviso.Padding = new Padding(0, 4, 0, 0);
            AgregarFila(filas, aviso, SizeType.Percent, 100);

            var botones = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Margin = Padding.Empty };
            botones.Controls.Add(comprobar);
            preparar.Margin = new Padding(0, 0, 7, 0);
            botones.Controls.Add(preparar);
            cerrar.Margin = new Padding(0, 0, 7, 0);
            cerrar.DialogResult = DialogResult.Cancel;
            botones.Controls.Add(cerrar);
            AgregarFila(filas, botones, SizeType.Absolute, 37);
            AcceptButton = comprobar;
            CancelButton = cerrar;

            autenticacion.SelectedIndexChanged += (_, __) => Habilitar();
            comprobar.Click += async (_, __) => await EjecutarAsync(false);
            preparar.Click += async (_, __) => await EjecutarAsync(true);
            Habilitar();
        }

        private static TextBox Caja(string nombre) => new TextBox
        {
            BorderStyle = BorderStyle.FixedSingle, Font = Diseno.Texto, AccessibleName = nombre
        };

        private static void Campo(TableLayoutPanel tabla, string titulo, Control control)
        {
            AgregarFila(tabla, Etiqueta(titulo, Medio), SizeType.Absolute, 21);
            control.Margin = new Padding(0, 0, 0, 6);
            AgregarFila(tabla, control, SizeType.Absolute, 32);
        }

        private void Habilitar()
        {
            servidor.Enabled = autenticacion.Enabled = confianza.Enabled = !ocupado;
            usuario.Enabled = clave.Enabled = !ocupado && autenticacion.SelectedIndex == 1;
            comprobar.Enabled = preparar.Enabled = cerrar.Enabled = !ocupado;
        }

        private async Task EjecutarAsync(bool instalar)
        {
            if (ocupado) return;
            if (string.IsNullOrWhiteSpace(servidor.Text) ||
                autenticacion.SelectedIndex == 1 && string.IsNullOrWhiteSpace(usuario.Text))
            {
                MostrarAviso("Completa el servidor y, si corresponde, el usuario de SQL Server.", Acento);
                return;
            }
            var config = new ConfiguracionSql(servidor.Text, autenticacion.SelectedIndex == 0, usuario.Text, clave.Text, confianza.Checked);
            var gestion = new GestionCatalogos(config);
            ocupado = true;
            Habilitar();
            MostrarAviso(instalar ? "Preparando la base DulceLu…" : "Comprobando la conexión…", Suave);
            try
            {
                if (instalar) await gestion.PrepararBaseAsync();
                else await gestion.ComprobarConexionAsync();
                Configuracion = config;
                ocupado = false;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { MostrarAviso(ex.Message, Acento); }
            finally
            {
                ocupado = false;
                Habilitar();
            }
        }

        private void MostrarAviso(string texto, Color color)
        {
            aviso.Text = texto;
            aviso.ForeColor = color;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (ocupado) e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
