using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using QuestPDF.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Entidades; // --> CAPA DONDE ESTAS LAS ENTIDADES
using Repositorio; // --> CAPA DE LAS CONSULTAS AL REPOSITORIO

namespace Negocio
{
    public class NProveedor
    {
        ReposProveedor oRProveedor = new ReposProveedor(); // --> para consultas al repositorio
        Proveedor oProveedor; // Para crear los proveedores
        List<Proveedor> Proveedores = new List<Proveedor>(); // Lista de proveedores a imprimir

        public DataTable ListaProveedores()
        {
            return oRProveedor.ListaProveedores();
        }

        public bool ProveedorExistente(string _CUIT, string _mail, string _telefono)
        {
            return oRProveedor.ProveedorExistente(_CUIT, _mail, _telefono);
        }

        public bool DatosDuplicados(int _proveedorID, string _CUIT, string _mail, string _telefono)
        {
            return oRProveedor.DatosDuplicados(_proveedorID, _CUIT, _mail, _telefono);
        }

        // Intenta crear un proveedor y lo pasa al Repositorio
        public bool AgregarProveedor(string _CUIT, string _razonSocial, string _mail, string _telefono, string _rubro, string _direccion)
        {
            try
            {
                oProveedor = new Proveedor()
                {
                    CUIT = _CUIT,
                    RazonSocial = _razonSocial,
                    Mail = _mail,
                    Telefono = _telefono,
                    Rubro = _rubro,
                    Direccion = _direccion
                };

                return oRProveedor.AgregarProveedor(oProveedor);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        // Intenta crear un proveedor y lo pasa al Repositorio
        public bool ModificarUsuario(int _proveedorID, string _razonSocial, string _mail, string _telefono, string _rubro, string _direccion)
        {
            try
            {
                oProveedor = new Proveedor()
                {
                    ProveedorID = _proveedorID,
                    RazonSocial = _razonSocial,
                    Mail = _mail,
                    Telefono = _telefono,
                    Rubro = _rubro,
                    Direccion = _direccion
                };

                return oRProveedor.ModificarProveedor(oProveedor);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        public bool EliminarProveedor(int _usuarioID)
        {
            return oRProveedor.EliminarProveedor(_usuarioID);
        }

        // Crea al proveedor y lo agrega a lista de impresión
        public bool AgregarImpimir(string _CUIT, string _razonSocial, string _mail, string _telefono, string _rubro, string _direccion)
        {
            try
            {
                Proveedores.Add(new Proveedor()
                {
                    CUIT = _CUIT,
                    RazonSocial = _razonSocial,
                    Mail = _mail,
                    Telefono = _telefono,
                    Rubro = _rubro,
                    Direccion = _direccion
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        // Elimina el proveedor de la lista de impresión
        public bool EliminarImprimir(int _indice)
        {
            try
            {
                Proveedores.RemoveAt(_indice);
                return true;
            }
            catch(Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        // Retorna la lista de impresión
        public List<Proveedor> VerLista()
        {
            return Proveedores;
        }

        // Crea el PDF de Proveedores
        public string ImprimirLista(string _path)
        {
            // No se agrego ningún proveedor
            if (Proveedores == null || Proveedores.Count == 0)
            {
                return "No se selecciono ningún proveedor";
            }

            string fecha_actul = DateTime.Now.ToString("dd-MM-yyyy");

            // QuestPDF.com para la documentación
            var pdf = Document.Create(doc =>
            {
                doc.Page(pag =>
                {
                    pag.Header().Padding(10).Row(header =>
                    {
                        Byte[] imagen = File.ReadAllBytes("C:\\Users\\TheWolfplay\\source\\repos\\Sistema_Supermercado\\Data\\Dragon.png");
                        header.ConstantItem(100).Image(imagen, ImageScaling.FitArea);
                        header.RelativeItem(50).Height(80).PaddingTop(20).Column(col =>
                        {
                            col.Item().AlignCenter().Text("Lista proveedores").FontSize(25).Bold().FontColor("FF0000");
                        });
                    });

                    pag.Content().Column(col =>
                    {
                        col.Item().PaddingLeft(10).Text(txt =>
                        {
                            txt.Span("Fecha:").FontSize(9).SemiBold();
                            txt.Span(fecha_actul).FontSize(9).SemiBold();
                        });

                        col.Item().LineHorizontal(0.8f).LineColor("D9D9D9");

                        col.Item().Table(tb =>
                        {
                            tb.ColumnsDefinition(column =>
                            {
                                column.RelativeColumn();
                                column.RelativeColumn();
                                column.RelativeColumn();
                                column.RelativeColumn();
                                column.RelativeColumn();
                            });

                            tb.Header(tbHeader =>
                            {
                                tbHeader.Cell().AlignCenter().Text("CUIT:").FontSize(11).FontColor(Colors.Black);
                                tbHeader.Cell().AlignCenter().Text("Razon Social:").FontSize(11).FontColor(Colors.Black);
                                tbHeader.Cell().AlignCenter().Text("Rubro:").FontSize(11).FontColor(Colors.Black);
                                tbHeader.Cell().AlignCenter().Text("Teléfono:").FontSize(11).FontColor(Colors.Black);
                                tbHeader.Cell().AlignCenter().Text("Mail:").FontSize(11).FontColor(Colors.Black);
                            });

                            foreach (var proveedor in Proveedores)
                            {
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignCenter().Text(proveedor.CUIT).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignCenter().Text(proveedor.RazonSocial).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignCenter().Text(proveedor.Rubro).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignCenter().Text(proveedor.Telefono).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignCenter().Text(proveedor.Mail).FontSize(10);
                            }
                        });
                    });
                });
            });

            try
            {
                pdf.GeneratePdf(_path);
                return "El  listado se imprimió con éxito";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        // Retorna la RPT del Repositorio
        public DataTable RealizarBusqueda(string _filtro, string _buscar)
        {
            return oRProveedor.Buscar(_filtro, _buscar);
        }
    }
}