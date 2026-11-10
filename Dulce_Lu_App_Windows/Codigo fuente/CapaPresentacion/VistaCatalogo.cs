using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DulceLu.Entidad;
using DulceLu.LogicaNegocio;
using static DulceLu.Presentacion.Diseno;

namespace DulceLu.Presentacion
{
    internal sealed class VistaCatalogo<T> : UserControl where T : Registro, new()
    {
        private readonly string singular;
        private readonly Catalogo<T> servicio;
        private readonly CampoCatalogo<T>[] campos;
        private readonly DataGridView tabla = Cuadricula();
        private readonly TextBox filtro = new TextBox { BorderStyle = BorderStyle.None, Font = Diseno.Texto, BackColor = Papel, ForeColor = Tinta };
        private readonly ComboBox estados = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Diseno.Texto };
        private readonly Label mensaje = Etiqueta("", Peque, Suave);
        private readonly Button editar = Boton("Editar", ancho: 82);
        private readonly Button cambiar = Boton("Inhabilitar", ancho: 105);
        private readonly Button nuevo = Boton("+  Nuevo", principal: true, ancho: 101);
        private readonly Button recargar = Boton("Actualizar", ancho: 95);
        private readonly Button configurar = Boton("Configurar SQL", ancho: 124);
        private readonly Panel vacio = new Panel { BackColor = Papel, Dock = DockStyle.Fill, Visible = false };
        private readonly Label tituloVacio = Etiqueta("", Subtitulo);
        private readonly Label textoVacio = Etiqueta("", Peque, Suave);
        private List<T> registros = new List<T>();
        private bool ocupado;
        private bool disponible;
        private bool inicioCarga;

