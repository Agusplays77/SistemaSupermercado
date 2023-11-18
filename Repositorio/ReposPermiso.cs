using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Entidades;

namespace Repositorio
{
    public class ReposPermiso
    {
        public List<Permiso> ListaPremisos(int _usuarioID)
        {
            List<Permiso> permisos = new List<Permiso>();

            try
            {
                using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
                {
                    string querry = "select p.RolID, p.NombreMenu from Permisos p " +
                        "inner join Roles r on r.RolID = p.RolID " +
                        "inner join Usuarios u on u.RolID = r.RolID " +
                        "where u.UsuarioID = @UsuarioID ";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    oConexion.Open();
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioID);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            permisos.Add(new Permiso
                            {
                                oRolID = new Rol() { RolID = Convert.ToInt32((rd["RolID"])) },
                                NombreMenu = rd["NombreMenu"].ToString()
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                permisos = new List<Permiso>();
            }
            return permisos;
        }

        public void RegistrarInicio(string _usuarioID)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                string query = "insert into Sesiones (UsuarioID) values (@UsuarioId)";
                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@UsuarioID", _usuarioID);
                oConexion.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void RegistrarCierre(string _usuarioID)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = "select top 1 SesionID from Sesiones where UsuarioID = @UsuarioID ORDER BY Inicio DESC";
                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioID);
                    oConexion.Open();

                    object sesionID = cmd.ExecuteScalar();

                    if (sesionID != null)
                    {
                        string newQuery = "update Sesiones set Cierre = getdate() where SesionID = @SesionID";
                        SqlCommand updateCmd = new SqlCommand(newQuery, oConexion);
                        updateCmd.Parameters.AddWithValue("@SesionID", (int)sesionID);
                        updateCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }
    }
}