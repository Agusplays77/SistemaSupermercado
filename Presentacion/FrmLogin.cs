using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using ValidarLibrary; // --> LIBRERIA DE LAS VALIDACIONES
using Entidades; // --> CAPA DONDE ESTAN LAS ENTIDADES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES

namespace Presentacion
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        #region BOTONES

        // El primer "IF" es para verificar que los campos no estén vacíos
        private void btnIngresar_Click(object sender, EventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtDocumento.Text) && !Validator.IsWhiteSpace(txtPassword.Text))
            {
                // Pregunta si el usuario esta en la lista, de lo contrario devuelve null
                Usuario oUsuario = new NUsuario().ListaUsuarios().Where(usuario => usuario.Documento == int.Parse(txtDocumento.Text) && usuario.Clave == txtPassword.Text).FirstOrDefault();

                if (oUsuario != null)
                {
                    Form ventana = new Inicio(oUsuario);
                    ventana.Show();
                    Hide();
                    ventana.FormClosing += FrmClosing;
                }
                else
                {
                    MessageBox.Show("Documento o Contraseña, no validos :(",
                        "UPS...",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    txtDocumento.Text = "";
                    txtPassword.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Complete todos los campos/No se permiten espacios, por favor :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        // Cierra el formulario
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region TEXTBOX

        // Saltar al siguiente txtBox
        private void txtDocumento_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Le pasa el foco al siguiente campo de contraseña
                txtPassword.Focus();
                // Marca la tecla como manejada para evitar que se procese nuevamente
                e.Handled = true;
            }
        }

        // Dispara el evento click del btnIngresar
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Dispara el evento click del btnIngresar
                btnIngresar_Click(sender, e);
                // Marca la tecla como manejada para evitar que se procese nuevamente
                e.Handled = true;
            }
        }

        // Validación de DNI
        private void txtDocumento_Validating(object sender, CancelEventArgs e)
        {
            // Valida que haya texto
            if (!Validator.IsWhiteSpace(txtDocumento.Text))
            {
                // Valida que sea introdujeron números
                if (!Validator.Isnumeric(txtDocumento.Text))
                {
                    errorProvider1.SetError(txtDocumento, "Solo ingrese números");
                    e.Cancel = true;
                    return;
                }

                // Valida que no se hayan introducido menos de 8 números
                if (!(txtDocumento.Text.Length >= 8))
                {
                    errorProvider1.SetError(txtDocumento, "El documento no puede tener menos de 8 dígitos");
                    e.Cancel = true;
                    return;
                }

                // Valida que no se hayan introducido menos de 10 números
                if (!(txtDocumento.Text.Length <= 10))
                {
                    errorProvider1.SetError(txtDocumento, "El documento no puede tener mas de 10 dígitos");
                    e.Cancel = true;
                    return;
                }

                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtDocumento, string.Empty);
                e.Cancel = false;
            }
        }

        #endregion

        #region FORMULARIO

        // Le da el foco al txtDocumento
        private void FrmLogin_Load(object sender, EventArgs e)
        {
            txtDocumento.Focus();
            txtDocumento.Select();
        }

        //Para mostrar el formulario de "Login" cuando cierran el "Inicio" y limpiar los txtBox
        private void FrmClosing(object sender, FormClosingEventArgs e)
        {
            txtDocumento.Text = "";
            txtPassword.Text = "";
            Show();
        }

        #endregion
    }
}