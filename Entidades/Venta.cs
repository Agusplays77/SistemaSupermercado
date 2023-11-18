using System.Collections.Generic;

namespace Entidades
{
    public class Venta
    {
        public int VentaID { get; set; }
        public Usuario oUsuario { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public int DocumentoCliente { get; set; }
        public float MontoPago { get; set; }
        public float MontoVuelto { get; set; }
        public float MontoTotal { get; set; }
        public List<DetalleVenta> Detalles { get; set; }
        public string FechaVenta { get; set; }

        public Venta()
        {
            Detalles = new List<DetalleVenta>();
        }
    }
}