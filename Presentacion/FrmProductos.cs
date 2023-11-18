using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using ValidarLibrary; // --> LIBRERIA DE LAS VALIDACIONES
using Entidades; // --> CAPA DONDE ESTAN LAS ENTIDADES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES
using System.Text.RegularExpressions;

namespace Presentacion
{
    public partial class FrmProductos : Form
    {
        private NProducto oNProducto = new NProducto(); // --> Administra los objetos y realiza las consultas de productos
        private DataTable dt;

        public FrmProductos()
        {
            InitializeComponent();
        }

        // Agrega filtros y carga datos de productos
        private void FrmProductos_Load(object sender, EventArgs e)
        {
            AgregarFiltros();
            LlenarDGV();
        }

        #region Botones

        // Botón para guardar un nuevo producto
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Pregunta si los txtBox están vacíos
            if (!CamposValidos())
            {
                MessageBox.Show("Olvido completar uno/varios campos complételos, por favor :)",
                    "UPS...",
                    MessageBoxButtons.OK);
                return;
            }

            // Pregunta si el producto ya esta registrado
            if (oNProducto.ProductoExistente(txtEAN13.Text))
            {
                MessageBox.Show("El producto ya existe",
                    "UPS...",
                    MessageBoxButtons.OK);
                BorrarCampos();
                return;
            }

            // Obtiene el ID del proveedor
            int proveedorID = oNProducto.IDProveedor(txtProveedor.Text);

            // No se encontró al proveedor
            if (proveedorID == -1)
            {
                MessageBox.Show("No se encontró al proveedor");
                return;
            }

            // Agrega el producto, actualiza dgvProductos y limpia los txtBox
            if (oNProducto.AgregarProducto(txtEAN13.Text, txtNombre.Text, txtDescripcion.Text, int.Parse(txtStock.Text), float.Parse(txtPrecioCompra.Text), float.Parse(txtPrecioVenta.Text), proveedorID))
            {
                MessageBox.Show("producto agregado con éxito",
                    "FELICIDADES!!",
                    MessageBoxButtons.OK);
                LlenarDGV();
                BorrarCampos();
            }
            else
            {
                MessageBox.Show("No se pudo agregar al producto, ocurrió un error :(",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                BorrarCampos();
            }
        }

        // Botón para eliminar un producto
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            DialogResult rpt = MessageBox.Show("¿Está seguro de que desea eliminar " + dgvProductos.CurrentRow.Cells[3].Value.ToString() + " ?",
                "CONFIRMACION",
                MessageBoxButtons.YesNo);

