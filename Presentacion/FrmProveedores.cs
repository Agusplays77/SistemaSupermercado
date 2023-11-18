using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ValidarLibrary; // --> LIBRERIA DE LAS VALIDACIONES
using Entidades; // --> CAPA DONDE ESTAN LAS ENTIDADES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES

namespace Presentacion
{
    public partial class FrmProveedores : Form
    {
        private NProveedor oNProveedor = new NProveedor(); // --> Administra los objetos y realiza las consultas de proveedores
        private DataTable dt;

        public FrmProveedores()
        {
            InitializeComponent();
        }

        // Carga los filtros y llena el dgvProveedores
        private void FrmProveedores_Load(object sender, EventArgs e)
        {
            AgregarFiltros();
            LlenarDGV();
        }

        #region Botones

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            // Pregunta si los campos están vacíos
            if (!CamposValidos())
            {
                MessageBox.Show("Olvido completar uno/varios campos complételos, por favor :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Consulta si ya existe el proveedor y limpia txtBox
            if (oNProveedor.ProveedorExistente(txtCUIT.Text, txtMail.Text, txtTelefono.Text))
            {
                MessageBox.Show("El proveedor ya existe",
                    "UPS...",
                    MessageBoxButtons.OK);
                BorrarCampos();
                return;
            }

            // Agrega el proveedor, actualiza lista y borra txtBox
            if (oNProveedor.AgregarProveedor(txtCUIT.Text, txtRazonSocial.Text, txtMail.Text, txtTelefono.Text, txtRubro.Text, txtDireccion.Text))
            {
                MessageBox.Show("Proveedor agregado correctamente",
                    "FELICIDADES!!",
                    MessageBoxButtons.OK);
                LlenarDGV();
                BorrarCampos();
            }
            else
            {
                MessageBox.Show("No se pudo agregar al proveedor, ocurrió un error",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                BorrarCampos();
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            DialogResult rpt = MessageBox.Show("¿Está seguro de que desea eliminar a " + dgvProveedores.CurrentRow.Cells[2].Value.ToString() + "?",
                "CONFIRMACION",
                MessageBoxButtons.YesNo);

            if (rpt == DialogResult.Yes)
            {
                // ID para eliminar el proveedor
                int usuarioID = int.Parse(dgvProveedores.CurrentRow.Cells[0].Value.ToString());

                // Elimina el proveedor, llena dgvProveedores y borra campos
                if (oNProveedor.EliminarProveedor(usuarioID))
                {
                    MessageBox.Show("Se elimino al proveedor correctamente",
                        "FELICIDADES!!",
                        MessageBoxButtons.OK);
                    LlenarDGV();
                    BorrarCampos();
                }
                else
                {
                    MessageBox.Show("No se ha podido eliminar al proveedor",
                        "UPS...",
                        MessageBoxButtons.OK);
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            // Pregunta si los campos están vacíos
            if (!CamposValidos())
            {
                MessageBox.Show("Olvido completar uno/varios campos complételos, por favor :)",
                    "UPS...",
                    MessageBoxButtons.OK);
                return;
            }

            DialogResult rpt = MessageBox.Show("¿Está seguro de que desea modificar los datos de " + dgvProveedores.CurrentRow.Cells[2].Value.ToString() + "?",
                "CONFIRMACION",
                MessageBoxButtons.YesNo);

            if (rpt == DialogResult.Yes)
            {
                // ID para modificar los datos
                int proveedorID = int.Parse(dgvProveedores.CurrentRow.Cells[0].Value.ToString());

                // Pregunta si otro proveedor tiene los mismos datos
                if (oNProveedor.DatosDuplicados(proveedorID, txtCUIT.Text, txtMail.Text, txtTelefono.Text))
                {
                    MessageBox.Show("Otro proveedor ya tiene el mismo CUIT, mail o teléfono",
                        "UPS...",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Modifica, actualiza lista, limpia txtBox
                if (oNProveedor.ModificarUsuario(proveedorID, txtRazonSocial.Text, txtMail.Text, txtTelefono.Text, txtRubro.Text, txtDireccion.Text))
                {
                    MessageBox.Show("Se modificaron los datos del proveedor",
                        "FELICIDADES!!",
                        MessageBoxButtons.OK);
                    BorrarCampos();
                    LlenarDGV();
                }
                else
                {
                    MessageBox.Show("Debe seleccionar al proveedor en la lista :)",
                        "Como modificar?",
                        MessageBoxButtons.OK);
                }
            }
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            // Selección del destino del PDF
            using (SaveFileDialog direccion = new SaveFileDialog())
            {
                // Establece el filtro para mostrar solo archivos PDF
                direccion.Filter = "Archivos PDF|*.pdf";
                // Nombre del archivo como la fecha actual
                direccion.FileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

                // Imprime la lista, retorna un mensaje y limpia el dgvImprimir
                if (direccion.ShowDialog() == DialogResult.OK)
                {
                    string rpt = oNProveedor.ImprimirLista(direccion.FileName);
                    MessageBox.Show(rpt,
                        string.Empty,
                        MessageBoxButtons.OK);
                    dgvImprimir.Rows.Clear();
                }
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            // Filtro o búsqueda nula, cancela evento
            if (string.IsNullOrEmpty(txtBuscar.Text) || cmbFiltro.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un filtro y escriba algo :)",
                    "Como buscar?",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            dt = new DataTable();
            string buscar = txtBuscar.Text;
            string filtro = cmbFiltro.SelectedItem.ToString();

            // Limpia limpia las filas del dgvProveedores
            dgvProveedores.Rows.Clear();
            // Realiza la búsqueda y retorna la respuesta en un DataTable
            dt = oNProveedor.RealizarBusqueda(filtro, buscar);

            //Consulta si se encontró algún proveedor en la búsqueda
            if (dt.Rows.Count > 0 && dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dgvProveedores.Rows.Add(
                        dr["ProveedorID"],
                        dr["CUIT"],
                        dr["RazonSocial"],
                        dr["Mail"],
                        dr["Telefono"],
                        dr["Rubro"],
                        dr["Direccion"],
                        dr["FechaCreacion"]);
                }
            }
            else
            {
                MessageBox.Show("No sea ha encontrado ningún proveedor " + buscar + " por" + filtro + ", pruebe con otras palabras o seleccione un filtro diferente :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Textbox

        // El primer "IF" es para validar solo cuando se introduce texto
        private void txtCUIT_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtCUIT.Text))
            {
                // Valida si al menos tiene mas de 13 caracteres
                if (!(txtCUIT.Text.Length >= 13))
                {
                    errorProvider1.SetError(txtCUIT, "El CUIT no puede tener menos de 13 caracteres");
                    e.Cancel = true;
                    return;
                }

                // Valida si es un CUIT
                if (!IsCUIT(txtCUIT.Text))
                {
                    errorProvider1.SetError(txtCUIT, "Ingrese un CUIT valido, ej: 22-22222222-2");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtCUIT, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtRazonSocial_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtRazonSocial.Text))
            {
                string Nombre = txtRazonSocial.Text;

                // Valida si son letras
                if (!Validator.IsAlphabetic(Nombre))
                {
                    errorProvider1.SetError(txtRazonSocial, "No se permiten números");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtRazonSocial, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtMail_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtMail.Text))
            {
                // Valida si es un mail
                if (!Validator.IsEmail(txtMail.Text))
                {
                    errorProvider1.SetError(txtMail, "Ingrese un mail valido, ej:correoDeEjemplo@gmail.com");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtMail, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtTelefono_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtTelefono.Text))
            {
                // Valida que sea un teléfono valido
                if (!Validator.IsPhone(txtTelefono.Text))
                {
                    errorProvider1.SetError(txtTelefono, "Ingrese un teléfono valido, ej. 0[opcional] 5555-555555");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtTelefono, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtRubro_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtRubro.Text))
            {
                // Valida si son letras
                if (!Validator.IsAlphabetic(txtRubro.Text))
                {
                    errorProvider1.SetError(txtRubro, "Ingrese solo letras, por favor");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtRubro, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtDireccion_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtDireccion.Text))
            {
                // Valida si es una direccion
                if (!IsDireccion(txtDireccion.Text))
                {
                    errorProvider1.SetError(txtDireccion, "Ingrese solo letras y números");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtDireccion, string.Empty);
                e.Cancel = false;
            }
        }

        // Cuando se borra la búsqueda en el txtBuscar carga los proveedores del repositorio
        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBuscar.Text))
            {
                LlenarDGV();
            }
        }

        #endregion

        #region Validaciones

        // Formato del CUIT 55-55555555-5, guiones obligatorios
        private bool IsCUIT(string _CUIT)
        {
            if (string.IsNullOrEmpty(_CUIT)) { return false; }
            return Regex.IsMatch(_CUIT, @"^(\d{2}-\d{8}-\d{1}|\d{11}|\d{2}-\d{7}-\d{1})$");
        }

        // Permite letras, números y espacios entre medio. No acepta caracteres especiales
        private bool IsDireccion(string _CUIT)
        {
            if (string.IsNullOrEmpty(_CUIT)) { return false; }
            return Regex.IsMatch(_CUIT, @"^[a-zA-ZáéíóúüÁÉÍÓÚÜñÑ0-9\s-]+$");
        }

        #endregion

        #region DGV

        // Carga los proveedores del repositorio
        private void LlenarDGV()
        {
            //Inicializa el DataTable
            dt = new DataTable();

            // Limpia dgvProveedores
            dgvProveedores.Rows.Clear();
            // Retorno del DataTable con los proveedores cargados en el repositorio
            dt = oNProveedor.ListaProveedores();

            // Consulta si esta vacía y agrega las filas
            if (dt.Rows.Count > 0 && dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dgvProveedores.Rows.Add(
                        dr["ProveedorID"],
                        dr["CUIT"],
                        dr["RazonSocial"],
                        dr["Mail"],
                        dr["Telefono"],
                        dr["Rubro"],
                        dr["Direccion"],
                        dr["FechaCreacion"]);
                }
            }
            else
            {
                MessageBox.Show("No hay proveedores cargados, agregue un proveedor :)",
                    "UPS...",
                    MessageBoxButtons.OK);
            }
        }

        // Carga los datos en los txtBox
        private void dgvProveedores_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtCUIT.Text = dgvProveedores.CurrentRow.Cells[1].Value.ToString();
            txtRazonSocial.Text = dgvProveedores.CurrentRow.Cells[2].Value.ToString();
            txtMail.Text = dgvProveedores.CurrentRow.Cells[3].Value.ToString();
            txtTelefono.Text = dgvProveedores.CurrentRow.Cells[4].Value.ToString();
            txtRubro.Text = dgvProveedores.CurrentRow.Cells[5].Value.ToString();
            txtDireccion.Text = dgvProveedores.CurrentRow.Cells[6].Value.ToString();
        }

        // Agrega el proveedor a lista de impresión y actualiza el dgvImprimir
        private void dgvProveedores_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvProveedores != null)
            {
                oNProveedor.AgregarImpimir(txtCUIT.Text, txtRazonSocial.Text, txtMail.Text, txtTelefono.Text, txtRubro.Text, txtDireccion.Text);
                ActualizarListaImpresion();
            }
        }

