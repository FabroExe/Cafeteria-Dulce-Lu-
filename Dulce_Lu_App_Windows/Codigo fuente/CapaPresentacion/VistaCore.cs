using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static DulceLu.Presentacion.Diseno;

namespace DulceLu.Presentacion
{
    internal sealed class VistaCore : UserControl
    {
        private readonly ModuloCore modulo;
        private readonly DataGridView tabla = Cuadricula();
        private readonly TextBox filtro = new TextBox { BorderStyle = BorderStyle.None, Font = Texto, BackColor = Papel, ForeColor = Tinta };
        private readonly Label pie = Etiqueta("", Peque, Suave);
        private readonly FlowLayoutPanel pestañas = new FlowLayoutPanel { WrapContents = false, Margin = Padding.Empty };
        private PestañaCore actual;

        public VistaCore(ModuloCore modulo)
        {
            this.modulo = modulo;
            actual = modulo.Pestañas[0];
            BackColor = Fondo;
            tabla.DefaultCellStyle.Padding = new Padding(5, 0, 2, 0);
            tabla.ColumnHeadersDefaultCellStyle.Padding = new Padding(5, 0, 2, 0);
            var lienzo = Filas();
            Controls.Add(lienzo);
            AgregarFila(lienzo, Encabezado(modulo.Rol, modulo.Titulo, modulo.Descripcion), SizeType.Absolute, 96);
            AgregarFila(lienzo, Aviso(), SizeType.Absolute, 45);

            var columnas = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 211));
            columnas.Controls.Add(AreaDatos(), 0, 0);
            columnas.Controls.Add(AreaAcciones(), 1, 0);
            AgregarFila(lienzo, columnas, SizeType.Percent, 100);
            CambiarPestaña(actual);
        }

        private static Control Aviso()
        {
            var panel = new Panel { BackColor = Miel, Margin = new Padding(0, 0, 0, 10) };
            var texto = Etiqueta("VISTA DEMO   ·   Datos ficticios. Las acciones son de muestra.", Micro, Tinta);
            texto.Location = new Point(12, 8);
            panel.Controls.Add(texto);
            return panel;
        }

        private Control AreaDatos()
        {
            var caja = Tarjeta();
            caja.Dock = DockStyle.Fill;
            caja.Margin = new Padding(0, 0, 12, 0);
            caja.Padding = new Padding(12, 12, 12, 10);
            var filas = Filas();
            caja.Controls.Add(filas);
            foreach (var pestaña in modulo.Pestañas)
            {
                var boton = Boton(pestaña.Titulo, ancho: TextRenderer.MeasureText(pestaña.Titulo, Medio).Width + 22);
                boton.Height = 32;
                boton.Margin = new Padding(0, 0, 5, 0);
                boton.Click += (_, __) => CambiarPestaña(pestaña);
                pestañas.Controls.Add(boton);
            }
            AgregarFila(filas, pestañas, SizeType.Absolute, 41);
            var busqueda = new Panel { Margin = new Padding(0, 0, 0, 9), Padding = new Padding(8), BackColor = Fondo };
            filtro.BackColor = Fondo;
            filtro.Dock = DockStyle.Top;
            filtro.AccessibleName = "Buscar en los datos de ejemplo";
            Pista(filtro, "Buscar en la lista…");
            filtro.TextChanged += (_, __) => MostrarFilas();
            busqueda.Controls.Add(filtro);
            AgregarFila(filas, busqueda, SizeType.Absolute, 44);
            AgregarFila(filas, tabla, SizeType.Percent, 100);
            pie.Padding = new Padding(1, 8, 0, 0);
            AgregarFila(filas, pie, SizeType.Absolute, 26);
            return caja;
        }

        private Control AreaAcciones()
        {
            var caja = Tarjeta();
            caja.Dock = DockStyle.Fill;
            caja.AutoScroll = true;
            var bloque = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(13, 13, 11, 12)
            };
            var titulo = Etiqueta("Acciones", Subtitulo);
            titulo.Margin = new Padding(0, 0, 0, 11);
            bloque.Controls.Add(titulo);
            foreach (var accion in modulo.Acciones)
            {
                var boton = Boton(accion.Nombre, accion == modulo.Acciones[0], 177);
                boton.Margin = new Padding(0, 0, 0, 7);
                boton.Click += (_, __) =>
                {
                    using var dialogo = new FormularioCore(accion);
                    dialogo.ShowDialog(this);
                };
                bloque.Controls.Add(boton);
            }

            var subtitulo = Etiqueta("EN EL FLUJO", Micro, Acento);
            subtitulo.Margin = new Padding(0, 15, 0, 7);
            bloque.Controls.Add(subtitulo);
            var nota = Etiqueta(modulo.Nota, Peque, Suave);
            nota.MaximumSize = new Size(174, 0);
            bloque.Controls.Add(nota);
            caja.Controls.Add(bloque);
            return caja;
        }

        private void CambiarPestaña(PestañaCore pestaña)
        {
            actual = pestaña;
            foreach (Button boton in pestañas.Controls)
            {
                boton.BackColor = boton.Text == pestaña.Titulo ? Arcilla : Papel;
                boton.ForeColor = boton.Text == pestaña.Titulo ? Acento : Tinta;
            }
            tabla.Columns.Clear();
            foreach (string titulo in actual.Columnas)
            {
                Columna(tabla, titulo, PesoColumna(titulo));
                tabla.Columns[tabla.Columns.Count - 1].MinimumWidth = 70;
            }
            tabla.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            MostrarFilas();
        }

        private static int PesoColumna(string titulo)
        {
            switch (titulo)
            {
                case "Preparación": case "Cliente": case "Producto": case "Insumo": case "Artículo": return 170;
                case "Estado": case "Período": case "Método de pago": return 122;
                case "Hora": case "Plan": case "Mesa": return 76;
                default: return 96;
            }
        }

        private void MostrarFilas()
        {
            tabla.Rows.Clear();
            var consulta = filtro.Text.Trim();
            foreach (var fila in actual.Filas.Where(f => f.Any(c => c.IndexOf(consulta, StringComparison.CurrentCultureIgnoreCase) >= 0)))
            {
                var indice = tabla.Rows.Add(fila.Cast<object>().ToArray());
                for (int i = 0; i < fila.Length; i++) tabla.Rows[indice].Cells[i].ToolTipText = fila[i];
                if (actual.Columnas.Last() == "Estado")
                    tabla.Rows[indice].Cells[fila.Length - 1].Style.ForeColor =
                        fila.Last() == "Pendiente" || fila.Last() == "Faltante" ? Acento : Verde;
            }
            tabla.ClearSelection();
            int cantidad = tabla.Rows.Count;
            pie.Text = cantidad == 1 ? "1 registro de ejemplo" : $"{cantidad} registros de ejemplo";
        }
    }

    internal sealed class FormularioCore : Form
    {
        public FormularioCore(AccionCore accion)
        {
            Text = accion.Nombre;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Fondo;
            Font = Diseno.Texto;
            ClientSize = new Size(430, 155 + accion.Campos.Length * 61);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22) };
            Controls.Add(panel);
            var filas = Filas();
            panel.Controls.Add(filas);
            AgregarFila(filas, Etiqueta(accion.Nombre, Subtitulo), SizeType.Absolute, 35);
            AgregarFila(filas, Etiqueta("Formulario de demostración", Micro, Acento), SizeType.Absolute, 29);
            foreach (var campo in accion.Campos)
            {
                var opciones = campo.Split('|');
                AgregarFila(filas, Etiqueta(opciones[0], Medio), SizeType.Absolute, 22);
                Control entrada;
                if (opciones.Length > 1)
                {
                    var lista = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Diseno.Texto };
                    lista.Items.AddRange(opciones.Skip(1).Cast<object>().ToArray());
                    lista.SelectedIndex = 0;
                    entrada = lista;
                }
                else
                {
                    entrada = new TextBox { Font = Diseno.Texto, BorderStyle = BorderStyle.FixedSingle };
                }
                entrada.AccessibleName = opciones[0];
                entrada.Margin = new Padding(0, 0, 0, 8);
                AgregarFila(filas, entrada, SizeType.Absolute, 39);
            }
            var cierre = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Margin = Padding.Empty };
            var boton = Boton("Cerrar vista", principal: true);
            boton.DialogResult = DialogResult.Cancel;
            CancelButton = boton;
            cierre.Controls.Add(boton);
            AgregarFila(filas, cierre, SizeType.Percent, 100);
        }
    }
}
