using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DulceLu.Entidad;
using DulceLu.LogicaNegocio;
using static DulceLu.Presentacion.Diseno;

namespace DulceLu.Presentacion
{
    internal sealed class VentanaPrincipal : Form
    {
        private readonly Dictionary<string, Button> menu = new Dictionary<string, Button>();
        private readonly Panel cuerpo = new Panel { Dock = DockStyle.Fill, BackColor = Fondo };
        private readonly Label ruta = Etiqueta("Inicio", Medio);
        private readonly Label estado = Etiqueta("SQL · por comprobar", Micro, Suave);
        private readonly Button conexion = Boton("Conexión SQL", ancho: 126);
        private ConfiguracionSql configuracion;
        private GestionCatalogos gestion;
        private string pagina = "inicio";

        public VentanaPrincipal(ConfiguracionSql configuracion)
        {
            this.configuracion = configuracion;
            gestion = new GestionCatalogos(configuracion);
            Text = "Dulce Lu  |  Gestión de cafetería";
            using var imagenMarca = typeof(VentanaPrincipal).Assembly.GetManifestResourceStream("DulceLu.Marca.ico");
            if (imagenMarca != null) Icon = new Icon(imagenMarca);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1190, 706);
            MinimumSize = new Size(990, 680);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Diseno.Texto;
            BackColor = Fondo;

            var marco = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            marco.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 207));
            marco.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(marco);
            marco.Controls.Add(BarraLateral(), 0, 0);

            var escritorio = Filas();
            marco.Controls.Add(escritorio, 1, 0);
            AgregarFila(escritorio, BarraSuperior(), SizeType.Absolute, 60);
            cuerpo.Padding = new Padding(28, 23, 28, 22);
            AgregarFila(escritorio, cuerpo, SizeType.Percent, 100);
            conexion.Click += (_, __) => Configurar();
            Shown += async (_, __) => await ComprobarAsync();
            Navegar("inicio");
        }

        private Control BarraLateral()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Espresso, Margin = Padding.Empty };
            var listado = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = true, Padding = new Padding(12, 0, 7, 0)
            };
            panel.Controls.Add(listado);
            var marca = new Panel { Dock = DockStyle.Top, Height = 93 };
            var icono = new PictureBox
            {
                Location = new Point(17, 23), Size = new Size(37, 37), Image = Icono("marca", Color.FromArgb(246, 188, 152), 35)
            };
            marca.Controls.Add(icono);
            var nombre = Etiqueta("Dulce Lu", new Font("Georgia", 19F, FontStyle.Bold), Color.FromArgb(255, 238, 222));
            nombre.Location = new Point(60, 17);
            marca.Controls.Add(nombre);
            var subtitulo = Etiqueta("CAFETERÍA  /  GESTIÓN", Micro, SidebarTexto);
            subtitulo.Location = new Point(62, 51);
            marca.Controls.Add(subtitulo);
            panel.Controls.Add(marca);
            var pie = Etiqueta("DULCE LU  ·  ESCRITORIO", Micro, SidebarTexto);
            pie.Dock = DockStyle.Bottom;
            pie.Padding = new Padding(17, 7, 0, 12);
            panel.Controls.Add(pie);

            Enlace(listado, "inicio", "Inicio");
            Grupo(listado, "OPERACIÓN");
            foreach (var modulo in ModuloCore.Todos) Enlace(listado, modulo.Clave, modulo.Titulo);
            Grupo(listado, "MANTENEDORES");
            Enlace(listado, "productos", "Productos");
            Enlace(listado, "mesas", "Mesas");
            Enlace(listado, "clientes", "Clientes");
            Enlace(listado, "pagos", "Métodos de pago");
            return panel;
        }

        private static void Grupo(FlowLayoutPanel panel, string nombre)
        {
            var etiqueta = Etiqueta(nombre, Micro, Color.FromArgb(222, 169, 141));
            etiqueta.Margin = new Padding(9, 12, 0, 5);
            panel.Controls.Add(etiqueta);
        }

        private void Enlace(FlowLayoutPanel lista, string clave, string texto)
        {
            var boton = Boton(texto, ancho: 176);
            boton.Height = 34;
            boton.Margin = new Padding(0, 0, 0, 2);
            boton.Image = Icono(clave, SidebarTexto, 18);
            boton.ImageAlign = ContentAlignment.MiddleLeft;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.TextImageRelation = TextImageRelation.ImageBeforeText;
            boton.Padding = new Padding(8, 0, 0, 0);
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(86, 49, 42);
            boton.ForeColor = SidebarTexto;
            boton.BackColor = Espresso;
            boton.Click += (_, __) => Navegar(clave);
            menu.Add(clave, boton);
            lista.Controls.Add(boton);
        }

        private Control BarraSuperior()
        {
            var panel = new Panel { BackColor = Papel, Margin = Padding.Empty };
            var linea = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Linea };
            ruta.Location = new Point(28, 20);
            panel.Controls.Add(ruta);
            var derecha = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Dock = DockStyle.Right,
                Width = 363, Padding = new Padding(0, 12, 24, 0)
            };
            estado.Margin = new Padding(0, 9, 13, 0);
            derecha.Controls.Add(conexion);
            derecha.Controls.Add(estado);
            panel.Controls.Add(derecha);
            panel.Controls.Add(linea);
            linea.BringToFront();
            return panel;
        }

        private async Task ComprobarAsync()
        {
            var actual = gestion;
            try
            {
                await actual.ComprobarConexionAsync();
                if (!IsDisposed && gestion == actual) MostrarConexion(true);
            }
            catch (Exception)
            {
                if (!IsDisposed && gestion == actual) MostrarConexion(false);
            }
        }

        private void MostrarConexion(bool disponible)
        {
            estado.Text = disponible ? "●  SQL conectado" : "○  SQL sin conexión";
            estado.ForeColor = disponible ? Verde : Acento;
        }

        private void Configurar()
        {
            using var dialogo = new DialogoConexion(configuracion);
            if (dialogo.ShowDialog(this) != DialogResult.OK) return;
            configuracion = dialogo.Configuracion;
            gestion = new GestionCatalogos(configuracion);
            MostrarConexion(true);
            Navegar(pagina);
        }

        internal void Navegar(string clave)
        {
            pagina = clave;
            foreach (var par in menu)
                par.Value.BackColor = par.Key == clave ? Color.FromArgb(119, 59, 48) : Espresso;
            string grupo = clave == "inicio" ? "ESCRITORIO" : "MANTENEDORES";
            Control vista;
            switch (clave)
            {
                case "inicio":
                    vista = new VistaInicio(Navegar);
                    break;
                case "productos":
                    vista = CatalogosVisuales.Productos(gestion.Productos, Configurar);
                    break;
                case "mesas":
                    vista = CatalogosVisuales.Mesas(gestion.Mesas, Configurar);
                    break;
                case "clientes":
                    vista = CatalogosVisuales.Clientes(gestion.Clientes, Configurar);
                    break;
                case "pagos":
                    vista = CatalogosVisuales.Metodos(gestion.MetodosPago, Configurar);
                    break;
                default:
                    grupo = "OPERACIÓN";
                    vista = new VistaCore(Array.Find(ModuloCore.Todos, modulo => modulo.Clave == clave)!);
                    break;
            }
            ruta.Text = $"{grupo}  /  {menu[clave].Text}";
            cuerpo.SuspendLayout();
            while (cuerpo.Controls.Count > 0) cuerpo.Controls[0].Dispose();
            vista.Dock = DockStyle.Fill;
            cuerpo.Controls.Add(vista);
            cuerpo.ResumeLayout(true);
        }
    }
}
