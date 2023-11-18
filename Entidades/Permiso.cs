namespace Entidades
{
    public class Permiso
    {
        public int PermisoID { get; set; }
        public Rol oRolID { get; set; }
        public string NombreMenu { get; set; }
        public string FechaCreacion { get; set; }
    }
}