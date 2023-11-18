using Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Repositorio
{
    public class ReposVender
    {
        public Producto ConsultaDatosProducto(string _EAN13)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string querry = "SELECT ProductoID, EAN13, Nombre, Descripcion, Stock, PrecioVenta FROM Productos WHERE EAN13 = @EAN13";
                    SqlCommand cmd = new SqlCommand(querry, oConexion);

                    cmd.Parameters.AddWithValue("@EAN13", _EAN13);
                    oConexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Producto oProducto = new Producto
                            {
                                ProductoID = int.Parse(reader["ProductoID"].ToString()),
                                EAN13 = long.Parse(reader["EAN13"].ToString()),
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Stock = int.Parse(reader["Stock"].ToString()),
                                PrecioVenta = float.Parse(reader["PrecioVenta"].ToString()),
                            };
                            return oProducto;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }

        public string GenerarNroDocumento()
        {
            string nro_documento = ObtenerNumeroVenta();

            if (nro_documento != null)
            {
                string[] dividir = nro_documento.Split('-');
                string numero_serie = dividir[0].Trim();
                string numero_venta = dividir[1].Trim();
                int serie = int.Parse(numero_serie);
                int venta = int.Parse(numero_venta);

                bool actualizar_nro_serie = numero_venta.All(n => n == '9');

                if (actualizar_nro_serie)
                {
                    serie++;
                    venta = 0;
                }

                venta++;

                int longitud_numero_venta = numero_venta.Length;
                string formato_numero_serie = new string('0', longitud_numero_venta - 1);
                string formato_numero_venta = new string('0', longitud_numero_venta);
                string numero_serie_actualizado = serie.ToString(formato_numero_serie);
                string numero_venta_actualizado = venta.ToString(formato_numero_venta);
                string nuevo_nro_documento = $"{numero_serie_actualizado}-{numero_venta_actualizado}";
                return nuevo_nro_documento;
            }
            else
            {
                return null;
            }
        }

        private string ObtenerNumeroVenta()
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                string NroDocumento = "0001-00000";

                try
                {
                    // Obtener el último VentaID
                    string querry = "SELECT TOP 1 VentaID FROM Ventas ORDER BY VentaID DESC";
                    oConexion.Open();
                    SqlCommand cmd = new SqlCommand(querry, oConexion);
                    var ventaIDResultado = cmd.ExecuteScalar();

                    if (ventaIDResultado != null)
                    {
                        int ventaID = (int)ventaIDResultado;

                        // Obtener el NroDocumento asociado al último VentaID
                        string docQuery = "SELECT TOP 1 NroDocumento FROM Ventas WHERE VentaID = @VentaID";
                        cmd.CommandText = docQuery;
                        cmd.Parameters.AddWithValue("@VentaID", ventaID);
                        var nroDocumentoResultado = cmd.ExecuteScalar();

                        if (nroDocumentoResultado != null)
                        {
                            NroDocumento = nroDocumentoResultado.ToString();
                        }
                    }

                    return NroDocumento;
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool RegistrarVenta(Venta _carrito)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                SqlTransaction transaction = null;
                int ventaID;

                try
                {
                    oConexion.Open();
                    transaction = oConexion.BeginTransaction();

                    // Primero, inserta los datos generales de la venta en la tabla Ventas
                    string ventaQuery = "INSERT INTO Ventas (UsuarioID, TipoDocumento, NroDocumento, DocumentoCliente, MontoPago, MontoVuelto, MontoTotal) " +
                        "OUTPUT INSERTED.VentaID " +
                        "VALUES (@UsuarioID, @TipoDocumento, @NroDocumento, @DocumentoCliente, @MontoPago, @MontoVuelto, @MontoTotal)";
                    SqlCommand cmd = new SqlCommand(ventaQuery, oConexion, transaction);
                    cmd.Parameters.AddWithValue("@UsuarioID", _carrito.oUsuario.UsuarioID);
                    cmd.Parameters.AddWithValue("@TipoDocumento", _carrito.TipoDocumento);
                    cmd.Parameters.AddWithValue("@NroDocumento", _carrito.NroDocumento);
                    cmd.Parameters.AddWithValue("@DocumentoCliente", _carrito.DocumentoCliente);
                    cmd.Parameters.AddWithValue("@MontoPago", _carrito.MontoPago);
                    cmd.Parameters.AddWithValue("@MontoVuelto", _carrito.MontoVuelto);
                    cmd.Parameters.AddWithValue("@MontoTotal", _carrito.MontoTotal);
                    ventaID = (int)cmd.ExecuteScalar();  // Usar cmd para obtener el valor

                    // Luego, inserta los detalles de la venta en la tabla DetalleVentas
                    foreach (DetalleVenta detalle in _carrito.Detalles)
                    {
                        string detalle_query = "INSERT INTO DetalleVentas (VentaID, ProductoID, PrecioVenta, Cantidad, SubTotal) " +
                            "VALUES (@VentaID, @ProductoID, @PrecioVenta, @Cantidad, @SubTotal)";
                        SqlCommand detalleCmd = new SqlCommand(detalle_query, oConexion, transaction);
                        detalleCmd.Parameters.AddWithValue("@VentaID", ventaID);
                        detalleCmd.Parameters.AddWithValue("@ProductoID", detalle.oProducto.ProductoID);
                        detalleCmd.Parameters.AddWithValue("@PrecioVenta", detalle.PrecioVenta);
                        detalleCmd.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                        detalleCmd.Parameters.AddWithValue("@SubTotal", detalle.SubTotal);
                        detalleCmd.ExecuteNonQuery();
                    }

                    // Confirma la transacción
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    return false;
                }
            }
        }

        public bool ActualizarStock(List<DetalleVenta> _detalles)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    foreach (var detalle in _detalles)
                    {
                        string querry = "UPDATE Productos SET Stock = Stock - @Stock WHERE EAN13 = @EAN13";
                        SqlCommand cmd = new SqlCommand(querry, oConexion);

                        cmd.Parameters.AddWithValue("@Stock", detalle.Cantidad);
                        cmd.Parameters.AddWithValue("@EAN13", detalle.oProducto.EAN13);
                        oConexion.Open();
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}