        public VistaCatalogo(string titulo, string singular, string descripcion, Catalogo<T> servicio,
            Action abrirConfiguracion, params CampoCatalogo<T>[] campos)
        {
            this.singular = singular;
            this.servicio = servicio;
            this.campos = campos;
            BackColor = Fondo;
            var lienzo = Filas();
            Controls.Add(lienzo);
            AgregarFila(lienzo, Encabezado("CATÁLOGO   /   SQL SERVER", titulo, descripcion), SizeType.Absolute, 94);
            AgregarFila(lienzo, BarraHerramientas(), SizeType.Absolute, 51);
            AgregarFila(lienzo, AreaTabla(), SizeType.Percent, 100);
            AgregarFila(lienzo, PieCatalogo(), SizeType.Absolute, 57);

            foreach (var campo in campos) Columna(tabla, campo.Titulo, campo.Peso);
            Columna(tabla, "Estado", 82);
            tabla.SelectionChanged += (_, __) => ActualizarAcciones();
            tabla.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && editar.Enabled) Editar(Seleccionado()); };
            filtro.TextChanged += (_, __) => AplicarFiltro();
            estados.SelectedIndexChanged += (_, __) => AplicarFiltro();
            nuevo.Click += (_, __) => Editar(null);
            editar.Click += (_, __) => Editar(Seleccionado());
            cambiar.Click += async (_, __) => await CambiarEstadoAsync();
            recargar.Click += async (_, __) => await CargarAsync();
            configurar.Click += (_, __) => abrirConfiguracion();
            ActualizarAcciones();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (inicioCarga) return;
            inicioCarga = true;
            await CargarAsync();
        }

        private Control BarraHerramientas()
        {
            var barra = new TableLayoutPanel { ColumnCount = 4, RowCount = 1, Margin = Padding.Empty };
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 103));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 101));
            var buscar = Tarjeta();
            buscar.Dock = DockStyle.Top;
            buscar.Height = 36;
            buscar.Margin = new Padding(0, 0, 9, 0);
            buscar.Padding = new Padding(10, 8, 10, 4);
            filtro.Dock = DockStyle.Top;
            filtro.AccessibleName = "Buscar en el catálogo";
            Pista(filtro, "Buscar en el catálogo…");
            buscar.Controls.Add(filtro);
            barra.Controls.Add(buscar, 0, 0);

            estados.Items.AddRange(new object[] { "Todos", "Activos", "Inactivos" });
            estados.SelectedIndex = 0;
            estados.AccessibleName = "Filtrar por estado";
            estados.Dock = DockStyle.Top;
            estados.Margin = new Padding(0, 5, 9, 0);
            barra.Controls.Add(estados, 1, 0);
            barra.Controls.Add(recargar, 2, 0);
            barra.Controls.Add(nuevo, 3, 0);
            return barra;
        }

        private Control AreaTabla()
        {
            var tarjeta = Tarjeta();
            tarjeta.Padding = new Padding(1);
            tabla.Dock = DockStyle.Fill;
            tarjeta.Controls.Add(tabla);
            var centro = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(21, 22, 14, 10)
            };
            tituloVacio.Margin = new Padding(0, 0, 0, 8);
            textoVacio.MaximumSize = new Size(450, 0);
            textoVacio.Margin = new Padding(0, 0, 0, 13);
            centro.Controls.Add(tituloVacio);
            centro.Controls.Add(textoVacio);
            centro.Controls.Add(configurar);
            vacio.Controls.Add(centro);
            tarjeta.Controls.Add(vacio);
            vacio.BringToFront();
            return tarjeta;
        }

        private Control PieCatalogo()
        {
            var panel = new Panel { Margin = Padding.Empty };
            mensaje.Location = new Point(1, 22);
            panel.Controls.Add(mensaje);
            var acciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, Width = 197, Padding = new Padding(0, 10, 0, 0),
                FlowDirection = FlowDirection.RightToLeft, WrapContents = false
            };
            editar.Margin = new Padding(0, 0, 8, 0);
            acciones.Controls.Add(cambiar);
            acciones.Controls.Add(editar);
            panel.Controls.Add(acciones);
            return panel;
        }

        private T? Seleccionado() => tabla.SelectedRows.Count == 1 ? tabla.SelectedRows[0].Tag as T : null;

        private async Task CargarAsync(string? confirmacion = null)
        {
            if (ocupado) return;
            ocupado = true;
            ActualizarAcciones();
            mensaje.Text = "Consultando SQL Server…";
            try
            {
                var datos = await servicio.ListarAsync();
                if (IsDisposed) return;
                registros = datos;
                disponible = true;
                AplicarFiltro();
                if (confirmacion != null) mensaje.Text = confirmacion;
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                registros.Clear();
                tabla.Rows.Clear();
                disponible = false;
                mensaje.Text = confirmacion == null ? "Sin datos de SQL Server" : confirmacion + " La lista no pudo recargarse.";
                MostrarVacio("No se pudo cargar el catálogo", ex.Message, ofrecerConexion: true);
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed) ActualizarAcciones();
            }
        }

        private void AplicarFiltro()
        {
            if (!disponible) return;
            tabla.Rows.Clear();
            string texto = filtro.Text.Trim();
            var comparar = CultureInfo.CurrentCulture.CompareInfo;
            foreach (var registro in registros)
            {
                if (estados.SelectedIndex == 1 && !registro.Activo || estados.SelectedIndex == 2 && registro.Activo) continue;
                var valores = campos.Select(campo => campo.Mostrar(registro)).ToArray();
                if (texto.Length > 0 && !valores.Any(v => comparar.IndexOf(v, texto, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)) continue;
                var fila = tabla.Rows[tabla.Rows.Add(valores.Cast<object>().Concat(new object[] { registro.Activo ? "Activo" : "Inactivo" }).ToArray())];
                fila.Tag = registro;
                var estado = fila.Cells[campos.Length];
                estado.Style.ForeColor = registro.Activo ? Verde : Acento;
                estado.Style.Font = Medio;
            }
            tabla.ClearSelection();
            vacio.Visible = false;
            tabla.Visible = true;
            if (tabla.Rows.Count == 0)
            {
                if (registros.Count == 0)
                    MostrarVacio("Este catálogo está vacío", $"Selecciona «Nuevo» para agregar un registro de {singular}.");
                else
                    MostrarVacio("Sin coincidencias", "Prueba otro término de búsqueda o cambia el filtro de estado.");
            }
            mensaje.Text = $"{tabla.Rows.Count} de {registros.Count} registros  ·  Selecciona una fila para editarla";
            ActualizarAcciones();
        }

        private void MostrarVacio(string titulo, string texto, bool ofrecerConexion = false)
        {
            tituloVacio.Text = titulo;
            textoVacio.Text = texto;
            configurar.Visible = ofrecerConexion;
            tabla.Visible = false;
            vacio.Visible = true;
        }

        private void ActualizarAcciones()
        {
            var registro = Seleccionado();
            recargar.Enabled = !ocupado;
            nuevo.Enabled = !ocupado && disponible;
            editar.Enabled = cambiar.Enabled = !ocupado && disponible && registro != null;
            cambiar.Text = registro?.Activo == false ? "Reactivar" : "Inhabilitar";
        }

        private async void Editar(T? registro)
        {
            using var editor = new EditorCatalogo<T>(singular, registro, campos, servicio);
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            await CargarAsync(registro == null ? "Registro creado correctamente." : "Cambios guardados.");
        }

        private async Task CambiarEstadoAsync()
        {
            var registro = Seleccionado();
            if (registro == null) return;
            bool activar = !registro.Activo;
            string accion = activar ? "reactivar" : "inhabilitar";
            var nombre = campos[0].Mostrar(registro);
            if (MessageBox.Show(this, $"¿Deseas {accion} «{nombre}»?", "Confirmar cambio de estado",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            ocupado = true;
            ActualizarAcciones();
            try
            {
                await servicio.CambiarEstadoAsync(registro, activar);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) MessageBox.Show(this, ex.Message, "Cambio de estado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                ocupado = false;
                if (!IsDisposed) ActualizarAcciones();
            }
            if (!IsDisposed) await CargarAsync(activar ? "Registro reactivado." : "Registro inhabilitado.");
        }
    }
}
