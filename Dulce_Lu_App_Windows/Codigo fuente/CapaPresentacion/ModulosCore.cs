using System;

namespace DulceLu.Presentacion
{
    internal sealed class PestañaCore
    {
        public string Titulo { get; }
        public string[] Columnas { get; }
        public string[][] Filas { get; }
        public PestañaCore(string titulo, string columnas, params string[] filas)
        {
            Titulo = titulo;
            Columnas = columnas.Split('|');
            Filas = Array.ConvertAll(filas, fila => fila.Split('|'));
        }
    }

    internal sealed class AccionCore
    {
        public string Nombre { get; }
        public string[] Campos { get; }
        public AccionCore(string nombre, params string[] campos)
        {
            Nombre = nombre;
            Campos = campos;
        }
    }

    internal sealed class ModuloCore
    {
        public string Clave { get; }
        public string Titulo { get; }
        public string Rol { get; }
        public string Corta { get; }
        public string Descripcion { get; }
        public string Nota { get; }
        public PestañaCore[] Pestañas { get; }
        public AccionCore[] Acciones { get; }

        private ModuloCore(string clave, string titulo, string rol, string corta, string descripcion, string nota,
            PestañaCore[] pestañas, params AccionCore[] acciones)
        {
            Clave = clave; Titulo = titulo; Rol = rol; Corta = corta; Descripcion = descripcion;
            Nota = nota; Pestañas = pestañas; Acciones = acciones;
        }

