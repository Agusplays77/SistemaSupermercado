using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Entidades;
using Repositorio;

namespace Negocio
{
    public class NProducto
    {
        private ReposProducto oRProducto = new ReposProducto();
        private List<Producto> Productos = new List<Producto>();
        private Producto oProducto;

        public DataTable ListaProveedores()
        {
            return oRProducto.ListaProductos();
        }

        public bool ProductoExistente(string _EAN13)
        {
            return oRProducto.ProductoExistente(_EAN13);
        }

        public int IDProveedor(string _CUIT)
        {
            return oRProducto.ProveedorExiste(_CUIT);
        }

        public bool DatosDuplicados(int _productoID, string _EAN13)
        {
            return oRProducto.DatosDuplicados( _productoID, _EAN13);
        }

        public bool AgregarProducto(string _EAN13, string _nombre, string _descripcion, int _stock, float _precioCompra, float _precioVenta, int _proveedorID)
        {
            try
            {
                oProducto = new Producto()
                {
                    EAN13 = Convert.ToInt64(_EAN13),
                    Nombre = _nombre,
                    Descripcion = _descripcion,
                    Stock = _stock,
                    PrecioCompra = _precioCompra,
                    PrecioVenta = _precioVenta,
                    ProveedorID = _proveedorID
                };

                return oRProducto.AgregarProducto(oProducto);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        public bool ModificarProducto(int _productoID, string _EAN13, string _nombre, string _descripcion, int _stock, float _precioCompra, float _precioVenta)
        {
            try
            {
                oProducto = new Producto()
                {
                    ProductoID = _productoID,
                    EAN13 = Convert.ToInt64(_EAN13),
                    Nombre = _nombre,
                    Descripcion = _descripcion,
                    Stock = _stock,
                    PrecioCompra = _precioCompra,
                    PrecioVenta = _precioVenta
                };

                return oRProducto.ModificarProducto(oProducto);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        public bool EliminarProducto(int _productoID)
        {
            return oRProducto.EliminarProducto(_productoID);
        }

        public bool AgregarImprimir(string _EAN13, string _nombre, string _descripcion, string _stock, string _precioCompra, string _proveedor)
        {
            try
            {
                Productos.Add(new Producto()
                {
                    EAN13 = long.Parse(_EAN13),
                    Nombre = _nombre,
                    Descripcion = _descripcion,
                    Stock = int.Parse(_stock),
                    PrecioCompra = float.Parse(_precioCompra),
                    Proveedor = _proveedor
                });
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }
        
        public bool EliminarImprimir(int _indice)
        {
            try
            {
                Productos.RemoveAt(_indice);
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }

        public List<Producto> VerListaImpresion()
        {
            return Productos;
        }
        
        public string ImprimirLista(string _path)
        {
            string fecha_actul = DateTime.Now.ToString("dd-MM-yyyy");

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
                            col.Item().AlignCenter().Text("Lista de productos").FontSize(25).Bold().FontColor("FF0000");
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
                                column.RelativeColumn();
                            });

                            tb.Header(tbHeader =>
                            {
                                tbHeader.Cell().AlignLeft().Text("EAN13:").FontSize(10).FontColor(Colors.Black);
                                tbHeader.Cell().AlignLeft().Text("Nombre:").FontSize(10).FontColor(Colors.Black);
                                tbHeader.Cell().AlignLeft().Text("Detalle:").FontSize(10).FontColor(Colors.Black);
                                tbHeader.Cell().AlignLeft().Text("Precio compra:").FontSize(10).FontColor(Colors.Black);
                                tbHeader.Cell().AlignLeft().Text("Stock:").FontSize(10).FontColor(Colors.Black);
                                tbHeader.Cell().AlignLeft().Text("Proveedor:").FontSize(10).FontColor(Colors.Black);
                            });

                            foreach (var producto in Productos)
                            {
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignLeft().Text(producto.EAN13.ToString()).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignLeft().Text(producto.Nombre).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignLeft().Text(producto.Descripcion).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignLeft().Text(producto.PrecioCompra.ToString()).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignLeft().Text(producto.Stock.ToString()).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignLeft().Text(producto.Proveedor).FontSize(10);
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
        
        public DataTable RealizarBusqueda(string _filtro, string _buscar)
        {
            return oRProducto.Buscar(_filtro, _buscar);
        }
    }
}