        // Consulta la lista de impresión
        private void ActualizarListaImpresion()
        {
            // Retorno de la lista de impresión
            List<Proveedor> listaProveedores = oNProveedor.VerLista();

            // Limpia el dgvImprimir
            dgvImprimir.Rows.Clear();

            // Agrega las filas
            foreach (Proveedor proveedor in listaProveedores)
            {
                int fila = dgvImprimir.Rows.Add(
                    proveedor.CUIT,
                    proveedor.RazonSocial,
                    proveedor.Mail,
                    proveedor.Telefono,
                    proveedor.Rubro,
                    proveedor.Direccion
                );
            }
        }

        // Eliminar el proveedor de la lista de impresión
        private void dgvImprimir_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verifica que se hizo clic en una celda válida y que la columna contiene una imagen
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvImprimir.Columns[e.ColumnIndex] is DataGridViewImageColumn)
            {
                // Obtén el índice de la fila seleccionada
                int rowIndex = e.RowIndex;
                // Paso del índice para eliminar al proveedor
                oNProveedor.EliminarImprimir(rowIndex);
                // Actualizar la lista
                ActualizarListaImpresion();
            }
        }

        #endregion

        #region OTROS

        // Valida que los campos no estén vacíos
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

        // Limpia los txtBox
        private void BorrarCampos()
        {
            foreach (TextBox txt in this.Controls.OfType<TextBox>())
            {
                txt.Clear();
            }
        }

        // Agrega los filtros de búsqueda
        private void AgregarFiltros()
        {
            cmbFiltro.Items.AddRange(new string[] { "CUIT", "RazonSocial", "Mail", "Telefono", "Rubro", "Direccion" });
        }

        #endregion
    }
}