using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FontAwesome.Sharp; // ICONOS DE LOS BOTONES
using Entidades; // --> CAPA DONDE ESTAN LAS ENTIDADES
using Negocio; // --> DONDE SE MANEJAN LAS SOLICITUDES Y ACCIONES

namespace Presentacion
{
    public partial class Inicio : Form
    {
        private static Usuario UsuarioActual; // --> Para guardar al usuario actualmente logeado
        private static IconButton MenuActivo; // --> Para guardar la opción del menú en el que esta
        private static Form FormularioActivo; // --> Para guardar el formulario en el que esta
        private List<Permiso> permisos; // --> Lista para los permisos del usuario
        private bool MenuValido = true; // --> bandera para los menús, muestra mensaje al operador de ser necesario
        private NPermiso cnPermiso = new NPermiso();

        // Recibe el usuario y lo asigna a la variable estática
        public Inicio(Usuario _usuario)
        {
            UsuarioActual = _usuario;
            InitializeComponent();
        }

        // Consulta los permisos de usuario, se muestra el usuario logeado y registra fecha de entrada
        private void Inicio_Load(object sender, EventArgs e)
        {
            permisos = new List<Permiso>(); // --> Consulta de los permisos
            permisos = new NPermiso().ListaPermisos(UsuarioActual.UsuarioID); // --> Lista los permisos del Usuario
            
            // Recorre los botones del pnlMenú
            foreach (IconButton menu in pnlMenu.Controls.OfType<IconButton>())
            {
                // Pregunta si encontro el permiso para el menú
                bool encontrado = permisos.Any(m => m.NombreMenu == menu.Name);

                // Si no lo encuentra deshabilita el menú cambia y la bandera para mostrar el mensaje
                if (!encontrado)
                {
                    menu.Enabled = false;
                    MenuValido = false;
                }
            }

            if (!MenuValido)
            {
                MessageBox.Show("Solo puede acceder a los opciones para empleados",
                    string.Empty,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            //Registra el inicio de sesión
            cnPermiso.RegistrarInicio(UsuarioActual.UsuarioID.ToString());

            // Nombre y Rol del usuario logeado
            lblUsuarioActual.Text = UsuarioActual.Nombre;
            lblUsuarioRol.Text = (UsuarioActual.oRol.RolID == 1) ? "ADMINISTRADOR" : "OPERADOR";

            // Abre el formulario de ventas
            AbrirFormulario(btnVender, new FrmVender(UsuarioActual));
        }

        // Registra la fecha de salida
        private void Inicio_FormClosing(object sender, FormClosingEventArgs e)
        {
            cnPermiso.RegistrarCierre(UsuarioActual.UsuarioID.ToString());
        }

        #region BOTONES

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnMaximizar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            btnMaximizar.Visible = false;
            btnRestaurar.Visible = true;
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            btnMaximizar.Visible = true;
            btnRestaurar.Visible = false;
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconButton)sender, new FrmUsuarios());
        }

        private void btnProveedor_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconButton)sender, new FrmProveedores());
        }

        private void btnVender_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconButton)sender, new FrmVender(UsuarioActual));
        }

        private void btnArticulos_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconButton)sender, new FrmProductos());
        }

        private void btnPrecios_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconButton)sender, new FrmPrecios());
        }

        private void btnCamaras_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconButton)sender, new FrmCamaras());
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconButton)sender, new FrmReportes());
        }

        #endregion

        #region IMPLEMENTACION DEL ARRASTRE

        //IMPORTAR FUNCIONES DE "user32.dll".

        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        // Método que se llama cuando se hace clic y se mantiene presionado en el panel.
        private void pnlControles_MouseDown(object sender, MouseEventArgs e)
        {
            // Liberar la captura del mouse.
            ReleaseCapture();
            // Enviar un mensaje a la ventana actual para simular el arrastre de la ventana.
            // Los valores específicos 0x112 y 0xf012 indican que se debe iniciar el proceso de arrastre de la ventana.
            SendMessage(Handle, 0x112, 0xf012, 0);
        }

        #endregion

        //Evento para mostrar los formularios en el contenedor de "Inicio"
        private void AbrirFormulario(IconButton _menu, Form _formulario)
        {
            // Para cambiar el color del anterior menú seleccionado y dejarlo como el del fondo
            if (MenuActivo != null)
            {
                MenuActivo.BackColor = Color.FromArgb(26, 32, 40);
            }

            // Cambia el color del menú seleccionado y actualiza el menuActivo
            _menu.BackColor = Color.SteelBlue;
            MenuActivo = _menu;

            // Para cerrar el anterior formulario activo
            if (FormularioActivo != null)
            {
                FormularioActivo.Close();
            }

            // Le paso el formulario que quiere abrir
            FormularioActivo = _formulario;
            // Le digo que no se muestre como ventana superior
            _formulario.TopLevel = false;
            // Le saco los bordes
            _formulario.FormBorderStyle = FormBorderStyle.None;
            // Le digo que complete el contenedor
            _formulario.Dock = DockStyle.Fill;

            // Lo agrego al pnlContenedor
            pnlContenedor.Controls.Add(_formulario);
            _formulario.Show();
        }

    }
}