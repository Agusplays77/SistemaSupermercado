using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Repositorio
{
    public class ReposProveedor
    {
        public DataTable ListaProveedores()
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                DataTable dt = new DataTable();
                try
                {
                    string querry = "SELECT ProveedorID, CUIT, RazonSocial, Mail, Telefono, Rubro, Direccion, FechaCreacion FROM Proveedores WHERE Estado = 1";
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

        public bool ProveedorExistente(string _CUIT, string _mail, string _telefono)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT COUNT(*) FROM Proveedores WHERE CUIT = @CUIT OR Mail = @Mail OR Telefono = @Telefono AND Estado = 1";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@CUIT", _CUIT);
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

        //Pregunta si otro proveedor tiene los mismos datos, excluye al proveedor que se modifica
        public bool DatosDuplicados(int _proveedorID, string _CUIT, string _mail, string _telefono)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT COUNT(*) FROM Proveedores WHERE (CUIT = @CUIT OR Mail = @Mail OR Telefono = @Telefono) AND Estado = 1 AND ProveedorID != @ProveedorID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@CUIT", _CUIT);
                    cmd.Parameters.AddWithValue("@Mail", _mail);
                    cmd.Parameters.AddWithValue("@Telefono", _telefono);
                    cmd.Parameters.AddWithValue("@ProveedorID", _proveedorID);
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

        public bool AgregarProveedor(Proveedor _proveedor)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "INSERT INTO Proveedores (CUIT, RazonSocial, Mail, Telefono, Estado, Rubro, Direccion)" +
                        "VALUES (@CUIT, @RazonSocial, @Mail, @Telefono, @Estado, @Rubro, @Direccion)";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@CUIT", _proveedor.CUIT);
                    cmd.Parameters.AddWithValue("@RazonSocial", _proveedor.RazonSocial);
                    cmd.Parameters.AddWithValue("@Mail", _proveedor.Mail);
                    cmd.Parameters.AddWithValue("@Telefono", _proveedor.Telefono);
                    cmd.Parameters.AddWithValue("@Estado", 1);
                    cmd.Parameters.AddWithValue("@Rubro", _proveedor.Rubro);
                    cmd.Parameters.AddWithValue("@Direccion", _proveedor.Direccion);
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

        public bool ModificarProveedor(Proveedor _proveedor)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "UPDATE Proveedores SET RazonSocial = @RazonSocial, Mail = @Mail, Telefono = @Telefono, Direccion = @Direccion, Rubro = @Rubro WHERE ProveedorID = @ProveedorID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@ProveedorID", _proveedor.ProveedorID);
                    cmd.Parameters.AddWithValue("@RazonSocial", _proveedor.RazonSocial);
                    cmd.Parameters.AddWithValue("@Mail", _proveedor.Mail);
                    cmd.Parameters.AddWithValue("@Telefono", _proveedor.Telefono);
                    cmd.Parameters.AddWithValue("@Direccion", _proveedor.Direccion);
                    cmd.Parameters.AddWithValue("@Rubro", _proveedor.Rubro);
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

        public bool EliminarProveedor(int _proveedorID)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "UPDATE Proveedores SET Estado = @NuevoEstado WHERE ProveedorID = @ProveedorID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@NuevoEstado", 0);
                    cmd.Parameters.AddWithValue("@ProveedorID", _proveedorID);
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
                    string querry = "SELECT * FROM Proveedores WHERE " + _filtro + " LIKE @Buscar AND Estado = 1";
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