            if (rpt == DialogResult.Yes)
            {
                // ID del producto para eliminarlo
                int productoID = int.Parse(dgvProductos.CurrentRow.Cells[0].Value.ToString());

                // Elimina el producto
                if (oNProducto.EliminarProducto(productoID))
                {
                    MessageBox.Show("Se elimino el producto correctamente :)",
                        "FELICIDADES!!",
                        MessageBoxButtons.OK);
                    LlenarDGV();
                    BorrarCampos();
                }
                else
                {
                    MessageBox.Show("No se ha podido eliminar el producto :(",
                        "UPS...",
                        MessageBoxButtons.OK);
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            // Pregunta si los txtBox están vacíos
            if (!CamposValidosModificar())
            {
                MessageBox.Show("Olvido completar uno/varios campos complételos, por favor :)",
                    "UPS...",
                    MessageBoxButtons.OK);
                return;
            }

            DialogResult rpt = MessageBox.Show("¿Está seguro de que desea modificar los datos de " + dgvProductos.CurrentRow.Cells[3].Value.ToString() + "?",
                string.Empty,
                MessageBoxButtons.YesNo);

            if (rpt == DialogResult.Yes)
            {
                // ID para modificar el producto
                int productoID = int.Parse(dgvProductos.CurrentRow.Cells[0].Value.ToString());

                // Se encontró otro producto con los mismos datos
                if (oNProducto.DatosDuplicados(productoID, txtEAN13.Text))
                {
                    MessageBox.Show("Otro producto ya tiene el mismo EAN13",
                        string.Empty,
                        MessageBoxButtons.OK);
                    return;
                }

                // Modifica los datos, actualiza la lista y borra los campos
                if (oNProducto.ModificarProducto(productoID, txtEAN13.Text, txtNombre.Text, txtDescripcion.Text, int.Parse(txtStock.Text), float.Parse(txtPrecioCompra.Text), float.Parse(txtPrecioVenta.Text)))
                {
                    MessageBox.Show("Se modificaron los datos del producto",
                        string.Empty,
                        MessageBoxButtons.OK);
                    BorrarCampos();
                    LlenarDGV();
                }
                else
                {
                    MessageBox.Show("Seleccione el producto en la lista",
                        string.Empty,
                        MessageBoxButtons.OK);
                }
            }
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            // Verifica si la lista esta vacia
            if (oNProducto.VerListaImpresion() == null || oNProducto.VerListaImpresion().Count == 0)
            {
                MessageBox.Show("Debe seleccionar un producto por lo menos",
                    "UPS...",
                    MessageBoxButtons.OK);
                return;
            }

            using (SaveFileDialog direccion = new SaveFileDialog())
            {
                direccion.Filter = "Archivos PDF|*.pdf";
                direccion.FileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

                if (direccion.ShowDialog() == DialogResult.OK)
                {
                    string rpt = oNProducto.ImprimirLista(direccion.FileName);
                    MessageBox.Show(rpt, "", MessageBoxButtons.OK);
                    dgvImprimir.Rows.Clear();
                }
            }

        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            // Verifica se el texto de búsqueda no esta vacío y si se selecciono un filtro
            if (string.IsNullOrEmpty(txtBuscar.Text) || cmbFiltro.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un filtro y y escriba algo",
                    "COMO BUSCAR?",
                    MessageBoxButtons.OK);
                return;
            }

            string buscar = txtBuscar.Text;
            string filtro = cmbFiltro.SelectedItem.ToString();
            dt = new DataTable();

            dt = oNProducto.RealizarBusqueda(filtro, buscar);
            dgvProductos.Rows.Clear();

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
        }

        #endregion

        #region TEXTBOX

        // Evento que se dispara cuando cambia el texto en el cuadro de búsqueda
        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            // Si el cuadro de búsqueda está vacío, se llenan los datos en el DataGridView
            if (string.IsNullOrEmpty(txtBuscar.Text))
            {
                LlenarDGV();
            }
        }

