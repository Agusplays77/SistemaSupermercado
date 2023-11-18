namespace Entidades
{
    public class Proveedor
    {
        public int ProveedorID { get; set; }
        public string CUIT { get; set; }
        public string RazonSocial { get; set; }
        public string Mail { get; set; }
        public string Telefono { get; set; }
        public bool Estado { get; set; }
        public string Rubro { get; set; }
        public string Direccion { get; set; }
        public string FechaCreacion { get; set; }
    }
}