namespace Entidades
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public Rol oRol { get; set; }
        public int Documento { get; set; }
        public string Nombre { get; set; }
        public string Mail { get; set; }
        public string Telefono { get; set; }
        public string Clave { get; set; }
        public bool Estado { get; set; }
        public string FechaCreacion { get; set; }
    }
}
