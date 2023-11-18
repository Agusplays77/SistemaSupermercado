using System.Data.SqlClient;
using Entidades;

namespace Repositorio
{
    public class ReposSupermercado
    {
        public Supermercado DatosSupermercado()
        {
            Supermercado supermercado = new Supermercado();

            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = "SELECT * FROM Supermercado";
                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    oConexion.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            supermercado = new Supermercado
                            {
                                CUIT = rd["CUIT"].ToString(),
                                RazonSocial = rd["RazonSocial"].ToString(),
                                Direccion = rd["Direccion"].ToString()
                            };
                        }
                    }
                }
                catch
                {
                    supermercado = new Supermercado();
                }
                return supermercado;
            }
        }
    }
}