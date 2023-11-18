using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text.RegularExpressions;
using ValidarLibrary; // --> LIBRERIA DE LAS VALIDACIONES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES

namespace Presentacion
{
    public partial class FrmUsuarios : Form
    {
        private NUsuario nUsuario = new NUsuario(); // --> Administra los objetos y realiza las consultas de usuarios
        private DataTable dt;
        private bool VerPass = false;
        private bool VerConfirm = false;

        public FrmUsuarios()
        {
            InitializeComponent();
        }

         // Llena el dgvUsuarios y los cmbBox
        private void FrmUsuarios_Load(object sender, EventArgs e)
        {
            // Agrega los roles al cmbRol
            cmbRoles.Items.AddRange(new string[] { "ADMINISTRADOR", "OPERADOR" });
            cmbRoles.SelectedIndex = -1;
            // Carga los datos de los usuarios
            LlenarDGV();
            // Agrega los filtros de búsqueda al cmbFiltro
            AgregarFiltros();
        }

        #region Botones

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            // Valida campos no vacíos
            if (!CamposValidos())
            {
                MessageBox.Show("Uno/varios campos vacíos, por favor complételos :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Pregunta si hay otro usuario con el mismo DNI, mail o teléfono
            if (nUsuario.UsuarioExistente(int.Parse(txtDocumento.Text), txtMail.Text, txtTelefono.Text))
            {
                MessageBox.Show("El usuario ya existe :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                BorrarCampos();
                return;
            }

            int rol = (cmbRoles.SelectedIndex == 0) ? 1 : 2;

            // Pasa los datos para agregar el usuario
            if (nUsuario.AgregarUsuario(rol, txtDocumento.Text, txtNombre.Text, txtMail.Text, txtTelefono.Text, txtPassword.Text))
            {
                MessageBox.Show("Usuario agregado con correctamente",
                    "FELICIDADES!!!",
                    MessageBoxButtons.OK);
                LlenarDGV();
                BorrarCampos();
            }
            else
            {
                MessageBox.Show("No se pudo agregar al usuario, ocurrió un error",
                    "UPS... :(",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                BorrarCampos();
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            DialogResult rpt = MessageBox.Show("¿Está seguro de que desea eliminar a " +
                dgvUsuarios.CurrentRow.Cells[2].Value.ToString() + "?",
                string.Empty,
                MessageBoxButtons.YesNo);

            if (rpt == DialogResult.Yes)
            {
                // ID para eliminar al usuario
                int usuarioID = int.Parse(dgvUsuarios.CurrentRow.Cells[0].Value.ToString());

                // Elimina al usuario y devuelve la rpt
                if (nUsuario.EliminarUsuario(usuarioID))
                {
                    MessageBox.Show("Se elimino al usuario correctamente",
                        "FELICIDADES!!!",
                        MessageBoxButtons.OK);
                    LlenarDGV();
                    BorrarCampos();
                }
                else
                {
                    MessageBox.Show("No se ha podido eliminar al usuario",
                        "UPS... :)",
                        MessageBoxButtons.OK);
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            // Valida campos no vacíos
            if (!CamposValidos())
            {
                MessageBox.Show("Uno/varios campos vacíos, por favor complételos :)",
                    "UPS...",
                    MessageBoxButtons.OK);
                return;
            }

            DialogResult rpt = MessageBox.Show("¿Está seguro de que desea modificar los datos de " +
                dgvUsuarios.CurrentRow.Cells[2].Value.ToString() + "?",
                string.Empty,
                MessageBoxButtons.YesNo);

            if (rpt != DialogResult.Yes)
            {
                return;
            }

            int usuarioID = int.Parse(dgvUsuarios.CurrentRow.Cells[0].Value.ToString()); // UsuarioID

            if (nUsuario.DatosDuplicados(usuarioID, int.Parse(txtDocumento.Text), txtMail.Text, txtTelefono.Text))
            {
                MessageBox.Show("Otro usuario ya tiene el mismo documento, mail o teléfono",
                    "UPS... :(",
                    MessageBoxButtons.OK);
                return;
            } // Pregunta si están duplicado los datos

            int rol = cmbRoles.SelectedIndex == 0 ? 1 : 2;

            if (nUsuario.ModificarUsuario(usuarioID, rol, txtNombre.Text, txtMail.Text, txtTelefono.Text, txtPassword.Text))
            {
                MessageBox.Show("Se modificaron los datos del usuario :)",
                    "FELICIDADES!!!",
                    MessageBoxButtons.OK);
                BorrarCampos();
                LlenarDGV();
            } // Modifica, actualiza lista y limpia textbox
            else
            {
                MessageBox.Show("Seleccione al usuario en la lista",
                    string.Empty,
                    MessageBoxButtons.OK);
            } // No se pudo modificar el usuario
        }

        // Para ver la contraseña
        private void btnVerPass_Click(object sender, EventArgs e)
        {
            if (VerPass)
            {
                txtPassword.PasswordChar = '*';
                VerPass = false;
            }
            else
            {
                txtPassword.PasswordChar = '\0';
                VerPass = true;
            }
        }

        // Para ver la confirmación
        private void btnVerConfirm_Click(object sender, EventArgs e)
        {
            if (VerConfirm)
            {
                txtConfirmPass.PasswordChar = '*';
                VerPass = false;
            }
            else
            {
                txtConfirmPass.PasswordChar = '\0';
                VerConfirm = true;
            }
        }

        //Realiza la búsqueda del usuario por filtro y texto
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBuscar.Text) || cmbFiltro.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un filtro y escriba algo :)",
                    "Como buscar?",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            dt = new DataTable();
            string filtro = cmbFiltro.SelectedItem.ToString();
            string buscar = txtBuscar.Text;

            //Realiza la búsqueda  y devuelve un datatable
            dt = nUsuario.RealizarBusqueda(filtro, buscar);

            // Limpia el dgvUsuarios y agrega los usuarios encontrados acorde a la búsqueda
            if (dt.Rows.Count > 0 && dt != null)
            {
                // Limpia las filas
                dgvUsuarios.Rows.Clear();

                foreach (DataRow dr in dt.Rows)
                {
                    string descripcion = ((int)dr["RolID"] == 1) ? "ADMINISTRADOR" : "OPERADOR";

                    dgvUsuarios.Rows.Add(
                        dr["UsuarioID"],
                        dr["Documento"],
                        dr["Nombre"],
                        dr["Mail"],
                        dr["RolID"],
                        descripcion,
                        dr["Telefono"],
                        dr["Clave"]);
                }
            }
            else
            {
                MessageBox.Show("No sea ha encontrado ningún usuario " + buscar + " por" + filtro + " pruebe con otras palabreas o seleccione un filtro diferente :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion

        #region DGV

        // Muestra los usuarios cargados en la base de datos
        private void LlenarDGV()
        {
            dt = new DataTable();

            dgvUsuarios.Rows.Clear(); // Limpiar filas
            dt = nUsuario.UsuariosCargados();

            // Agrega los usuarios del repositorio
            if (dt.Rows.Count > 0 && dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    // Evalúa ROLID y asigna el valor correspondiente a Descripción
                    string descripcion = ((int)dr["RolID"] == 1) ? "ADMINISTRADOR" : "OPERADOR";

                    dgvUsuarios.Rows.Add(
                        dr["UsuarioID"],
                        dr["Documento"],
                        dr["Nombre"],
                        dr["Mail"],
                        dr["RolID"],
                        descripcion,
                        dr["Telefono"],
                        dr["Clave"]);
                }
            }
            else
            {
                MessageBox.Show("No hay usuarios cargados :(",
                    "UPS...",
                    MessageBoxButtons.OK);
            }
        }

        // Para cargar los datos de los usuarios en los textbox
        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtDocumento.Text = dgvUsuarios.CurrentRow.Cells[1].Value.ToString();
            txtNombre.Text = dgvUsuarios.CurrentRow.Cells[2].Value.ToString();
            txtMail.Text = dgvUsuarios.CurrentRow.Cells[3].Value.ToString();

            // obtiene el RolID
            int rol = Convert.ToInt32(dgvUsuarios.CurrentRow.Cells[4].Value.ToString());
            // Se evalúa y cambia el cmbBox
            cmbRoles.SelectedIndex = (rol == 1) ? cmbRoles.SelectedIndex = 0 : cmbRoles.SelectedIndex = 1;

            txtTelefono.Text = dgvUsuarios.CurrentRow.Cells[6].Value.ToString();
            txtPassword.Text = dgvUsuarios.CurrentRow.Cells[7].Value.ToString();
        }

        #endregion

        #region Textbox

        /*El primer "IF" de los eventos Validating es para que
          valide el campo solo cuando se ingrese texto*/

        private void txtDocumento_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtDocumento.Text))
            {
                // Valida que sean numeros
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

        private void txtNombre_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtNombre.Text))
            {
                // Valida solo letras
                if (!Validator.IsAlphabetic(txtNombre.Text))
                {
                    errorProvider1.SetError(txtNombre, "El nombre solo puede contener letras");
                    e.Cancel = true;
                    return;
                }

                // Valida que haya al menos mas de 2 letras
                if (txtNombre.Text.Length >= 2)
                {
                    errorProvider1.SetError(txtNombre, "El nombre debe tener mas de 2 letras");
                    e.Cancel = true;
                    return;
                }

                errorProvider1.SetError(txtNombre, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtMail_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtMail.Text))
            {
                // Valida que por lo menos tenga mas de 6 caracteres
                if (txtMail.Text.Length >= 6)
                {
                    errorProvider1.SetError(txtMail, "Un mail debe tener mas de 5 caracteres");
                    e.Cancel = true;
                    return;
                }

                // Valida que sea un correo
                if (!Validator.IsEmail(txtMail.Text))
                {
                    errorProvider1.SetError(txtMail, "El mail ingresado debe tener formato mail");
                    e.Cancel = true;
                    return;
                }
                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtMail, string.Empty);
                e.Cancel = false;
            }
        }

