using System;
using System.Globalization;
using System.Windows.Forms;
using DulceLu.Entidad;
using DulceLu.LogicaNegocio;

namespace DulceLu.Presentacion
{
    internal sealed class CampoCatalogo<T>
    {
        public string Titulo { get; }
        public int Peso { get; }
        public Func<T, string> Mostrar { get; }
        public Func<T, Control> Entrada { get; }
        public Action<T, Control> Aplicar { get; }

        private CampoCatalogo(string titulo, int peso, Func<T, string> mostrar, Func<T, Control> entrada, Action<T, Control> aplicar)
        {
            Titulo = titulo; Peso = peso; Mostrar = mostrar; Entrada = entrada; Aplicar = aplicar;
        }

        public static CampoCatalogo<T> Texto(string titulo, int peso, int maximo, Func<T, string> leer,
            Action<T, string> escribir, string pista, bool obligatorio = true) =>
            new CampoCatalogo<T>(titulo, peso, leer, registro =>
            {
                var caja = new TextBox
                {
                    Text = leer(registro), MaxLength = maximo, BorderStyle = BorderStyle.FixedSingle,
                    Font = Diseno.Texto, AccessibleName = titulo
                };
                Diseno.Pista(caja, pista);
                return caja;
            }, (registro, caja) => escribir(registro, caja.Text))
            { Obligatorio = obligatorio };

        public static CampoCatalogo<T> Numero(string titulo, int peso, decimal minimo, decimal maximo, int decimales,
            Func<T, decimal> leer, Action<T, decimal> escribir, string? formato = null) =>
            new CampoCatalogo<T>(titulo, peso,
                registro => formato == null ? leer(registro).ToString(CultureInfo.CurrentCulture) : string.Format(formato, leer(registro)),
                registro => new NumericUpDown
                {
                    Font = Diseno.Texto, Minimum = minimo, Maximum = maximo, DecimalPlaces = decimales,
                    Value = Math.Max(minimo, Math.Min(maximo, leer(registro))), AccessibleName = titulo,
                    ThousandsSeparator = true, Increment = decimales == 0 ? 1 : 0.50m
                },
                (registro, caja) => escribir(registro, ((NumericUpDown)caja).Value));

        public bool Obligatorio { get; private set; } = true;
    }

    internal static class CatalogosVisuales
    {
        public static Control Productos(Catalogo<Producto> datos, Action configurar) =>
            new VistaCatalogo<Producto>("Productos", "producto", "Organiza la carta, los precios y las existencias de la cafetería.",
                datos, configurar,
                CampoCatalogo<Producto>.Texto("Código", 72, 24, p => p.Codigo, (p, v) => p.Codigo = v, "Ej. CAF-001"),
                CampoCatalogo<Producto>.Texto("Nombre", 182, 100, p => p.Nombre, (p, v) => p.Nombre = v, "Ej. Latte de vainilla"),
                CampoCatalogo<Producto>.Texto("Categoría", 95, 50, p => p.Categoria, (p, v) => p.Categoria = v, "Ej. Cafés"),
                CampoCatalogo<Producto>.Numero("Precio (S/)", 80, 0m, 99999999.99m, 2, p => p.Precio, (p, v) => p.Precio = v, "S/ {0:N2}"),
                CampoCatalogo<Producto>.Numero("Stock", 64, 0, int.MaxValue, 0, p => p.Stock, (p, v) => p.Stock = (int)v));

        public static Control Mesas(Catalogo<Mesa> datos, Action configurar) =>
            new VistaCatalogo<Mesa>("Mesas", "mesa", "Administra los espacios del salón y su capacidad de atención.",
                datos, configurar,
                CampoCatalogo<Mesa>.Texto("Código", 85, 16, m => m.Codigo, (m, v) => m.Codigo = v, "Ej. M-01"),
                CampoCatalogo<Mesa>.Texto("Zona", 165, 50, m => m.Zona, (m, v) => m.Zona = v, "Ej. Terraza"),
                CampoCatalogo<Mesa>.Numero("Capacidad", 115, 1, 30, 0, m => m.Capacidad, (m, v) => m.Capacidad = (int)v));

        public static Control Clientes(Catalogo<Cliente> datos, Action configurar) =>
            new VistaCatalogo<Cliente>("Clientes", "cliente", "Mantén al día los datos de contacto para las reservaciones.",
                datos, configurar,
                CampoCatalogo<Cliente>.Texto("Nombre o razón social", 145, 100, c => c.Nombre, (c, v) => c.Nombre = v, "Ej. Mariana Ríos"),
                CampoCatalogo<Cliente>.Texto("Documento", 80, 16, c => c.Documento, (c, v) => c.Documento = v, "DNI, RUC u otro", false),
                CampoCatalogo<Cliente>.Texto("Teléfono", 95, 24, c => c.Telefono, (c, v) => c.Telefono = v, "Ej. +51 987 654 321", false),
                CampoCatalogo<Cliente>.Texto("Correo", 155, 120, c => c.Correo, (c, v) => c.Correo = v, "Ej. hola@ejemplo.pe", false));

        public static Control Metodos(Catalogo<MetodoPago> datos, Action configurar) =>
            new VistaCatalogo<MetodoPago>("Métodos de pago", "método de pago", "Define los medios de pago que ofrece la cafetería.",
                datos, configurar,
                CampoCatalogo<MetodoPago>.Texto("Nombre", 123, 50, m => m.Nombre, (m, v) => m.Nombre = v, "Ej. Yape"),
                CampoCatalogo<MetodoPago>.Texto("Descripción", 247, 160, m => m.Descripcion, (m, v) => m.Descripcion = v, "Ej. Pago por QR", false));
    }
}
