using System;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using Entidades; // --> CAPA DONDE ESTAN LAS ENTIDADES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES

namespace Presentacion
{
    public partial class FrmPrecios : Form
    {
        private NPrecios cnPrecio = new NPrecios(); // --> Administra los objetos y realiza las consultas de precios
        private DataTable dt;

        public FrmPrecios()
        {
            InitializeComponent();
        }

        private void FrmPrecios_Load(object sender, EventArgs e)
        {
            AgregarFiltros();
            LlenarDGV();
        }

        #region BOTONES

        // Botón para agregar todos los productos al carrito de precios
        private void btnAgregarTodos_Click(object sender, EventArgs e)
        {
            if (!(dgvProductos.Rows.Count > 0))
            {
                MessageBox.Show("No hay productos cargados. Debe cargar un producto.",
                    string.Empty,
                    MessageBoxButtons.OK);
                return;
            }

            foreach (DataGridViewRow producto in dgvProductos.Rows)
            {
                long EAN13 = long.Parse(producto.Cells["EAN13"].Value.ToString());
                string Nombre = producto.Cells["Nombre"].Value.ToString();
                string Descrion = producto.Cells["Descripcion"].Value.ToString();
                float PrecioVenta = float.Parse(producto.Cells["PrecioVenta"].Value.ToString());

                cnPrecio.AgregarProducto(EAN13, Nombre, Descrion, PrecioVenta);
            }
            LlenarDGV();
        }

        // Botón para buscar productos según los parámetros seleccionados
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBuscar.Text) || cmbFiltro.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione algún parámetro de búsqueda, por favor :)",
                    "UPSS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string buscar = txtBuscar.Text;
            string filtro = cmbFiltro.Text;
            dt = new DataTable();

            dt = cnPrecio.Buscar(filtro, buscar);

            if (dt.Rows.Count > 0 && dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dgvProductos.Rows.Add(
                        dr["ProductoID"],
                        dr["EAN13"],
                        dr["Nombre"],
                        dr["Descripcion"],
                        dr["Stock"],
                        dr["PrecioCompra"],
                        dr["PrecioVenta"],
                        dr["FechaRegistro"]);
                }
            }
            else
            {
                MessageBox.Show("No se encontró ningún producto con esos parámetros",
                    "UPSS... :(",
                    MessageBoxButtons.OK);
            }
        }

        // Botón para imprimir la lista de productos seleccionados
        private void btnImprimir_Click(object sender, EventArgs e)
        {
            if (cnPrecio.ConsultarListaImpresion() == null || cnPrecio.ConsultarListaImpresion().Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un producto", string.Empty, MessageBoxButtons.OK);
                return;
            }

            using (SaveFileDialog direccion = new SaveFileDialog())
            {
                direccion.Filter = "Archivos PDF|*.pdf";
                direccion.FileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

                if (direccion.ShowDialog() == DialogResult.OK)
                {
                    string directorio = Path.GetDirectoryName(direccion.FileName);
                    string nombreBase = Path.GetFileNameWithoutExtension(direccion.FileName);

                    var resultado = cnPrecio.ImprimirLista(directorio, nombreBase);

                    if (resultado.Item1)
                    {
                        MessageBox.Show(resultado.Item2, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error al imprimir los tickets: " + resultado.Item2, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion

        #region DGV

        // Evento que se dispara al hacer clic en una celda del DataGridView de productos
        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                long EAN13 = long.Parse(dgvProductos.Rows[e.RowIndex].Cells["EAN13"].Value.ToString());
                string Nombre = dgvProductos.Rows[e.RowIndex].Cells["Nombre"].Value.ToString();
                string Descrion = dgvProductos.Rows[e.RowIndex].Cells["Descripcion"].Value.ToString();
                float PrecioVenta = float.Parse(dgvProductos.Rows[e.RowIndex].Cells["PrecioVenta"].Value.ToString());

                cnPrecio.AgregarProducto(EAN13, Nombre, Descrion, PrecioVenta);
                ActualizarLista();
            }
        }

        // Método para llenar el DataGridView con la lista de productos
        private void LlenarDGV()
        {
            dgvProductos.Rows.Clear();
            dt = new DataTable();

            dt = cnPrecio.VerListaProductos();

            if (dt.Rows.Count > 0 && dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dgvProductos.Rows.Add(
                        dr["ProductoID"],
                        dr["EAN13"],
                        dr["Nombre"],
                        dr["Descripcion"],
                        dr["Stock"],
                        dr["PrecioCompra"],
                        dr["PrecioVenta"],
                        dr["FechaRegistro"]);
                }
            }
            else
            {
                MessageBox.Show("No hay productos cargados", "", MessageBoxButtons.OK);
            }
        }

        // Método para actualizar la lista de productos en el DataGridView de impresión
        private void ActualizarLista()
        {
            List<Producto> lista_productos = cnPrecio.ConsultarListaImpresion();

            dgvImprimir.Rows.Clear();

            foreach (Producto producto in lista_productos)
            {
                int fila = dgvImprimir.Rows.Add(
                    producto.EAN13,
                    producto.Nombre,
                    producto.Descripcion,
                    producto.PrecioVenta
                );
            }
        }

        // Evento que se dispara al hacer clic en una celda del DataGridView de impresión
        private void dgvImprimir_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verifica que se hizo clic en una celda válida y que la columna contiene una imagen
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvImprimir.Columns[e.ColumnIndex] is DataGridViewImageColumn)
            {
                // Obtén el índice de la fila seleccionada
                int indice_fila = e.RowIndex;
                cnPrecio.EliminarProducto(indice_fila);
                ActualizarLista();
            }
        }

        #endregion

        #region OTROS

        // Método para agregar filtros al ComboBox de búsqueda
        private void AgregarFiltros()
        {
            cmbFiltro.Items.AddRange(new string[] { "EAN13", "Nombre", "Descripcion", "Stock", "PrecioCompra", "PrecioVenta" });
        }

        #endregion
    }
}