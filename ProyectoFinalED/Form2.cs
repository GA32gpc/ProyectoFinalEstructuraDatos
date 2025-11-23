using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoFinalED
{
    public partial class SeleccionMinijuegos : Form
    {
        public SeleccionMinijuegos()
        {
            InitializeComponent();
        }

        private void btnColorSwitch_Click(object sender, EventArgs e)
        {
            var frm = new ColorSwitch();
            frm.FormClosed += (s, args) => this.Close(); // Cierra el splash al cerrar el principal.
            frm.Show();
            this.Hide();
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            var frm = new Inicio();
            frm.FormClosed += (s, args) => this.Close(); // Cierra el splash al cerrar el principal.
            frm.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var frm = new MasterTest();
            frm.FormClosed +=(s, args) => this.Close();
            frm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var frm = new ShadowCommandXbox();
            frm.FormClosed += (s, args) => this.Close();
            frm.Show();
            this.Hide();
        }
    }
}
