using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.Recognition;

namespace ProyectoFinalED
{
    public partial class Inicio : Form
    {
        SpeechSynthesizer voz = new SpeechSynthesizer();
        SpeechRecognitionEngine reconocimiento;
        bool yaCambio = false;

        public Inicio()
        {
            InitializeComponent();

            var installed = SpeechRecognitionEngine.InstalledRecognizers();
            // Intentar obtener es-MX si está disponible, sino usar el primero instalado
            var ri = installed.FirstOrDefault(r => r.Culture.Name == "es-MX") ?? installed.FirstOrDefault();
            if (ri == null)
                throw new InvalidOperationException("No hay reconocedores de voz instalados en el sistema.");

            reconocimiento = new SpeechRecognitionEngine(ri);

            CargarReconocimiento();

            progressBarMorada1.Value = 0;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBarMorada1.Value++;

            if (progressBarMorada1.Value >= 100)
            {
                timer1.Enabled = false;
                btnEmpezar.Visible = true;
            }
        }

        private void btnEmpezar_Click(object sender, EventArgs e)
        {
            voz.SpeakAsync("Iniciando");
            AbrirSiguienteForm();
        }

        private void CargarReconocimiento()
        {
            Choices palabras = new Choices();
            palabras.Add("empezar");

            GrammarBuilder gb = new GrammarBuilder();
            // Asegurar que la gramática usa la misma cultura que el recognizer en tiempo de ejecución
            gb.Culture = reconocimiento.RecognizerInfo.Culture;
            gb.Append(palabras);

            Grammar gramatica = new Grammar(gb);
            reconocimiento.LoadGrammar(gramatica);

            reconocimiento.SetInputToDefaultAudioDevice();
            reconocimiento.SpeechRecognized += Reconocido;
            reconocimiento.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Reconocido(object sender, SpeechRecognizedEventArgs e)
        {
            if (yaCambio)
                return;

            if (e.Result.Text.ToLower() == "empezar")
            {
                yaCambio = true;
                voz.SpeakAsync("Iniciando");
                reconocimiento.RecognizeAsyncCancel();
                AbrirSiguienteForm();
            }
        }

        private void AbrirSiguienteForm()
        {
            var frm = new SeleccionMinijuegos();
            frm.FormClosed += (s, args) => this.Close();
            frm.Show();
            this.Hide();
        }
    }
}



