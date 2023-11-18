using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using ZXing;
using Entidades;
using Repositorio;
using System.Drawing;

namespace Negocio
{
    public class NPrecios
    {
        private ReposProducto reposProducto = new ReposProducto();
        private Producto oProducto = new Producto();
        private List<Producto> ListaProductos = new List<Producto>();
        
        public void AgregarProducto(long _EAN13, string _nombre, string _descripcion, float _precioVenta)
        {
            oProducto = new Producto
            {
                EAN13 = _EAN13,
                Nombre = _nombre,
                Descripcion = _descripcion,
                PrecioVenta = _precioVenta
            };

            ListaProductos.Add(oProducto);
        }

        public void EliminarProducto(int _indice)
        {
            ListaProductos.RemoveAt(_indice);
        }

        public (bool, string) ImprimirLista(string _directorio, string _nombreBase)
        {
            int productoCount = 1;

            foreach (var producto in ListaProductos)
            {
                string EAN13 = producto.EAN13.ToString();
                Bitmap bitmap_barcode = GenerarBarcode(EAN13);

                var pdf = Document.Create(doc =>
                {
                    doc.Page(pag =>
                    {
                        pag.Header().Padding(10).Row(row =>
                        {
                            byte[] codigo_barras = ConvertImageToBytes(bitmap_barcode);
                            row.ConstantItem(100).Border(0.8f).PaddingTop(2).Image(codigo_barras, ImageScaling.FitArea);
                            row.RelativeItem(100).Border(0.8f).BorderColor(Colors.Black).Column(col =>
                            {
                                col.Item().BorderBottom(0.8f).AlignLeft().Text("Nombre:" + producto.Nombre).FontSize(9).Bold();
                                col.Item().BorderBottom(0.8f).AlignLeft().Text("Precio: " + producto.PrecioVenta).FontSize(9).Bold();
                                col.Item().AlignLeft().Text("Detalle: " + producto.Descripcion).FontSize(9).Bold();
                            });
                            row.RelativeItem(100).PaddingRight(20);
                        });
                    });
                });

                string pdfFileName = Path.Combine(_directorio, $"{_nombreBase}_{productoCount}.pdf");

                try
                {
                    pdf.GeneratePdf(pdfFileName);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

                productoCount++;
            }

            return (true, "Los tickets si imprimieron correctamente");
        }

        private Bitmap GenerarBarcode(string _EAN13)
        {
            BarcodeWriter escritor_codigo = new BarcodeWriter();
            escritor_codigo.Format = BarcodeFormat.EAN_13;
            escritor_codigo.Options = new ZXing.Common.EncodingOptions
            {
                Width = 60,
                Height = 50
            };

            Bitmap barcodeBitmap = escritor_codigo.Write(_EAN13);
            return barcodeBitmap;
        }

        private byte[] ConvertImageToBytes(Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public List<Producto> ConsultarListaImpresion()
        {
            return ListaProductos;
        }

        public DataTable VerListaProductos()
        {
            return reposProducto.ListaProductos();
        }

        public DataTable Buscar(string _filtro, string _buscar)
        {
            return reposProducto.Buscar(_filtro, _buscar);
        }
    }
}