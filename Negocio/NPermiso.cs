using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using Repositorio;

namespace Negocio
{
    public class NPermiso
    {
        public ReposPermiso oRpermiso = new ReposPermiso();

        public List<Permiso> ListaPermisos(int _usuarioID)
        {
            return oRpermiso.ListaPremisos(_usuarioID);
        }

        public void RegistrarInicio(string _usuarioID)
        {
            oRpermiso.RegistrarInicio(_usuarioID);
        }

        public void RegistrarCierre(string _usuarioID)
        {
            oRpermiso.RegistrarCierre(_usuarioID);
        }
    }
}