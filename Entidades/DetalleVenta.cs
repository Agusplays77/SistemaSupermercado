namespace Entidades
{
    public class DetalleVenta
    {
        public Producto oProducto { get; set; }
        public float PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public float SubTotal { get; set; }
    }
}