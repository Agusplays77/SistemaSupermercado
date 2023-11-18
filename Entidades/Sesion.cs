namespace Entidades
{
    internal class Sesion
    {
        public int SesionID { get; set; }
        public Usuario oUsuarios { get; set; }
        public string Inicio { get; set; }
        public string Cierre { get; set; }
    }
}