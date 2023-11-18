using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Entidades;
using Repositorio;

namespace Negocio
{
    public class NVender
    {
        private ReposVender oRVender = new ReposVender();
        private ReposSupermercado cnSupermercado = new ReposSupermercado();
        private List<Producto> Productos = new List<Producto>();
        private List<DetalleVenta> Detalles = new List<DetalleVenta>();
        private Venta oCarrito = new Venta();
        private Supermercado supermercado;
        private Producto oProducto;
        private DetalleVenta oDetalle;
        private string path_logo = "C:\\Users\\TheWolfplay\\source\\repos\\Sistema_Supermercado\\Data\\Dragon.png";

        public bool AgrgegarProducto(string _EAN13, string _cantidad)
        {
            oProducto = new Producto();
            oProducto = oRVender.ConsultaDatosProducto(_EAN13);
            if (oProducto != null)
            {
                Productos.Add(oProducto);
                int cantidad = int.Parse(_cantidad);
                CrearDetalleVenta(oProducto, cantidad);
                return true;
            }
            return false;
        }

        public void EliminarProducto(int _indice)
        {
            Productos.RemoveAt(_indice);
            Detalles.RemoveAt(_indice);
        }

        public void ModificarProducto(int _indice, int _nuevaCantidad)
        {
            Detalles[_indice].Cantidad = _nuevaCantidad;
            Detalles[_indice].SubTotal = (Detalles[_indice].PrecioVenta * _nuevaCantidad);
        }

        public void CrearDetalleVenta(Producto _producto, int _cantidad)
        {
            oDetalle = new DetalleVenta()
            {
                oProducto = _producto,
                Cantidad = _cantidad,
                PrecioVenta = _producto.PrecioVenta,
                SubTotal = (_cantidad * _producto.PrecioVenta)
            };

            Detalles.Add(oDetalle);
        }

        public List<DetalleVenta> ConsultarLista()
        {
            return Detalles;
        }

        private bool ArmarCarrito(Usuario _usuario, int _documentoCliente, float _total, float _pago, float _vuelto, string _nroDocumento)
        {
            try
            {
                oCarrito.oUsuario = _usuario;
                oCarrito.DocumentoCliente = _documentoCliente;
                oCarrito.NroDocumento = _nroDocumento;
                oCarrito.TipoDocumento = "Factura";
                oCarrito.Detalles = Detalles;
                oCarrito.MontoPago = _pago;
                oCarrito.MontoVuelto = _vuelto;
                oCarrito.MontoTotal = _total;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RegistrarVenta(Usuario _usuario, int _documentoCliente, float _total, float _pago, float _vuelto, string _path)
        {
            try
            {
                string nroVenta = oRVender.GenerarNroDocumento();

                if (ArmarCarrito(_usuario, _documentoCliente, _total, _pago, _vuelto, nroVenta))
                {
                    oRVender.RegistrarVenta(oCarrito);
                    ActualizarStock();
                    ImprimirTicket(_path, _total, _usuario, nroVenta);
                    Detalles.Clear();
                    Productos.Clear();
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void ImprimirTicket(string _path, float total, Usuario _usuario, string _nroVenta)
        {
            supermercado = new Supermercado();
            supermercado = cnSupermercado.DatosSupermercado();

            var pdf = Document.Create(doc =>
            {
                doc.Page(pag =>
                {
                    //CABEZERA
                    pag.Header().Row(row =>
                    {
                        byte[] imagedata = System.IO.File.ReadAllBytes(path_logo);
                        row.ConstantItem(80).Image(imagedata, ImageScaling.FitArea);
                        row.RelativeItem(25).Height(80);
                        row.RelativeItem(50).Height(80).PaddingTop(20).Column(col =>
                        {
                            col.Item().AlignCenter().Text(supermercado.RazonSocial).FontSize(25).Bold().FontColor("FF0000");
                        });
                        row.RelativeItem(50).Height(80).PaddingTop(20).Column(col =>
                        {
                            col.Item().AlignCenter().Text("Atendido por:" + _usuario.Nombre).FontSize(9).Bold();
                            col.Item().AlignCenter().Text("CUIT: " + supermercado.CUIT).FontSize(9).Bold();
                            col.Item().AlignCenter().Text("Dirección: " + supermercado.Direccion).FontSize(9).Bold();
                            col.Item().AlignCenter().Text("Nro. documento: " + _nroVenta).FontSize(9).Bold();
                        });
                    });

                    //#CONTENIDO
                    pag.Content().Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Fecha:").FontSize(9).SemiBold();
                            text.Span(DateTime.Now.ToString()).FontSize(9);
                        });
                        col.Item().LineHorizontal(0.8f).LineColor(Colors.Black);
                        col.Item().PaddingTop(1).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columna =>
                            {
                                columna.RelativeColumn(3);
                                columna.RelativeColumn();
                                columna.RelativeColumn();
                                columna.RelativeColumn();
                            });
                            tabla.Header(header =>
                            {
                                header.Cell().Padding(2).AlignCenter().Text("Descripción").FontSize(11).FontColor(Colors.Black);
                                header.Cell().Padding(2).AlignCenter().Text("Cantidad").FontSize(11).FontColor(Colors.Black);
                                header.Cell().Padding(2).AlignCenter().Text("Precio").FontSize(11).FontColor(Colors.Black);
                                header.Cell().Padding(2).AlignCenter().Text("Importe").FontSize(11).FontColor(Colors.Black);
                            });

                            foreach (DetalleVenta product in Detalles)
                            {
                                tabla.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").Padding(2).AlignCenter().Text(product.oProducto.Descripcion).FontSize(10);
                                tabla.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").Padding(2).AlignCenter().Text(product.Cantidad.ToString()).FontSize(10);
                                tabla.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").Padding(2).AlignCenter().Text(product.PrecioVenta.ToString()).FontSize(10);
                                tabla.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").Padding(2).AlignCenter().Text(product.SubTotal.ToString()).FontSize(10);
                            }
                        });
                        col.Item().Padding(2).AlignRight().Text(text => {
                            text.Span("TOTAL: ").FontSize(11).SemiBold();
                            text.Span(total.ToString("C")).FontSize(11);
                        });
                        col.Item().LineHorizontal(0.8f).LineColor(Colors.Black);
                        col.Item().Padding(2).AlignCenter().Text("GRACIAS POR ELEGIRNOS :)").FontSize(15).Bold();
                    });
                });
            });

            pdf.GeneratePdf(_path);
        }

        private void ActualizarStock()
        {
            oRVender.ActualizarStock(Detalles);
        }
    }
}