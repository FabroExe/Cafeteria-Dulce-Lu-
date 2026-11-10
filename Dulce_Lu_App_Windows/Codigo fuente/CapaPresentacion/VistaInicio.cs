using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static DulceLu.Presentacion.Diseno;

namespace DulceLu.Presentacion
{
    internal sealed class VistaInicio : UserControl
    {
        public VistaInicio(Action<string> navegar)
        {
            BackColor = Fondo;
            AutoScroll = true;
            var lienzo = Filas();
            lienzo.MinimumSize = new Size(0, 500);
            Controls.Add(lienzo);
            AgregarFila(lienzo, Bienvenida(navegar), SizeType.Absolute, 195);
            var titulo = new Panel { Margin = Padding.Empty };
            var nombre = Etiqueta("Operación de la cafetería", Subtitulo);
            nombre.Location = new Point(0, 19);
            titulo.Controls.Add(nombre);
            var detalle = Etiqueta("Cinco pantallas de demostración, desde el salón hasta la producción.", Peque, Suave);
            detalle.Location = new Point(1, 47);
            titulo.Controls.Add(detalle);
            AgregarFila(lienzo, titulo, SizeType.Absolute, 75);

            var modulos = new TableLayoutPanel { ColumnCount = 3, RowCount = 2, Margin = Padding.Empty };
            for (int i = 0; i < 3; i++) modulos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            for (int i = 0; i < 2; i++) modulos.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            for (int i = 0; i < ModuloCore.Todos.Length; i++)
            {
                var modulo = ModuloCore.Todos[i];
                modulos.Controls.Add(TarjetaModulo(modulo.Clave, modulo.Rol, modulo.Titulo, modulo.Corta, navegar), i % 3, i / 3);
            }
            modulos.Controls.Add(TarjetaModulo("productos", "CATÁLOGOS  /  SQL SERVER", "Mantenedores", "Gestiona la carta, mesas, clientes y medios de pago.", navegar), 2, 1);
            AgregarFila(lienzo, modulos, SizeType.Absolute, 248);
            AgregarFila(lienzo, new Panel { Margin = Padding.Empty }, SizeType.Percent, 100);
        }

        private static Panel Bienvenida(Action<string> navegar)
        {
            var panel = new Panel { BackColor = Color.FromArgb(232, 205, 187), Margin = Padding.Empty };
            panel.Paint += (_, e) => DibujarTaza(e.Graphics, panel.Width, panel.Height);
            panel.Resize += (_, __) => panel.Invalidate();
            var seccion = Etiqueta("DULCE LU   /   GESTIÓN INTERNA", Micro, Acento);
            seccion.Location = new Point(24, 18);
            panel.Controls.Add(seccion);
            var titulo = Etiqueta("Bienvenido a Dulce Lu", Titulo);
            titulo.Location = new Point(22, 42);
            panel.Controls.Add(titulo);
            var bajada = Etiqueta("La carta, el salón y la cocina en un solo lugar.", Texto, Suave);
            bajada.Location = new Point(25, 94);
            panel.Controls.Add(bajada);
            var boton = Boton("Abrir carta  →", principal: true, ancho: 142);
            boton.Location = new Point(25, 134);
            boton.Click += (_, __) => navegar("productos");
            panel.Controls.Add(boton);
            return panel;
        }

        private static Panel TarjetaModulo(string clave, string rol, string titulo, string descripcion, Action<string> navegar)
        {
            var card = Tarjeta();
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 10, 10);
            var icono = new PictureBox
            {
                Image = Icono(clave, Acento, 24), Location = new Point(13, 14),
                Size = new Size(25, 25), Cursor = Cursors.Hand
            };
            card.Controls.Add(icono);
            var indicador = Etiqueta(rol.ToUpperInvariant(), Micro, Acento);
            indicador.Location = new Point(45, 13);
            card.Controls.Add(indicador);
            var nombre = Etiqueta(titulo, Medio);
            nombre.Location = new Point(45, 34);
            card.Controls.Add(nombre);
            var texto = Etiqueta(descripcion, Peque, Suave);
            texto.Location = new Point(14, 62);
            texto.AutoSize = false;
            texto.Size = new Size(200, 42);
            texto.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            card.Controls.Add(texto);
            card.Resize += (_, __) => texto.Width = Math.Max(60, card.ClientSize.Width - 26);
            var enlace = Boton("›", ancho: 26);
            enlace.Height = 25;
            enlace.FlatAppearance.BorderSize = 0;
            enlace.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            enlace.Location = new Point(card.Width - 31, 26);
            enlace.AccessibleName = "Abrir " + titulo;
            card.Controls.Add(enlace);
            card.Cursor = Cursors.Hand;
            foreach (Control control in card.Controls) control.Click += (_, __) => navegar(clave);
            card.Click += (_, __) => navegar(clave);
            return card;
        }

        private static void DibujarTaza(Graphics g, int ancho, int alto)
        {
            if (ancho < 620) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var estado = g.Save();
            g.TranslateTransform(ancho - 245, alto / 2f - 83);
            using var crema = new SolidBrush(Color.FromArgb(247, 226, 205));
            using var linea = new Pen(Color.FromArgb(138, 73, 58), 2) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var borde = new Pen(Color.FromArgb(199, 151, 130), 1);
            using var cafe = new SolidBrush(Color.FromArgb(173, 101, 78));
            g.FillEllipse(crema, 35, 8, 154, 154);
            g.DrawEllipse(borde, 27, 0, 170, 170);
            g.DrawEllipse(linea, 48, 126, 130, 16);
            g.DrawArc(linea, 143, 74, 31, 32, -95, 190);
            g.FillEllipse(cafe, 66, 64, 89, 18);
            g.DrawEllipse(linea, 65, 61, 92, 22);
            using var taza = new GraphicsPath();
            taza.AddBezier(65, 73, 71, 128, 84, 130, 111, 131);
            taza.AddBezier(111, 131, 138, 130, 151, 128, 157, 73);
            g.DrawPath(linea, taza);
            g.DrawBezier(linea, 96, 51, 83, 41, 110, 37, 96, 25);
            g.DrawBezier(linea, 114, 51, 101, 37, 128, 33, 114, 19);
            g.DrawLine(linea, 181, 43, 195, 43);
            g.DrawLine(linea, 188, 36, 188, 50);
            g.DrawLine(borde, 28, 114, 38, 114);
            g.DrawLine(borde, 33, 109, 33, 119);
            g.Restore(estado);
        }
    }
}
