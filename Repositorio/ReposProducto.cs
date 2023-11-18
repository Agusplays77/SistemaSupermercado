using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Repositorio
{
    public class ReposProducto
    {
        public DataTable ListaProductos()
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                DataTable dt = new DataTable();
                try
                {
                    string querry = "SELECT P.ProductoID, P.EAN13, P.Nombre, P.Descripcion, P.Stock, P.PrecioCompra, P.PrecioVenta, P.FechaRegistro, p.ProveedorID, PR.RazonSocial " +
                           "FROM Productos P " +
                           "JOIN Proveedores PR ON P.ProveedorID = PR.ProveedorID " +
                           "WHERE P.Estado = 1";
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

        public bool ProductoExistente(string _EAN13)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT COUNT(*) FROM Productos WHERE (EAN13 = @EAN13) AND Estado = 1";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@EAN13", _EAN13);
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

        public int ProveedorExiste(string _CUIT)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT ProveedorID FROM Proveedores WHERE CUIT = @CUIT";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);

                    cmd.Parameters.AddWithValue("@CUIT", _CUIT);
                    cmd.CommandType = CommandType.Text;
                    oConexion.Open();

                    object result = cmd.ExecuteScalar();

                    // Si se encontró un proveedor, se devuelve su ID, de lo contrario, se devuelve -1.
                    return result != null ? (int)result : -1;
                }
                catch (Exception ex)
                {
                    // Manejo de la excepción
                    Console.WriteLine(ex.ToString());
                    return -1; // Devuelve -1 en caso de error
                }
            }
        }

        public bool DatosDuplicados(int _productoID, string _EAN13)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT COUNT(*) FROM Productos WHERE (EAN13 = @EAN13) AND Estado = 1 AND ProductoID != @ProductoID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);

                    cmd.Parameters.AddWithValue("@ProductoID", _productoID);
                    cmd.Parameters.AddWithValue("@EAN13", _EAN13);
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

        public bool AgregarProducto(Producto _producto)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "INSERT INTO Productos (EAN13, Nombre, Descripcion, Stock, Estado, PrecioCompra, PrecioVenta, ProveedorID) " +
                        "VALUES (@EAN13D, @Nombre, @Descripcion, @Stock, @Estado, @PrecioCompra, @PrecioVenta, @ProveedorID)";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@EAN13D", _producto.EAN13);
                    cmd.Parameters.AddWithValue("@Nombre", _producto.Nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", _producto.Descripcion);
                    cmd.Parameters.AddWithValue("@Stock", _producto.Stock);
                    cmd.Parameters.AddWithValue("@Estado", 1);
                    cmd.Parameters.AddWithValue("@PrecioCompra", _producto.PrecioCompra);
                    cmd.Parameters.AddWithValue("@PrecioVenta", _producto.PrecioVenta);
                    cmd.Parameters.AddWithValue("@ProveedorID", _producto.ProveedorID);
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

        public bool ModificarProducto(Producto _producto)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "UPDATE Productos SET Nombre = @Nombre, Descripcion = @Descripcion, Stock = @Stock, PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta WHERE ProductoID = @ProductoID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@ProductoID", _producto.ProductoID);
                    cmd.Parameters.AddWithValue("@Nombre", _producto.Nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", _producto.Descripcion);
                    cmd.Parameters.AddWithValue("@Stock", _producto.Stock);
                    cmd.Parameters.AddWithValue("@PrecioCompra", _producto.PrecioCompra);
                    cmd.Parameters.AddWithValue("@PrecioVenta", _producto.PrecioVenta);
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

        public bool EliminarProducto(int _productoID)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "UPDATE Productos SET Estado = @NuevoEstado WHERE ProductoID = @ProductoID";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    cmd.Parameters.AddWithValue("@ProductoID", _productoID);
                    cmd.Parameters.AddWithValue("@NuevoEstado", 0);
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
                    string querry = "SELECT * FROM Productos WHERE " + _filtro + " LIKE @Buscar AND Estado = 1";
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