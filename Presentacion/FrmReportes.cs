using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Entidades; // --> CAPA DONDE ESTAN LAS ENTIDADES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES

namespace Presentacion
{
    public partial class FrmReportes : Form
    {
        private NReportes cnReportes = new NReportes(); // --> Administra los objetos y realiza las consultas de reportes
        List<Venta> Ventas;

        public FrmReportes()
        {
            InitializeComponent();
        }
        private void FrmReportes_Load(object sender, EventArgs e)
        {
            cnReportes.CargarLista();
            LlenarDGV();
        }

        #region BOTONES

        private void btnImprimo_Click(object sender, EventArgs e)
        {
            if (cnReportes.ConsultarLista() == null)
            {
                MessageBox.Show("Debe seleccionar al menos una venta",
                    string.Empty,
                    MessageBoxButtons.OK);
            }

            string fecha_inicio = dtpInicio.Value.ToString("dd-MM-yyyy");
            string fecha_cierre = dtpFinal.Value.ToString("dd-MM-yyyy");

            using (SaveFileDialog direccion = new SaveFileDialog())
            {
                direccion.Filter = "Archivos PDF|*.pdf";
                direccion.FileName = DateTime.Now.ToString("dd-MM-yyyy");

                if (direccion.ShowDialog() == DialogResult.OK)
                {
                    var rpt = cnReportes.ImprimirListado(direccion.FileName, fecha_inicio, fecha_cierre);

                    if (rpt.Item1)
                    {
                        MessageBox.Show(rpt.Item2, "EXITO", MessageBoxButtons.OK);
                    }
                    else
                    {
                        MessageBox.Show(rpt.Item2, "UPS...", MessageBoxButtons.OK);
                    }
                }

            }
        }

        private void btnFiltro_Click(object sender, EventArgs e)
        {
            string fecha_inicio = dtpInicio.Value.ToString("dd-MM-yyyy");
            string fecha_cierre = dtpFinal.Value.ToString("dd-MM-yyyy");

            cnReportes.FiltrarLista(fecha_inicio, fecha_cierre);
            LlenarDGV();
        }

        #endregion

        #region OTROS

        private void LlenarDGV()
        {
            dgvListaVentas.Rows.Clear();
            Ventas = cnReportes.ConsultarLista();

            if (Ventas.Count > 0 && Ventas != null)
            {
                foreach (var reporte in Ventas)
                {
                    dgvListaVentas.Rows.Add(
                        reporte.NroDocumento,
                        reporte.MontoTotal,
                        reporte.FechaVenta);
                }
            }
            else
            {
                MessageBox.Show("No se ha encontrado ninguna venta",
                    "Advertencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
            ActualizarLabels();
        }

        private void ActualizarLabels()
        {
            decimal total = 0;
            int cantidad = dgvListaVentas.Rows.Count;

            foreach (DataGridViewRow row in dgvListaVentas.Rows)
            {
                decimal monto;
                if (decimal.TryParse(row.Cells["MontoTotal"].Value.ToString(), out monto))
                {
                    total += monto;
                }
            }

            lblTotal2.Text = total.ToString();
            lblCantidad2.Text = cantidad.ToString();
        }

        #endregion
    }
}