        public static readonly ModuloCore[] Todos =
        {
            new ModuloCore("pedidos", "Pedidos", "01  /  SALÓN", "Toma pedidos y organiza las comandas.",
                "Registro de pedidos, disponibilidad de la carta y envío de comandas a cocina.",
                "El mozo toma el pedido, revisa la disponibilidad y asigna una mesa antes de enviar la comanda.",
                new[]
                {
                    new PestañaCore("Pedidos", "Pedido|Mesa|Mozo|Total|Estado",
                        "PED-018|M-02 · Ventanal|Ana|S/ 39.00|En preparación",
                        "PED-019|M-01 · Salón|Diego|S/ 23.50|Listo",
                        "PED-020|M-05 · Terraza|Ana|S/ 54.00|Pendiente"),
                    new PestañaCore("Estado de mesas", "Mesa|Zona|Personas|Estado",
                        "M-01|Salón|2|Ocupada", "M-02|Ventanal|4|Ocupada", "M-03|Terraza|6|Disponible")
                },
                new AccionCore("Registrar pedido", "Mesa|M-02 · Ventanal|M-03 · Terraza", "Producto|Latte de vainilla|Torta de chocolate",
                    "Cantidad", "Observaciones"),
                new AccionCore("Enviar comanda", "Pedido|PED-018|PED-019|PED-020", "Nota para cocina"),
                new AccionCore("Ver disponibilidad", "Producto|Latte de vainilla · disponible|Torta de chocolate · disponible")),

            new ModuloCore("reservas", "Reservaciones", "02  /  RECEPCIÓN", "Revisa horarios, aforo y mesas disponibles.",
                "Agenda del salón y asignación de mesas por horario y número de personas.",
                "Recepción verifica la capacidad y sugiere otra mesa u horario si el cliente lo necesita.",
                new[]
                {
                    new PestañaCore("Reservaciones", "Hora|Cliente|Mesa|Personas|Estado",
                        "15:30|Mariana Ríos|M-02|4|Confirmada", "16:00|Álvaro Paredes|M-03|6|Pendiente", "17:30|Camila Vega|M-01|2|Confirmada"),
                    new PestañaCore("Mesas disponibles", "Mesa|Zona|Capacidad|Hora libre",
                        "M-01|Salón|2 personas|16:00", "M-03|Terraza|6 personas|15:30", "M-04|Ventanal|4 personas|17:00")
                },
                new AccionCore("Nueva reservación", "Cliente", "Fecha y hora", "Personas", "Mesa|M-01 · 2 personas|M-02 · 4 personas|M-03 · 6 personas"),
                new AccionCore("Sugerir horario", "Mesa alternativa|M-03 · Terraza|M-04 · Ventanal", "Hora alternativa"),
                new AccionCore("Cancelar reserva", "Reservación|Mariana Ríos · 15:30|Álvaro Paredes · 16:00", "Motivo")),

            new ModuloCore("ventas", "Ventas y caja", "03  /  CAJA", "Consulta ventas y comprobantes.",
                "Registro de ingresos por venta, medios de pago y comprobantes electrónicos.",
                "En caja se calcula el total, se registra el pago, se descuenta stock y se emite el comprobante para SUNAT.",
                new[]
                {
                    new PestañaCore("Ventas", "Venta|Hora|Método de pago|Total|Estado",
                        "V-0041|10:04|Yape|S/ 25.50|Pagada", "V-0042|10:22|Tarjeta|S/ 43.00|Pagada", "V-0043|10:48|Efectivo|S/ 17.50|Pagada"),
                    new PestañaCore("Comprobantes", "Número|Venta|Tipo|Importe|Estado",
                        "B001-0041|V-0041|Boleta|S/ 25.50|Validado", "B001-0042|V-0042|Boleta|S/ 43.00|Pendiente", "F001-0043|V-0043|Factura|S/ 17.50|Validado")
                },
                new AccionCore("Registrar venta", "Pedido|PED-019|PED-020", "Medio de pago|Efectivo|Tarjeta|Yape", "Total (S/)"),
                new AccionCore("Emitir comprobante", "Venta|V-0041|V-0042|V-0043", "Tipo|Boleta|Factura", "Documento del cliente"),
                new AccionCore("Consultar por fechas", "Desde", "Hasta")),

            new ModuloCore("cocina", "Cocina", "04  /  PREPARACIÓN", "Revisa comandas e insumos en cocina.",
                "Órdenes de preparación, revisión de insumos y aviso al mozo.",
                "Cocina revisa los insumos, registra incidencias y avisa al mozo cuando el pedido esté listo.",
                new[]
                {
                    new PestañaCore("Órdenes de cocina", "Orden|Mesa|Preparación|Entrada|Estado",
                        "COM-018|M-02|2 lattes · 1 croissant|10:20|En preparación",
                        "COM-019|M-01|1 torta · 1 americano|10:24|Listo",
                        "COM-020|M-05|3 capuchinos|10:29|Pendiente"),
                    new PestañaCore("Insumos", "Insumo|Disponible|Unidad|Estado",
                        "Leche fresca|6|Litros|Disponible", "Café molido|1.2|Kg|Disponible", "Almendras|0|Kg|Faltante")
                },
                new AccionCore("Actualizar orden", "Orden|COM-018|COM-019|COM-020", "Estado|En preparación|Listo|Pendiente", "Fecha", "Nota para el mozo"),
                new AccionCore("Registrar reproceso", "Orden|COM-018|COM-019|COM-020", "Observaciones"),
                new AccionCore("Avisar falta de insumo", "Orden|COM-018|COM-019|COM-020", "Insumo faltante")),

            new ModuloCore("produccion", "Producción", "05  /  PLANIFICACIÓN", "Organiza faltantes y próximas tandas.",
                "Planificación de tandas y seguimiento de faltantes de cocina.",
                "Producción revisa existencias y ajusta cantidades antes de comunicar el plan a Administración.",
                new[]
                {
                    new PestañaCore("Planes", "Plan|Período|Producto|Unid.|Estado",
                        "PL-021|Esta semana|Croissant de almendras|24|Borrador",
                        "PL-020|Esta semana|Torta de chocolate|8|Comunicado",
                        "PL-019|Anterior|Pan de plátano|12|Cerrado"),
                    new PestañaCore("Faltantes", "Reporte|Insumo|Cantidad|Estado",
                        "F-011|Almendras|1 kg|Pendiente", "F-010|Cacao|0.5 kg|Atendido"),
                    new PestañaCore("Existencias", "Artículo|Existencias|Tipo",
                        "Torta de chocolate|8 und.|Producto", "Croissant de almendras|12 und.|Producto", "Almendras|0 kg|Insumo")
                },
                new AccionCore("Nuevo plan", "Período", "Producto", "Cantidad planificada", "Observaciones"),
                new AccionCore("Reportar faltante", "Insumo", "Cantidad faltante", "Observaciones"),
                new AccionCore("Comunicar plan", "Plan|PL-021|PL-020", "Destinatario|Administración", "Nota"))
        };
    }
}
