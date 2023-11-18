using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Entidades; // --> CAPA DONDE ESTAS LAS ENTIDADES

namespace Repositorio
{
    public class ReposUsuario
    {
        public List<Usuario> ListaUsuarios()
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                List<Usuario> lista_usuarios = new List<Usuario>();

                try
                {
                    string querry = "SELECT UsuarioID, RolID, Documento, Nombre, Mail, Telefono, Clave FROM Usuarios WHERE Estado = 1";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    
                    cmd.CommandType = CommandType.Text;
                    oConexion.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista_usuarios.Add(new Usuario
                            {
                                UsuarioID = Convert.ToInt32(rd["UsuarioID"]),
                                oRol = new Rol { RolID = Convert.ToInt32(rd["RolID"]) },
                                Documento = Convert.ToInt32(rd["Documento"]),
                                Nombre = rd["Nombre"].ToString(),
                                Mail = rd["Mail"].ToString(),
                                Telefono = rd["Telefono"].ToString(),
                                Clave = rd["Clave"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    lista_usuarios = new List<Usuario>();
                }

                return lista_usuarios;
            }
        }

        public DataTable UsuariosCargados()
        {
            using(SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                DataTable dt = new DataTable();
                try
                {
                    string querry = "SELECT UsuarioID, RolID, Documento, Nombre, Mail, Telefono, Clave FROM Usuarios WHERE Estado = 1";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);

                    cmd.CommandType = CommandType.Text;
                    oConexion.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    dt = new DataTable();
                }
                return dt;
            }
        }

        public bool UsuarioExistente(int _documento, string _mail, string _telefono)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT COUNT(*) FROM Usuarios WHERE Documento = @Documento OR Mail = @Mail OR Telefono = @Telefono AND Estado = 1";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@Documento", _documento);
                    cmd.Parameters.AddWithValue("@Mail", _mail);
                    cmd.Parameters.AddWithValue("@Telefono", _telefono);
                    oConexion.Open();
                    int c = (int)cmd.ExecuteScalar();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    return (c > 0);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return true;
                }
            }
        }

        public bool DatosDuplicados(int _usuarioID, int _documento, string _mail, string _telefono)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT COUNT(*) FROM Usuarios WHERE (Documento = @Documento OR Mail = @Mail OR Telefono = @Telefono) AND Estado = 1 AND UsuarioID != @UsuarioID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@Documento", _documento);
                    cmd.Parameters.AddWithValue("@Mail", _mail);
                    cmd.Parameters.AddWithValue("@Telefono", _telefono);
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioID);
                    oConexion.Open();
                    int c = (int)cmd.ExecuteScalar();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    return (c > 0);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return true;
                }
            }
        }

        public bool AgregarUsuario(Usuario _usuario)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "INSERT INTO Usuarios (RolID, Documento, Nombre, Mail, Telefono, Clave, Estado)" +
                        "VALUES (@RolID, @Documento, @Nombre, @Mail, @Telefono, @Clave, @Estado)";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@RolID", _usuario.oRol.RolID);
                    cmd.Parameters.AddWithValue("@Documento", _usuario.Documento);
                    cmd.Parameters.AddWithValue("@Nombre", _usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Mail", _usuario.Mail);
                    cmd.Parameters.AddWithValue("@Telefono", _usuario.Telefono);
                    cmd.Parameters.AddWithValue("@Clave", _usuario.Clave);
                    cmd.Parameters.AddWithValue("@Estado", 1);
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return false;
                }
            }
        }

        public bool ModificarUsuario(Usuario _usuario)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "UPDATE Usuarios SET RolID = @RolID, Nombre = @Nombre, Mail = @Mail, Telefono = @Telefono, Clave = @Clave WHERE UsuarioID = @UsuarioID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuario.UsuarioID);
                    cmd.Parameters.AddWithValue("@RolID", _usuario.oRol.RolID);
                    cmd.Parameters.AddWithValue("@Nombre", _usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Mail", _usuario.Mail);
                    cmd.Parameters.AddWithValue("@Telefono", _usuario.Telefono);
                    cmd.Parameters.AddWithValue("@Clave", _usuario.Clave);
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return false;
                }
            }
        }

        public bool EliminarUsuario(int _usuarioID)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "UPDATE Usuarios SET Estado = @NuevoEstado WHERE UsuarioID = @UsuarioID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@NuevoEstado", 0);
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioID);
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return false;
                }
            }
        }

        public DataTable Buscar(string _filtro, string _buscar)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                DataTable dt = new DataTable();
                try
                {
                    string querry = "";

                    if (_filtro == "Descripcion")
                    {
                        querry = "SELECT U.* FROM Usuarios U INNER JOIN Roles R ON U.RolID = R.RolID WHERE R.Descripcion LIKE @Buscar AND U.Estado = 1";
                    }
                    else
                    {
                        querry = "SELECT * FROM Usuarios WHERE " + _filtro + " LIKE @Buscar AND Estado = 1";
                    }
                    SqlCommand cmd = new SqlCommand(querry, oConexion);

                    cmd.Parameters.AddWithValue("@buscar", "%" + _buscar + "%");
                    oConexion.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    dt = new DataTable();
                }
                return dt;
            }
        }
    }
}