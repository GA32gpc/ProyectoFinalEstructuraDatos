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
using SharpDX.XInput;
using System.Speech.Recognition;


namespace ProyectoFinalED
{
    public partial class ColorSwitch : Form
    {
        //Listas para la secuencia generada y la entrada del usuario
        List<Color> secuencia = new List<Color>();
        List<Color> entradaUsuario = new List<Color>();

        //Variables para el nivel, el estado de espera y la generación de números aleatorios
        Random rand = new Random();
        int nivel = 1;
        bool esperandoUsuario = false;

        //Objeto para la síntesis de voz
        SpeechSynthesizer voz = new SpeechSynthesizer();

        //Constructor para elegir color con la voz
        SpeechRecognitionEngine recognizer;


        public ColorSwitch()
        {
            InitializeComponent();
            MessageBox.Show("¡Bienvenido al juego de Simon Dice!\n\nSigue la secuencia de colores y sonidos repitiéndola correctamente para avanzar de nivel.\n\nPuedes seleccionar los colores haciendo clic en los botones o diciendo el nombre del color en voz alta.\n\n¡Buena suerte!", "Instrucciones", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Color GenerarColorAleatorio()
        {
            Color[] colores = { Color.Red, Color.Green, Color.Blue, Color.Yellow };
            return colores[rand.Next(colores.Length)];
        }

        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            nivel = 1;
            secuencia.Clear();
            entradaUsuario.Clear();

            lblNivel.Text = "Nivel: 1";

            secuencia.Add(GenerarColorAleatorio());
            await MostrarSecuencia();
        }
        //Metodo para mostrar la secuencia de colores al usuario y emitir sonidos y vibraciones
        private async Task MostrarSecuencia()
        {
            esperandoUsuario = false;

            List<Color> copia = new List<Color>(secuencia);

            foreach (Color color in copia)
            {
                System.Media.SystemSounds.Beep.Play();
                PnlColores.BackColor = color;
                await Task.Delay(800);
                PnlColores.BackColor = Color.Black;
                await Task.Delay(300);
            }

            esperandoUsuario = true;
            entradaUsuario.Clear();
            lblEstado.Text = "Repite la secuencia";
            {
                esperandoUsuario = false;

                foreach (Color color in secuencia)
                {
                    PnlColores.BackColor = color;

                    // voz
                    DecirColor(color);

                    // sonido
                    SonidoPitido();

                    // vibración leve
                    Vibrar(20000, 300);

                    await Task.Delay(700);

                    PnlColores.BackColor = Color.Black;
                    await Task.Delay(300);
                }

                esperandoUsuario = true;
                entradaUsuario.Clear();
                lblEstado.Text = "Repite la secuencia";
            }
        }

        private void btnRojo_Click(object sender, EventArgs e)
        {
            if (!esperandoUsuario) return;
            ProcesarEntrada(Color.Red);
            Vibrar(100000, 200);
        }

        private void btnAzul_Click(object sender, EventArgs e)
        {
            if (!esperandoUsuario) return;
            ProcesarEntrada(Color.Blue);
            Vibrar(100000, 200);
        }

        private void btnVerde_Click(object sender, EventArgs e)
        {
            if (!esperandoUsuario) return;
            ProcesarEntrada(Color.Green);
            Vibrar(100000, 200);
        }

        private void btnAmarillo_Click(object sender, EventArgs e)
        {
            if (!esperandoUsuario) return;
            ProcesarEntrada(Color.Yellow);
            Vibrar(100000, 200);

        }
        private void InterfazPrincipal_Load(object sender, EventArgs e)
        {
            {
                var installed = SpeechRecognitionEngine.InstalledRecognizers();
                var ri = installed.FirstOrDefault(r => r.Culture.Name.StartsWith("es"))
                         ?? installed.FirstOrDefault();

                recognizer = new SpeechRecognitionEngine(ri);

                Choices colores = new Choices(new string[] { "rojo", "verde", "azul", "amarillo" });

                GrammarBuilder gb = new GrammarBuilder();
                gb.Culture = recognizer.RecognizerInfo.Culture;   // ← IMPORTANTE
                gb.Append(colores);

                Grammar g = new Grammar(gb);
                recognizer.LoadGrammar(g);

                recognizer.SetInputToDefaultAudioDevice();
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
        }
        //Metodo para reconocer el color dicho por el usuario y procesar la entrada correspondiente
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string c = e.Result.Text;

            if (!esperandoUsuario) return;

            if (c == "rojo") ProcesarEntrada(Color.Red);
            if (c == "verde") ProcesarEntrada(Color.Green);
            if (c == "azul") ProcesarEntrada(Color.Blue);
            if (c == "amarillo") ProcesarEntrada(Color.Yellow);
        }

        private async void ProcesarEntrada(Color color)
        {
            entradaUsuario.Add(color);

            int i = entradaUsuario.Count - 1;

            if (entradaUsuario[i] != secuencia[i])
            {
                esperandoUsuario = false;
                lblEstado.Text = "¡Fallaste! Inténtalo de nuevo.";
                return;
            }
            DecirColor(color);
            SonidoPitido();
            MessageBox.Show("¡Correcto!", "Acierto");

            if (entradaUsuario.Count == secuencia.Count)
            {
                esperandoUsuario = false;
                nivel++;

                lblNivel.Text = "Nivel: " + nivel;
                lblEstado.Text = "¡Bien hecho! Mostrando nueva secuencia...";

                await Task.Delay(1000);

                secuencia.Add(GenerarColorAleatorio());
                await MostrarSecuencia();
            }
        }
        //Para reproducir un pitido que identifique un acierto
        private void SonidoPitido()
        {
            Console.Beep(1200, 120);  
        }

        //Metodo para que una voz diga el color seleccionado por el usuario para confirmar su eleccion
        private void DecirColor(Color color)
        {
            string texto = "";

            if (color == Color.Red) texto = "Rojo";
            if (color == Color.Green) texto = "Verde";
            if (color == Color.Blue) texto = "Azul";
            if (color == Color.Yellow) texto = "Amarillo";

            voz.SpeakAsync(texto);
        }

        Controller control = new Controller(UserIndex.One);

        private void Vibrar(int intensidad, int tiempo)
        {
            if (!control.IsConnected) return;

            // intensidad: 0 a 65535
            var motor = new Vibration
            {
                LeftMotorSpeed = (ushort)intensidad,
                RightMotorSpeed = (ushort)intensidad
            };

            control.SetVibration(motor);

            Task.Delay(tiempo).ContinueWith(_ =>
            {
      control.SetVibration(new Vibration()); // apagamos vibración
            });
        }


    }
}
