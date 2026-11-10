using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DulceLu.Presentacion
{
    internal static class Diseno
    {
        public static readonly Color Fondo = Color.FromArgb(247, 243, 237);
        public static readonly Color Papel = Color.FromArgb(255, 253, 248);
        public static readonly Color Tinta = Color.FromArgb(50, 32, 29);
        public static readonly Color Suave = Color.FromArgb(113, 92, 85);
        public static readonly Color Linea = Color.FromArgb(226, 214, 202);
        public static readonly Color Espresso = Color.FromArgb(55, 34, 31);
        public static readonly Color SidebarTexto = Color.FromArgb(243, 220, 204);
        public static readonly Color Acento = Color.FromArgb(145, 58, 42);
        public static readonly Color Arcilla = Color.FromArgb(246, 224, 213);
        public static readonly Color Salvia = Color.FromArgb(225, 235, 221);
        public static readonly Color Verde = Color.FromArgb(43, 86, 63);
        public static readonly Color Miel = Color.FromArgb(248, 232, 199);
        public static readonly Font Texto = new Font("Segoe UI", 10F);
        public static readonly Font Peque = new Font("Segoe UI", 9F);
        public static readonly Font Medio = new Font("Segoe UI Semibold", 10F);
        public static readonly Font Micro = new Font("Segoe UI Semibold", 8.5F);
        public static readonly Font Titulo = new Font("Georgia", 25F, FontStyle.Regular);
        public static readonly Font Subtitulo = new Font("Georgia", 15F, FontStyle.Regular);

        public static Label Etiqueta(string texto, Font? fuente = null, Color? color = null) => new Label
        {
            Text = texto, Font = fuente ?? Texto, ForeColor = color ?? Tinta,
            BackColor = Color.Transparent, AutoSize = true, Margin = Padding.Empty,
            UseMnemonic = false
        };

        public static Button Boton(string texto, bool principal = false, int ancho = 120) => new Button
        {
            Text = texto, Font = Medio, Width = ancho, Height = 36, Margin = Padding.Empty,
            FlatStyle = FlatStyle.Flat, FlatAppearance =
            {
                BorderSize = principal ? 0 : 1,
                BorderColor = Linea,
                MouseOverBackColor = principal ? Color.FromArgb(121, 45, 34) : Arcilla
            },
            BackColor = principal ? Acento : Papel, ForeColor = principal ? Color.White : Tinta,
            Cursor = Cursors.Hand, UseVisualStyleBackColor = false
        };

        public static TableLayoutPanel Filas() => new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 0, Margin = Padding.Empty
        };

        public static void AgregarFila(TableLayoutPanel tabla, Control control, SizeType tipo, float alto)
        {
            int fila = tabla.RowCount++;
            tabla.RowStyles.Add(new RowStyle(tipo, alto));
            control.Dock = DockStyle.Fill;
            tabla.Controls.Add(control, 0, fila);
        }

        public static Panel Tarjeta()
        {
            var panel = new Panel { BackColor = Papel, Margin = Padding.Empty };
            panel.Paint += (_, e) => ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle, Linea, ButtonBorderStyle.Solid);
            return panel;
        }

        public static Control Encabezado(string grupo, string titulo, string bajada)
        {
            var panel = new Panel { Margin = Padding.Empty };
            panel.Controls.Add(Etiqueta(grupo, Micro, Acento));
            var nombre = Etiqueta(titulo, Titulo);
            nombre.Top = 18;
            panel.Controls.Add(nombre);
            var texto = Etiqueta(bajada, Peque, Suave);
            texto.Top = 62;
            panel.Controls.Add(texto);
            return panel;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, string lParam);

        public static void Pista(TextBox caja, string pista)
        {
            void Aplicar() => SendMessage(caja.Handle, 0x1501, IntPtr.Zero, pista);
            caja.HandleCreated += (_, __) => Aplicar();
            if (caja.IsHandleCreated) Aplicar();
        }

        public static Bitmap Icono(string clave, Color color, int tamano = 20)
        {
            var bmp = new Bitmap(tamano, tamano);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.ScaleTransform(tamano / 24f, tamano / 24f);
            using var p = new Pen(color, 1.65f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            void Trazo(params int[] xy)
            {
                var puntos = new Point[xy.Length / 2];
                for (int i = 0; i < puntos.Length; i++) puntos[i] = new Point(xy[i * 2], xy[i * 2 + 1]);
                g.DrawLines(p, puntos);
            }

            switch (clave)
            {
                case "inicio":
                    Trazo(3, 10, 12, 3, 21, 10); Trazo(5, 10, 5, 21, 19, 21, 19, 10); Trazo(10, 21, 10, 15, 14, 15, 14, 21);
                    break;
                case "pedidos":
                    g.DrawRectangle(p, 5, 5, 14, 16); g.DrawRectangle(p, 9, 3, 6, 4);
                    Trazo(8, 11, 16, 11); Trazo(8, 15, 16, 15); Trazo(8, 18, 13, 18);
                    break;
                case "reservas":
                    g.DrawRectangle(p, 3, 5, 18, 16); Trazo(3, 10, 21, 10);
                    Trazo(8, 3, 8, 7); Trazo(16, 3, 16, 7); Trazo(8, 15, 11, 18, 16, 13);
                    break;
                case "ventas": case "pagos":
                    g.DrawRectangle(p, 3, 5, 18, 14); Trazo(3, 10, 21, 10); Trazo(7, 15, 11, 15); Trazo(15, 15, 17, 15);
                    break;
                case "cocina":
                    g.DrawArc(p, 4, 6, 16, 17, 180, 180); Trazo(2, 16, 22, 16); Trazo(5, 19, 19, 19); Trazo(11, 4, 13, 4);
                    break;
                case "produccion":
                    g.DrawRectangle(p, 4, 4, 16, 16); Trazo(4, 10, 20, 10); Trazo(4, 15, 20, 15); Trazo(10, 10, 10, 20);
                    break;
                case "productos": case "marca":
                    g.DrawArc(p, 15, 9, 6, 6, -85, 170); Trazo(4, 8, 16, 8, 15, 16, 13, 18, 7, 18, 5, 16, 4, 8);
                    Trazo(3, 21, 19, 21); Trazo(8, 5, 8, 3); Trazo(12, 5, 12, 2);
                    break;
                case "mesas":
                    Trazo(3, 8, 21, 8, 21, 11, 3, 11, 3, 8); Trazo(6, 11, 6, 20); Trazo(18, 11, 18, 20);
                    break;
                case "clientes":
                    g.DrawEllipse(p, 8, 3, 8, 8); g.DrawArc(p, 4, 13, 16, 13, 180, 180); Trazo(4, 19, 20, 19);
                    break;
                case "conexion":
                    g.DrawEllipse(p, 4, 3, 16, 6); Trazo(4, 6, 4, 17); Trazo(20, 6, 20, 17);
                    g.DrawArc(p, 4, 9, 16, 6, 0, 180); g.DrawArc(p, 4, 14, 16, 6, 0, 180);
                    break;
            }

            return bmp;
        }

        public static DataGridView Cuadricula()
        {
            var tabla = new DataGridView
            {
                BackgroundColor = Papel, BorderStyle = BorderStyle.None, GridColor = Linea,
                AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, RowHeadersVisible = false, MultiSelect = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 40, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EnableHeadersVisualStyles = false, RowTemplate = { Height = 42 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = Padding.Empty
            };
            tabla.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = Texto, ForeColor = Tinta, BackColor = Papel,
                SelectionBackColor = Arcilla, SelectionForeColor = Tinta,
                Padding = new Padding(9, 0, 4, 0),
                NullValue = ""
            };
            tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 247, 241);
            tabla.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = Medio, ForeColor = Suave, BackColor = Color.FromArgb(240, 233, 224),
                SelectionBackColor = Color.FromArgb(240, 233, 224),
                Padding = new Padding(9, 0, 4, 0)
            };
            return tabla;
        }

        public static void Columna(DataGridView tabla, string encabezado, int peso)
        {
            tabla.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = encabezado, FillWeight = peso, MinimumWidth = 62,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }
    }
}
