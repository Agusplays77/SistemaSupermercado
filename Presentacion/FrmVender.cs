using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ValidarLibrary; // --> LIBREIRA DE LAS VALDIACIONES
using ZXing; // --> CODIGOS DE BARRAS
using AForge.Video; // --> LECTOR
using AForge.Video.DirectShow; // --> LECTOR
using Entidades; // --> CAPA DONDE ESTAN LAS ENTIDADES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES

namespace Presentacion
{
    public partial class FrmVender : Form
    {
        private bool HayDispositivos; // --> Bandera para saber si hay dispositivos
        private FilterInfoCollection Dispositivos; // --> Filtra los dispositivos
        private VideoCaptureDevice MiDispositivo; // --> Dispositivo de captura
        private NVender oNVenta = new NVender(); // --> Administra los objetos y realiza las consultas de ventas
        private float Total = 0; // Total de la compra
        private Usuario Usuario; // Guarda los datos del usuario para la factura

        public FrmVender(Usuario _usuario)
        {
            InitializeComponent();
            CargarDispositivos();
            Usuario = _usuario;
        }

        private void FrmVender_FormClosed(object sender, FormClosedEventArgs e)
        {
            CerrarDispositivo();
        }

        #region BOTONES

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEAN.Text) && !string.IsNullOrEmpty(txtCantidad.Text))
            {
                if (IsEAN13(txtEAN.Text))
                {
                    if (oNVenta.AgrgegarProducto(txtEAN.Text, txtCantidad.Text))
                    {
                        ActualizarLista();
                        // Obtener el último elemento agregado al DataGridView
                        if (dgvTicket.Rows.Count > 0)
                        {
                            DataGridViewRow ultima_fila = dgvTicket.Rows[dgvTicket.Rows.Count - 1];
                            float precioUnitario = float.Parse(ultima_fila.Cells["PrecioVenta"].Value.ToString());
                            int cantidad = Convert.ToInt32(txtCantidad.Text);
                            float subtotal = precioUnitario * cantidad;

                            // Actualizar los campos de texto con los valores obtenidos
                            txtPrecioUnitario.Text = precioUnitario.ToString();
                            txtSubtotal.Text = subtotal.ToString();

                            CalcularTotal();
                        }
                        LimpiarTxtBox();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el producto",
                            "UPS...",
                            MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show("Ingrese un código valido",
                        "Advertencia",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Complete los campos de código y cantidad",
                    string.Empty,
                    MessageBoxButtons.OK);
            }
        }

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (dgvTicket == null || dgvTicket.Rows.Count == 0)
            {
                MessageBox.Show("Primero debe agregar los productos :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (txtDocumentoCliente.Text == string.Empty)
            {
                MessageBox.Show("Pregúntele el documento al cliente :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (txtMontoPago.Text == string.Empty)
            {
                MessageBox.Show("Debe ingresar el pago del cliente :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (float.Parse(txtMontoPago.Text) < Total)
            {
                MessageBox.Show("El pago debe ser mayor al total esto no es una ONG :)",
                    string.Empty,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            txtMontoVuelto.Text = (float.Parse(txtMontoPago.Text) - Total).ToString();

            
            using (SaveFileDialog doc = new SaveFileDialog())
            {
                doc.FileName = DateTime.Now.ToString("ddMMyyyyHHmmSS") + ".pdf";

                if (doc.ShowDialog() == DialogResult.OK)
                {
                    oNVenta.RegistrarVenta(Usuario, Convert.ToInt32(txtDocumentoCliente.Text), Total, float.Parse(txtMontoPago.Text), float.Parse(txtMontoVuelto.Text), doc.FileName);
                    FinalizarLimpiar();
                }
            }
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            CerrarDispositivo();
            int i = cmbDispositivos.SelectedIndex;
            string NombreVideo = Dispositivos[i].MonikerString;
            MiDispositivo = new VideoCaptureDevice(NombreVideo);
            MiDispositivo.NewFrame += new NewFrameEventHandler(Capturando);
            MiDispositivo.Start();
        }

        #endregion

        #region LECTOR

        public void CargarDispositivos()
        {
            Dispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (Dispositivos.Count > 0)
            {
                HayDispositivos = true;

                foreach (FilterInfo dispositivo in Dispositivos)
                {
                    cmbDispositivos.Items.Add(dispositivo.Name);
                }

                cmbDispositivos.SelectedIndex = 0;
            }
            else
            {
                HayDispositivos = false;
            }
        }

        private void CerrarDispositivo()
        {
            if (MiDispositivo != null && MiDispositivo.IsRunning)
            {
                MiDispositivo.SignalToStop();
                MiDispositivo = null;
                pbEscaner.Image = null;
            }
        }

        private void Capturando(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap imagen = (Bitmap)eventArgs.Frame.Clone();
            BarcodeReader LectorDeBarras = new BarcodeReader();

            if (imagen != null)
            {
                Result resultado = LectorDeBarras.Decode(imagen);

                if (resultado != null)
                {
                    txtEAN.Invoke(new MethodInvoker(delegate ()
                    {
                        txtEAN.Text = resultado.ToString();
                    }));
                }

                pbEscaner.Image = imagen;
            }
        }

        #endregion

        #region TEXTBOX

        // Cuando se detecte el código cierra la cámara
        private void txtEAN_TextChanged(object sender, EventArgs e)
        {
            CerrarDispositivo();
            pbEscaner.Image = null;
        }

        // El primer "IF" es para no obligar al usuario a introducir el dato
        private void txtEAN_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtEAN.Text))
            {
                // Valida que sean números
                if (!Validator.Isnumeric(txtEAN.Text))
                {
                    errorProvider1.SetError(txtEAN, "Solo ingrese números");
                    e.Cancel = true;
                    return;
                }

                if (!(txtEAN.Text.Length == 13))
                {
                    errorProvider1.SetError(txtEAN, "El código EAN-13 tiene 13 dígitos");
                    e.Cancel = true;
                    return;
                }

                if (!IsEAN13(txtEAN.Text))
                {
                    errorProvider1.SetError(txtEAN, "Ingrese un EAN-13 valido");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtEAN, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtCantidad_Validating(object sender, CancelEventArgs e)
        {
            if (Validator.IsWhiteSpace(txtCantidad.Text))
            {
                errorProvider1.SetError(txtCantidad, string.Empty);
                e.Cancel = false;
                return;
            }

            // Valida que sean números
            if (!Validator.Isnumeric(txtCantidad.Text))
            {
                errorProvider1.SetError(txtCantidad, "Solo ingrese números");
                e.Cancel = true;
                return;
            }

            // Valida que no se hayan números menor/igual a 0
            if (!(CantidadValida(txtCantidad.Text)))
            {
                errorProvider1.SetError(txtCantidad, "La cantidad no puede ser menor/igual a cero");
                e.Cancel = true;
                return;
            }

            // Si ha llegado hasta aquí, la validación es exitosa
            errorProvider1.SetError(txtCantidad, string.Empty);
            e.Cancel = false;
        }

        private void txtDocumentoCliente_Validating(object sender, CancelEventArgs e)
        {
            // Valida que haya texto
            if (!Validator.IsWhiteSpace(txtDocumentoCliente.Text))
            {
                // Valida que sea introdujeron números
                if (!Validator.Isnumeric(txtDocumentoCliente.Text))
                {
                    errorProvider1.SetError(txtDocumentoCliente, "Solo ingrese números");
                    e.Cancel = true;
                    return;
                }

                // Valida que no se hayan introducido menos de 8 números
                if (!(txtDocumentoCliente.Text.Length >= 8))
                {
                    txtDocumentoCliente.Focus();
                    errorProvider1.SetError(txtDocumentoCliente, "El documento no puede tener menos de 8 dígitos");
                    e.Cancel = true;
                    return;
                }

                // Valida que no se hayan introducido menos de 10 números
                if (!(txtDocumentoCliente.Text.Length <= 10))
                {
                    errorProvider1.SetError(txtDocumentoCliente, "El documento no puede tener mas de 10 dígitos");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtDocumentoCliente, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtMontoPago_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtMontoPago.Text))
            {
                if (!Validator.IsSalario(txtMontoPago.Text))
                {
                    txtMontoPago.Focus();
                    errorProvider1.SetError(txtMontoPago, "El monto pago solo puede contener números y una coma");
                    e.Cancel = true;
                    return;
                }

                if (Validator.IsWhiteSpace(txtTotal.Text))
                {
                    txtMontoPago.Focus();
                    errorProvider1.SetError(txtMontoPago, "No puede pagar si no se agrego nada al carrito");
                    e.Cancel = true;
                    return;
                }

                if (MontoPagoValido(txtMontoPago.Text, txtTotal.Text))
                {
                    txtMontoPago.Focus();
                    errorProvider1.SetError(txtMontoPago, "El monto pago no puede ser menor al total");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtDocumentoCliente, string.Empty);
                e.Cancel = false;
            }
        }

        #endregion

        #region DATAGRIDVIEW

        private void dgvTicket_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvTicket.Columns["Eliminar"].Index && e.RowIndex >= 0)
            {
                EliminarProductoSeleccionado();
            }
            else if (e.ColumnIndex == dgvTicket.Columns["Modificar"].Index && e.RowIndex >= 0)
            {
                ModificarProductoSeleccionado();
                CargarDatosEnTextBox(e.RowIndex);
            }
            else if (e.RowIndex >= 0)
            {
                CargarDatosEnTextBox(e.RowIndex);
            }
        }

        private void ActualizarLista()
        {
            List<DetalleVenta> lista = new List<DetalleVenta>();
            lista = oNVenta.ConsultarLista();

            dgvTicket.Rows.Clear();

            foreach (var detalle in lista)
            {
                int fila_indice = dgvTicket.Rows.Add();
                DataGridViewRow nueva_fila = dgvTicket.Rows[fila_indice];
                nueva_fila.Cells["ProductoID"].Value = detalle.oProducto.ProductoID;
                nueva_fila.Cells["EAN13"].Value = detalle.oProducto.EAN13;
                nueva_fila.Cells["Nombre"].Value = detalle.oProducto.Nombre;
                nueva_fila.Cells["Descripcion"].Value = detalle.oProducto.Descripcion;
                nueva_fila.Cells["Cantidad"].Value = detalle.Cantidad;
                nueva_fila.Cells["PrecioVenta"].Value = detalle.oProducto.PrecioVenta;
                nueva_fila.Cells["Subtotal"].Value = detalle.SubTotal;
            }
        }

        private void CargarDatosEnTextBox(int rowIndex)
        {
            txtEAN.Text = dgvTicket.Rows[rowIndex].Cells["EAN13"].Value.ToString();
            txtCantidad.Text = dgvTicket.Rows[rowIndex].Cells["Cantidad"].Value.ToString();
            txtPrecioUnitario.Text = dgvTicket.Rows[rowIndex].Cells["PrecioVenta"].Value.ToString();
            txtSubtotal.Text = dgvTicket.Rows[rowIndex].Cells["Subtotal"].Value.ToString();
        }

        private void EliminarProductoSeleccionado()
        {
            if (dgvTicket.SelectedRows.Count > 0)
            {
                DataGridViewRow filaSeleccionada = dgvTicket.SelectedRows[0];
                int indice_fila = dgvTicket.Rows.IndexOf(filaSeleccionada);
                oNVenta.EliminarProducto(indice_fila);
                ActualizarLista();
                CalcularTotal();
            }
        }

        private void ModificarProductoSeleccionado()
        {
            if (dgvTicket.SelectedRows.Count > 0)
            {
                DataGridViewRow fila_seleccionada = dgvTicket.SelectedRows[0];
                int indice_fila = dgvTicket.Rows.IndexOf(fila_seleccionada);
                int nueva_cantidad = int.Parse(txtCantidad.Text);

                oNVenta.ModificarProducto(indice_fila, nueva_cantidad);
                ActualizarLista();
                CalcularTotal();
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

        private bool MontoPagoValido(string _montoPago, string _montoTotal)
        {
            // Eliminar símbolos de dólar y espacios en blanco
            _montoPago = LimpiarMonto(_montoPago);
            _montoTotal = LimpiarMonto(_montoTotal);

            // Convertir a float
            float monto_pago = float.Parse(_montoPago);
            float monto_total = float.Parse(_montoTotal);

            if (monto_pago < monto_total)
            {
                return false;
            }
            return true;
        }

        // Elimina los símbolos de dólar y espacios en blanco de la cadena dada
        private string LimpiarMonto(string monto)
        {
            return Regex.Replace(monto, @"[^\d.]", "");
        }

        #endregion

        #region OTROS

        private void CalcularTotal()
        {
            float total = 0;

            foreach (DataGridViewRow fila in dgvTicket.Rows)
            {
                if (fila.Cells["Subtotal"].Value != null)
                {
                    float subtotal = float.Parse(fila.Cells["Subtotal"].Value.ToString());
                    total += subtotal;
                }
            }

            Total = total;
            txtTotal.Text = total.ToString("C");
        }

        private void LimpiarTxtBox()
        {
            txtCantidad.Text = string.Empty;
            txtEAN.Text = string.Empty;
            txtPrecioUnitario.Text = string.Empty;
            txtSubtotal.Text = string.Empty;
        }
        
        private void FinalizarLimpiar()
        {
            dgvTicket.Rows.Clear();
            
            foreach (TextBox txt in this.Controls.OfType<TextBox>())
            {
                txt.Text = string.Empty;
            }

        }
        
        #endregion
    }
}