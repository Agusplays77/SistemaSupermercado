using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using AForge.Video.DirectShow; // --> CAMARAS

namespace Presentacion
{
    public partial class FrmCamaras : Form
    {
        private bool HayDispositivos; // Verifica si hay dispositivos
        private FilterInfoCollection ListaDispositivos; // Colección de las camaras que detecta windows
        private List<VideoCaptureDevice> ListaCamaras; // Lista de cámaras
        private List<VideoCaptureDevice> ListaCamarasCorriendo; // Lista de cámaras corriendo

        public FrmCamaras()
        {
            InitializeComponent();
        }

        private void FrmCamaras_Load(object sender, EventArgs e)
        {
            CargarDispositivos();
        }

        //AGREGRAR BOTON PARA REINTENTAR LA CARGA Y BUSQUEDA DE DISPOSITIVOS

        // Carga los dispositivos registrados por el sistema
        private void CargarDispositivos()
        {
            ListaDispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (ListaDispositivos.Count > 0 ) // Consulta si encontró dispositivos conectados
            {
                HayDispositivos = true;
                ListaCamaras = new List<VideoCaptureDevice>();
                ListaCamarasCorriendo = new List<VideoCaptureDevice>();

                for (int i = 0; i < ListaDispositivos.Count; i++)
                {
                    cmbCamaras1.Items.Add(ListaDispositivos[i].Name);
                    cmbCamaras2.Items.Add(ListaDispositivos[i].Name);
                    cmbCamaras3.Items.Add(ListaDispositivos[i].Name);
                    cmbCamaras4.Items.Add(ListaDispositivos[i].Name);

                    ListaCamaras.Add(new VideoCaptureDevice(ListaDispositivos[i].MonikerString));
                }
            }
            else
            {
                HayDispositivos = false;
                MessageBox.Show("No se encontraron dispositivos conectados, debe conectar un dispositivo de video :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        // Cierra las cámaras encendidas
        private void CerrarDispositivos()
        {
            foreach (var camara in ListaCamaras)
            {
                if (camara != null && camara.IsRunning)
                {
                    camara.SignalToStop();
                    camara.WaitForStop();
                }
            }
        }

        // Maneja cámaras e inicializa
        public void IniciarCamara(int _cmbIndice, PictureBox _pbCamara)
        {
            int i = _cmbIndice;

            if (i < 0 || i >= ListaCamaras.Count)
            {
                MessageBox.Show("Seleccione un dispositivo valido. :)",
                    "UPS...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string nombre_camara = ListaDispositivos[i].MonikerString;
            VideoCaptureDevice mi_camara = ListaCamaras[i];
            ListaCamarasCorriendo.Add(mi_camara);

            mi_camara.NewFrame += (sender, e) =>
            {
                Bitmap imagen = (Bitmap)e.Frame.Clone();
                _pbCamara.Image = imagen;
            };

            mi_camara.Start();
        }

        #region Llamada inicio de camara

        private void btnEncender1_Click(object sender, EventArgs e)
        {
            IniciarCamara(cmbCamaras1.SelectedIndex, pbCamara1);
        }

        private void btnEncender2_Click(object sender, EventArgs e)
        {
            IniciarCamara(cmbCamaras2.SelectedIndex, pbCamara2);
        }

        private void btnEncender3_Click(object sender, EventArgs e)
        {
            IniciarCamara(cmbCamaras3.SelectedIndex, pbCamara3);
        }

        private void btnEncender4_Click(object sender, EventArgs e)
        {
            IniciarCamara(cmbCamaras4.SelectedIndex, pbCamara4);
        }

        #endregion

        // Cierra las cámaras cuando se cierra el formulario
        private void FrmCamaras_FormClosed(object sender, FormClosedEventArgs e)
        {
            CerrarDispositivos();
        }
    }
}