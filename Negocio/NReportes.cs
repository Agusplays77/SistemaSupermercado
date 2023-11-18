using System;
using System.Collections.Generic;
using System.IO;
using Entidades;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using Repositorio;

namespace Negocio
{
    public class NReportes
    {
        private ReposSupermercado ReposSupermercado = new ReposSupermercado();
        private ReposReportes ReposReportes = new ReposReportes();
        private Supermercado Supermercado = new Supermercado();
        private List<Venta> ListaVentas = new List<Venta>();
        private float Total;

        public void CargarLista()
        {
            ListaVentas = ReposReportes.ListarReportes();
        }

        public void FiltrarLista(string _fechaInicio, string _fechaCierre)
        {
            ListaVentas.Clear();
            ListaVentas = ReposReportes.ListaReportesFiltrados(_fechaInicio, _fechaCierre);
        }

        public List<Venta> ConsultarLista()
        {
            return ListaVentas;
        }

        public (bool, string) ImprimirListado(string _path, string _fechaInicio, string _fechaCierre)
        {
            Total = 0;
            Supermercado = ReposSupermercado.DatosSupermercado();

            var pdf = Document.Create(doc =>
            {
                doc.Page(pag =>
                {
                    pag.Header().Padding(10).Row(header =>
                    {
                        Byte[] imagen = File.ReadAllBytes("C:\\Users\\TheWolfplay\\source\\repos\\TP_2\\data\\Dragon.png");
                        header.ConstantItem(100).Image(imagen, ImageScaling.FitArea);
                        header.RelativeItem(25).Height(80);
                        header.RelativeItem(50).Height(80).PaddingTop(20).Column(col =>
                        {
                            col.Item().AlignCenter().Text(Supermercado.RazonSocial).FontSize(25).Bold().FontColor("FF0000");
                        });
                        header.RelativeItem(50).Height(80).PaddingTop(20).Column(col =>
                        {
                            col.Item().AlignCenter().Text("CUIT: " + Supermercado.CUIT).FontSize(9).Bold();
                            col.Item().AlignCenter().Text("Dirección: " + Supermercado.Direccion).FontSize(9).Bold();
                        });
                    });

                    pag.Content().Column(col =>
                    {
                        col.Item().PaddingLeft(10).Text(txt =>
                        {
                            txt.Span("Desde:").FontSize(9).SemiBold();
                            txt.Span(_fechaInicio).FontSize(9).SemiBold();
                        });

                        col.Item().PaddingLeft(10).Text(txt =>
                        {
                            txt.Span("Hasta:").FontSize(9).SemiBold();
                            txt.Span(_fechaCierre).FontSize(9).SemiBold();
                        });

                        col.Item().LineHorizontal(0.8f).LineColor("D9D9D9");

                        col.Item().Table(tb =>
                        {
                            tb.ColumnsDefinition(column =>
                            {
                                column.RelativeColumn();
                                column.RelativeColumn();
                                column.RelativeColumn();
                            });

                            tb.Header(tbHeader =>
                            {
                                tbHeader.Cell().AlignLeft().PaddingLeft(30).Text("Nro documento:").FontSize(11).FontColor(Colors.Black);
                                tbHeader.Cell().AlignCenter().Text("Monto total:").FontSize(11).FontColor(Colors.Black);
                                tbHeader.Cell().AlignRight().PaddingRight(30).Text("Fecha creación:").FontSize(11).FontColor(Colors.Black);
                            });

                            foreach (var reporte in ListaVentas)
                            {
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").PaddingLeft(30).Text(reporte.NroDocumento).FontSize(10);
                                Total += reporte.MontoTotal;
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").AlignCenter().Text(reporte.MontoTotal.ToString()).FontSize(10);
                                tb.Cell().BorderBottom(0.5f).BorderColor("D9D9D9").PaddingRight(30).AlignRight().Text(DateTime.Parse(reporte.FechaVenta).ToString("dd-MM-yyyy")).FontSize(10);
                            }
                        });

                        col.Item().PaddingTop(5).PaddingRight(30).AlignRight().Text(text => {
                            text.Span("TOTAL: ").FontSize(11).SemiBold();
                            text.Span(Total.ToString("C")).FontSize(11);
                        });
                    });
                });
            });
            try
            {
                pdf.GeneratePdf(_path);
                return (true, "Se imprimió el listado correctamente :)");
            }
            catch (Exception ex)
            {
                return (true, ex.Message);
            }
        }
    }
}