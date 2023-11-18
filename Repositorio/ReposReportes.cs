using Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace Repositorio
{
    public class ReposReportes
    {
        public List<Venta> ListarReportes()
        {
            List<Venta> ListaReportes = new List<Venta>();

            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = "SELECT NroDocumento, MontoTotal, FechaCreacion FROM Ventas";
                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    cmd.CommandType = CommandType.Text;
                    oConexion.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            ListaReportes.Add(new Venta
                            {
                                NroDocumento = rd["NroDocumento"].ToString(),
                                MontoTotal = float.Parse(rd["MontoTotal"].ToString()),
                                FechaVenta = rd["FechaCreacion"].ToString()
                            });
                        }
                    }
                }
                catch
                {
                    ListaReportes = new List<Venta>();
                }
            }

            return ListaReportes;
        }

        public List<Venta> ListaReportesFiltrados(string _fechaInicio, string _fechaCierre)
        {
            List<Venta> ListaReportes = new List<Venta>();

            using (SqlConnection oConexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = "SELECT NroDocumento, MontoTotal, FechaCreacion FROM Ventas " +
                        "WHERE FechaCreacion >= @FechaInicio AND FechaCreacion <= @FechaCierre";

                    // If querying for the current day, modify the query
                    if (_fechaInicio == _fechaCierre)
                    {
                        query = "SELECT NroDocumento, MontoTotal, FechaCreacion FROM Ventas " +
                            "WHERE CAST(FechaCreacion AS DATE) = @FechaInicio";
                    }

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    cmd.CommandType = CommandType.Text;

                    if (_fechaInicio != _fechaCierre)
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", DateTime.Parse(_fechaInicio));
                        cmd.Parameters.AddWithValue("@FechaCierre", DateTime.Parse(_fechaCierre));
                    }
                    else
                    {
                        // When querying for the current day, use only one date parameter
                        cmd.Parameters.AddWithValue("@FechaInicio", DateTime.Parse(_fechaInicio));
                    }

                    oConexion.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            ListaReportes.Add(new Venta
                            {
                                NroDocumento = rd["NroDocumento"].ToString(),
                                MontoTotal = float.Parse(rd["MontoTotal"].ToString()),
                                FechaVenta = rd["FechaCreacion"].ToString()
                            });
                        }
                    }
                }
                catch
                {
                    ListaReportes = new List<Venta>();
                }
            }

            return ListaReportes;
        }
    }
}