        // Método de validación abajo
        private void txtTelefono_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtTelefono.Text))
            {
                // Valida que tenga el formato especificado
                if (!IsTelefono(txtTelefono.Text))
                {
                    errorProvider1.SetError(txtTelefono, "Celular no valido, ej: 0[opcional] 5555-555555");
                    e.Cancel = true;
                    return;
                }
                // Si ha llegado hasta aquí, la validación es exitosa
                errorProvider1.SetError(txtTelefono, string.Empty);
                e.Cancel = false;
            }
        }

        // No caracteres especiales ni espacios en blanco
        private void txtPassword_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtPassword.Text))
            {
                // Valida que sea alfanumérico
                if (!Validator.IsAlphaNumeric(txtPassword.Text))
                {
                    errorProvider1.SetError(txtPassword, "No se permiten caracteres especiales");
                    e.Cancel = true;
                }

                // Valida que no sea nulo o vacío
                if (Validator.IsWhiteSpace(txtPassword.Text))
                {
                    errorProvider1.SetError(txtPassword, "no puede haber espacios en blanco");
                    e.Cancel = true;
                    return;
                }

                errorProvider1.SetError(txtPassword, string.Empty);
                e.Cancel = false;
            }
        }

        // Valida que las contraseñas coincidan
        private void txtConfirmPass_Validating(object sender, CancelEventArgs e)
        {
            if (!Validator.IsWhiteSpace(txtConfirmPass.Text))
            {
                // Valida que las contraseñas coincidan
                if (txtPassword.Text != txtConfirmPass.Text)
                {
                    errorProvider1.SetError(txtConfirmPass, "Las contraseñas no coinciden");
                    e.Cancel = true;
                    return;
                }

                // Valida que no este en blanco
                if (!Validator.IsWhiteSpace(txtConfirmPass.Text))
                {
                    errorProvider1.SetError(txtConfirmPass, "La contraseña no puede estar en blanco");
                    e.Cancel = true;
                    return;
                }

                // Valida que tenga igual o mayor a 5 caracteres
                if (!(txtConfirmPass.Text.Length >= 5))
                {
                    errorProvider1.SetError(txtConfirmPass, "La contraseña no puede tener 1 caracter");
                    e.Cancel = true;
                    return;
                }

                errorProvider1.SetError(txtConfirmPass, string.Empty);
                e.Cancel = false;
            }
        }

        // Si no hay texto carga los datos del repositorio
        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBuscar.Text))
            {
                LlenarDGV();
            }
        }

        #endregion

        #region Validaciones

        // Valida que las campos no estén vacíos
        private bool CamposValidos()
        {
            foreach (TextBox txt in Controls.OfType<TextBox>())
            {
                // Ignora el textbox de busqueda
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

        private bool IsTelefono(string _numeroTelefono)
        {
            return Regex.IsMatch(_numeroTelefono, @"^0?\s?\d{4}\s?-\s?\d{6}$");
        }

        #endregion

        #region OTROS

        //Borra los textbox y cambia el índice del combobox de roles
        private void BorrarCampos()
        {
            foreach (TextBox txt in Controls.OfType<TextBox>())
            {
                txt.Clear();
            }

            cmbRoles.SelectedIndex = -1;
        }

        // Agrega los filtros de busqueda
        private void AgregarFiltros()
        {
            cmbFiltro.Items.AddRange(new string[] { "Documento", "Nombre", "Mail", "Rol", "Telefono" });
        }

        #endregion
    }
}