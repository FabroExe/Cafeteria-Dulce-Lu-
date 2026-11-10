using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DulceLu.Entidad;
using DulceLu.LogicaNegocio;
using static DulceLu.Presentacion.Diseno;

namespace DulceLu.Presentacion
{
    internal sealed class EditorCatalogo<T> : Form where T : Registro, new()
    {
        private readonly T borrador;
        private readonly Catalogo<T> servicio;
        private readonly List<(CampoCatalogo<T> Campo, Control Entrada)> entradas = new List<(CampoCatalogo<T>, Control)>();
        private readonly Label aviso = Etiqueta("", Peque, Acento);
        private readonly Button guardar = Boton("Guardar", principal: true);
        private readonly Button cancelar = Boton("Cancelar", ancho: 102);
        private bool guardando;

        public EditorCatalogo(string nombre, T? origen, CampoCatalogo<T>[] campos, Catalogo<T> servicio)
        {
            this.servicio = servicio;
            borrador = origen == null ? new T() : new T { Id = origen.Id, Version = origen.Version, Activo = origen.Activo };
            Text = origen == null ? "Crear " + nombre : "Editar " + nombre;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            AutoScaleMode = AutoScaleMode.Dpi;
            MaximizeBox = MinimizeBox = ShowInTaskbar = false;
            BackColor = Fondo;
            Font = Diseno.Texto;
            ClientSize = new Size(453, 197 + campos.Length * 58);
            var contenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(21, 17, 21, 17) };
            Controls.Add(contenedor);
            var filas = Filas();
            contenedor.Controls.Add(filas);
            AgregarFila(filas, Etiqueta("MANTENEDOR  /  FICHA", Micro, Acento), SizeType.Absolute, 20);
            AgregarFila(filas, Etiqueta(Text, Subtitulo), SizeType.Absolute, 34);
            AgregarFila(filas, Etiqueta("Los campos con * son obligatorios.", Peque, Suave), SizeType.Absolute, 27);
            foreach (var campo in campos)
            {
                AgregarFila(filas, Etiqueta(campo.Titulo + (campo.Obligatorio ? " *" : ""), Medio), SizeType.Absolute, 23);
                var entrada = campo.Entrada(origen ?? borrador);
                entradas.Add((campo, entrada));
                entrada.Margin = new Padding(0, 0, 0, 7);
                AgregarFila(filas, entrada, SizeType.Absolute, 35);
            }
            aviso.AutoSize = false;
            AgregarFila(filas, aviso, SizeType.Percent, 100);
            var botones = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Margin = Padding.Empty };
            botones.Controls.Add(guardar);
            cancelar.Margin = new Padding(0, 0, 8, 0);
            cancelar.DialogResult = DialogResult.Cancel;
            botones.Controls.Add(cancelar);
            AgregarFila(filas, botones, SizeType.Absolute, 38);
            AcceptButton = guardar;
            CancelButton = cancelar;
            guardar.Click += async (_, __) =>
            {
                if (guardando) return;
                guardando = true;
                guardar.Enabled = cancelar.Enabled = false;
                foreach (var campo in entradas) campo.Entrada.Enabled = false;
                aviso.Text = "Guardando en SQL Server…";
                try
                {
                    foreach (var campo in entradas) campo.Campo.Aplicar(borrador, campo.Entrada);
                    await this.servicio.GuardarAsync(borrador);
                    guardando = false;
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    aviso.Text = ex.Message;
                }
                finally
                {
                    guardando = false;
                    guardar.Enabled = cancelar.Enabled = true;
                    foreach (var campo in entradas) campo.Entrada.Enabled = true;
                }
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (guardando) e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
