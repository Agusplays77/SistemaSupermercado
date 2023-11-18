namespace Entidades
{
    public class Producto
    {
        public int ProductoID { get; set; }
        public long EAN13 { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Stock { get; set; }
        public float PrecioCompra { get; set; }
        public float PrecioVenta { get; set; }
        public string FechaRegistro { get; set; }
        public int ProveedorID { get; set; }
        public string Proveedor { get; set; }
    }
}