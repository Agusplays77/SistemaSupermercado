using System;
using System.Data;
using System.Collections.Generic;
using Entidades;
using Repositorio;

namespace Negocio
{
    public class NUsuario
    {
        ReposUsuario oRUsuario = new ReposUsuario();
        Usuario oUsuario;

        public List<Usuario> ListaUsuarios()
        {
            return oRUsuario.ListaUsuarios();
        } // Lista para buscar el usuario y contraseñas en el LOGIN

        public DataTable UsuariosCargados()
        {
            return oRUsuario.UsuariosCargados();
        } // Retorna los usuarios cargados en la base de datos

        public bool UsuarioExistente(int _documento, string _mail, string _telefono)
        {
            return oRUsuario.UsuarioExistente(_documento, _mail, _telefono);
        } // Pregunta si existe otro usuario con los mismos datos

        public bool DatosDuplicados(int _usuarioID, int _documento, string _mail, string _telefono)
        {
            return oRUsuario.DatosDuplicados(_usuarioID, _documento, _mail, _telefono);
        } // Pregunta si los datos modificados son unicos

        public bool AgregarUsuario(int _rolID, string _documento, string _nombre, string _mail, string _telefono, string _clave)
        {
            try
            {
                oUsuario = new Usuario()
                {
                    oRol = new Rol { RolID = _rolID },
                    Documento = Convert.ToInt32(_documento),
                    Nombre = _nombre,
                    Mail = _mail,
                    Telefono = _telefono,
                    Clave = _clave
                };

                return oRUsuario.AgregarUsuario(oUsuario);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        public bool ModificarUsuario(int _usuarioID,int _rolID, string _nombre, string _mail, string _telefono, string _clave)
        {
            try
            {
                oUsuario = new Usuario()
                {
                    UsuarioID = _usuarioID,
                    oRol = new Rol { RolID = _rolID },
                    Nombre = _nombre,
                    Mail = _mail,
                    Telefono = _telefono,
                    Clave = _clave
                };

                return oRUsuario.ModificarUsuario(oUsuario);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        public bool EliminarUsuario(int _usuarioID)
        {
            return oRUsuario.EliminarUsuario(_usuarioID);
        }

        public DataTable RealizarBusqueda(string _filtro, string _buscar)
        {
            if (_filtro == "Rol") { _filtro = "Descripcion"; }
            return oRUsuario.Buscar(_filtro, _buscar);
        } // Retorna el resultado de la busqueda
    }
}