        // Validación del TextBox para el código EAN-13
        private void txtEAN13_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtEAN13.Text))
            {
                // Valida que sean solo números
                if (!Validator.Isnumeric(txtEAN13.Text))
                {
                    errorProvider1.SetError(txtEAN13, "Solo ingrese números");
                    e.Cancel = true;
                    return;
                }

                // Valida que tenga exactamente 13 dígitos
                if (!(txtEAN13.Text.Length == 13))
                {
                    errorProvider1.SetError(txtEAN13, "El código EAN-13 debe tener 13 dígitos");
                    e.Cancel = true;
                    return;
                }

                // Valida si es un EAN-13 válido
                if (!IsEAN13(txtEAN13.Text))
                {
                    errorProvider1.SetError(txtEAN13, "Ingrese un EAN-13 válido");
                    e.Cancel = true;
                    return;
                }

                // Si la validación es exitosa, se limpia el errorProvider
                errorProvider1.SetError(txtEAN13, string.Empty);
                e.Cancel = false;
            }
        }

        // Validación del TextBox para el nombre del producto
        private void txtNombre_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtNombre.Text))
            {
                // Valida que solo contenga letras
                if (!Validator.IsAlphabetic(txtNombre.Text))
                {
                    errorProvider1.SetError(txtNombre, "El nombre solo puede contener letras");
                    e.Cancel = true;
                    return;
                }

                // Valida que tenga más de 2 letras
                if (txtNombre.Text.Length < 2)
                {
                    errorProvider1.SetError(txtNombre, "El nombre debe tener más de 2 letras");
                    e.Cancel = true;
                    return;
                }

                // Si la validación es exitosa, se limpia el errorProvider
                errorProvider1.SetError(txtNombre, string.Empty);
                e.Cancel = false;
            }
        }

        // Validación del TextBox para la cantidad en stock
        private void txtStock_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtStock.Text))
            {
                // Valida que sean solo números
                if (!Validator.Isnumeric(txtStock.Text))
                {
                    errorProvider1.SetError(txtStock, "Solo ingrese números");
                    e.Cancel = true;
                    return;
                }

                // Valida que la cantidad no sea menor o igual a cero
                if (!CantidadValida(txtStock.Text))
                {
                    errorProvider1.SetError(txtStock, "La cantidad no puede ser menor/igual a cero");
                    e.Cancel = true;
                    return;
                }

                // Si la validación es exitosa, se limpia el errorProvider
                errorProvider1.SetError(txtStock, string.Empty);
                e.Cancel = false;
            }
        }

        // Validación del TextBox para el precio de compra
        private void txtPrecioCompra_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtPrecioCompra.Text))
            {
                // Valida que sea un formato de moneda válido
                if (!Validator.IsMoneda(txtPrecioCompra.Text))
                {
                    errorProvider1.SetError(txtPrecioCompra, "Introduzca un precio válido");
                    e.Cancel = true;
                    return;
                }

                // Si la validación es exitosa, se limpia el errorProvider
                errorProvider1.SetError(txtPrecioCompra, string.Empty);
                e.Cancel = false;
            }
        }

        // Validación del TextBox para el precio de venta
        private void txtPrecioVenta_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtPrecioVenta.Text))
            {
                // Valida que sea un formato de moneda válido
                if (!Validator.IsMoneda(txtPrecioVenta.Text))
                {
                    errorProvider1.SetError(txtPrecioVenta, "Introduzca un precio válido");
                    e.Cancel = true;
                    return;
                }

                // Si la validación es exitosa, se limpia el errorProvider
                errorProvider1.SetError(txtPrecioVenta, string.Empty);
                e.Cancel = false;
            }
        }

        // Validación del TextBox para el CUIT del proveedor
        private void txtProveedor_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtProveedor.Text))
            {
                // Valida que tenga al menos 13 caracteres
                if (!(txtProveedor.Text.Length >= 13))
                {
                    errorProvider1.SetError(txtProveedor, "El CUIT no puede tener menos de 13 caracteres");
                    e.Cancel = true;
                    return;
                }

                // Valida que sea un CUIT válido
                if (!IsCUIT(txtProveedor.Text))
                {
                    errorProvider1.SetError(txtProveedor, "Ingrese un CUIT válido, ej: 22-22222222-2");
                    e.Cancel = true;
                    return;
                }

                // Si la validación es exitosa, se limpia el errorProvider
                errorProvider1.SetError(txtProveedor, string.Empty);
                e.Cancel = false;
            }
        }

        #endregion

        #region DGV

        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtEAN13.Text = dgvProductos.CurrentRow.Cells[1].Value.ToString();
            txtNombre.Text = dgvProductos.CurrentRow.Cells[2].Value.ToString();
            txtDescripcion.Text = dgvProductos.CurrentRow.Cells[3].Value.ToString();
            txtStock.Text = dgvProductos.CurrentRow.Cells[4].Value.ToString();
            txtPrecioCompra.Text = dgvProductos.CurrentRow.Cells[5].Value.ToString();
            txtPrecioVenta.Text = dgvProductos.CurrentRow.Cells[6].Value.ToString();
        }

        private void dgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvProductos != null)
            {
                oNProducto.AgregarImprimir(txtEAN13.Text, txtNombre.Text, txtDescripcion.Text, txtStock.Text, txtPrecioCompra.Text, dgvProductos.CurrentRow.Cells["RazonSocial"].Value.ToString());
                ActualizarListaImpresion();
            }
        }

        private void dgvImprimir_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verifica que se hizo clic en una celda válida y que la columna contiene una imagen
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvImprimir.Columns[e.ColumnIndex] is DataGridViewImageColumn)
            {
                // Obtén el índice de la fila seleccionada
                int indice_fila = e.RowIndex;
                oNProducto.EliminarImprimir(indice_fila);
                ActualizarListaImpresion();
            }
        }

        private void LlenarDGV()
        {
            dgvProductos.Rows.Clear();
            dt = new DataTable();

            dt = oNProducto.ListaProveedores();

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
                        dr["FechaRegistro"],
                        dr["RazonSocial"],
                        dr["ProveedorID"]);
                }
            }
            else
            {
                MessageBox.Show("No hay proveedores cargados"
                    , "",
                    MessageBoxButtons.OK);
            }
        }

        private void ActualizarListaImpresion()
        {
            List<Producto> lista_productos = oNProducto.VerListaImpresion();

            dgvImprimir.Rows.Clear();

            foreach (Producto producto in lista_productos)
            {
                int fila = dgvImprimir.Rows.Add(
                    producto.EAN13,
                    producto.Nombre,
                    producto.Descripcion,
                    producto.Stock,
                    producto.PrecioCompra,
                    producto.Proveedor
                );
            }
        }

        #endregion

        #region VALIDACIONES

        private bool IsEAN13(string _EAN)
        {
            if (!Regex.IsMatch(_EAN, @"^\d{13}$"))
            {
                return false;
            }

            char[] digitos = _EAN.Take(12).ToArray();

            // Calcular el dígito de verificación.
            int suma_pares = 0;
            int suma_impares = 0;

            for (int i = 0; i < 12; i++)
            {
                int digito = int.Parse(digitos[i].ToString());
                if (i % 2 == 0)
                {
                    suma_pares += digito;
                }
                else
                {
                    suma_impares += digito;
                }
            }
            int total = suma_pares + (suma_impares * 3);
            int digito_verificacion = (10 - (total % 10)) % 10;

            // Comparar el dígito de verificación calculado con el último dígito.
            return digito_verificacion == int.Parse(_EAN[12].ToString());
        }

        private bool CantidadValida(string _cantidad)
        {
            int cantidad;

            if (int.TryParse(_cantidad, out cantidad) && cantidad > 0)
            {
                return true;
            }
            return false;
        }

        // Formato del CUIT 55-55555555-5, guiones obligatorios
        private bool IsCUIT(string _CUIT)
        {
            if (string.IsNullOrEmpty(_CUIT)) { return false; }
            return Regex.IsMatch(_CUIT, @"^(\d{2}-\d{8}-\d{1}|\d{11}|\d{2}-\d{7}-\d{1})$");
        }

        #endregion

        #region OTROS

        private bool CamposValidos()
        {
            foreach (TextBox txt in this.Controls.OfType<TextBox>())
            {
                // Salta a la siguiente iteración del bucle
                if (txt.Name == "txtBuscar")
                {
                    continue;
                }

                if (Validator.IsWhiteSpace(txt.Text))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CamposValidosModificar()
        {
            foreach (TextBox txt in this.Controls.OfType<TextBox>())
            {
                // Salta a la siguiente iteración del bucle
                if (txt.Name == "txtBuscar")
                {
                    continue;
                }

                // Salta a la siguiente iteración del bucle
                if (txt.Name == "txtProveedor")
                {
                    continue;
                }

                if (Validator.IsWhiteSpace(txt.Text))
                {
                    return false;
                }
            }
            return true;
        }

        private void BorrarCampos()
        {
            foreach (TextBox txt in this.Controls.OfType<TextBox>())
            {
                txt.Clear();
            }
        }

        private void AgregarFiltros()
        {
            cmbFiltro.Items.AddRange(new string[] { "EAN13", "Nombre", "Descripcion", "Stock", "PrecioCompra", "PrecioVenta" });
        }

        #endregion
    }
}