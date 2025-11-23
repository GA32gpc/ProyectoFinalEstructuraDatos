using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using SharpDX.XInput;
using System.Threading.Tasks;


namespace ProyectoFinalED
{
    public partial class MasterTest : Form
    {
        // UI
        private Panel pnlDisplay;
        private Label lblNivel;
        private Label lblEstado;
        private Button btnIniciar;
        private ProgressBar pbTiempo;

        // Juego
        private List<Color> secuencia = new List<Color>();
        private List<Color> entradaUsuario = new List<Color>();
        private Random rand = new Random();
        private int nivel = 0;
        private bool esperandoUsuario = false;
        private int tiempoLimiteMs = 4000; // tiempo por entrada (disminuye con niveles)
        private int baseDelayMostrar = 700; // tiempo que se muestra cada color

        // Voz y vibración
        private SpeechSynthesizer voz = new SpeechSynthesizer();
        private SpeechRecognitionEngine recognizer;
        private Controller control = new Controller(UserIndex.One);

        // Timer para límite de entrada
        private Timer timerTiempo = new Timer();

        public MasterTest()
        {
            InitializeComponent();
            this.FormClosing += MasterTest_FormClosing;
            this.Text = "Prueba del Maestro";
            this.Size = new Size(420, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            pnlDisplay = new Panel()
            {
                Size = new Size(300, 150),
                Location = new Point(55, 20),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlDisplay);

            lblNivel = new Label()
            {
                Text = "Nivel: 0",
                Location = new Point(30, 190),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            this.Controls.Add(lblNivel);

            lblEstado = new Label()
            {
                Text = "Pulsa Iniciar o di 'empezar' por voz",
                Location = new Point(30, 220),
                AutoSize = true
            };
            this.Controls.Add(lblEstado);

            btnIniciar = new Button()
            {
                Text = "Iniciar Prueba",
                Location = new Point(250, 190),
                Size = new Size(110, 40)
            };
            btnIniciar.Click += BtnIniciar_Click;
            this.Controls.Add(btnIniciar);

            pbTiempo = new ProgressBar()
            {
                Location = new Point(30, 260),
                Size = new Size(330, 20),
                Maximum = 1000
            };
            this.Controls.Add(pbTiempo);

            // timer para tiempo por entrada (ticks cada 100ms)
            timerTiempo.Interval = 100;
            timerTiempo.Tick += TimerTiempo_Tick;

            // Pre-configurar recognizer (no cargamos grammar de colores aún)
            ConfigurarReconocimiento();
        }

        private void ConfigurarReconocimiento()
        {
            try
            {
                var installed = SpeechRecognitionEngine.InstalledRecognizers();
                var ri = installed.FirstOrDefault(r => r.Culture.Name.StartsWith("es"))
                         ?? installed.FirstOrDefault();

                if (ri == null)
                {
                    MessageBox.Show("No hay reconocedores de voz instalados.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                recognizer = new SpeechRecognitionEngine(ri);

                // Comandos de control globales: empezar / reiniciar / rendirse
                Choices comandos = new Choices(new string[] { "empezar", "reiniciar", "rendirse" });
                Choices colores = new Choices(new string[] { "rojo", "verde", "azul", "amarillo" });

                GrammarBuilder gbComandos = new GrammarBuilder();
                gbComandos.Culture = recognizer.RecognizerInfo.Culture;
                gbComandos.Append(comandos);

                GrammarBuilder gbColores = new GrammarBuilder();
                gbColores.Culture = recognizer.RecognizerInfo.Culture;
                gbColores.Append(colores);

                Grammar gComandos = new Grammar(gbComandos) { Name = "comandos" };
                Grammar gColores = new Grammar(gbColores) { Name = "colores" };

                recognizer.LoadGrammar(gComandos);
                recognizer.LoadGrammar(gColores);

                recognizer.SetInputToDefaultAudioDevice();
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                // muestra cultura para debugging si lo necesitas
                // MessageBox.Show("Recognizer culture: " + recognizer.RecognizerInfo.Culture.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al configurar reconocimiento: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            nivel = 1;
            secuencia.Clear();
            entradaUsuario.Clear();
            lblNivel.Text = "Nivel: " + nivel;
            lblEstado.Text = "Observa la secuencia...";
            // generar primer paso
            secuencia.Add(GenerarColorAleatorio());
            Task.Run(async () =>
            {
                await MostrarSecuencia();
                HabilitarEntrada();
            });
        }

        private Color GenerarColorAleatorio()
        {
            Color[] colores = { Color.Red, Color.Green, Color.Blue, Color.Yellow };
            return colores[rand.Next(colores.Length)];
        }

        private async Task MostrarSecuencia()
        {
            // tiempo por color disminuye con el nivel (más difícil)
            int delayMostrar = Math.Max(220, baseDelayMostrar - (nivel - 1) * 60);

            esperandoUsuario = false;
            await Task.Delay(300);

            foreach (Color c in secuencia)
            {
                // mostrar color
                Invoke(new Action(() => pnlDisplay.BackColor = c));
                DecirColor(c);
                SystemSounds.Beep.Play();
                Vibrar(30000, 250);
                await Task.Delay(delayMostrar);
                Invoke(new Action(() => pnlDisplay.BackColor = Color.Black));
                await Task.Delay(200);
            }
        }

        private void HabilitarEntrada()
        {
            entradaUsuario.Clear();
            esperandoUsuario = true;
            // El tiempo por entrada se reduce con el nivel
            tiempoLimiteMs = Math.Max(1500, 4000 - (nivel - 1) * 400);
            Invoke(new Action(() =>
            {
                lblEstado.Text = "Repite la secuencia - Habla o haz clic en pantalla";
                pbTiempo.Value = pbTiempo.Maximum;
            }));
            timerTiempo.Tag = 0; // contador de ticks
            timerTiempo.Start();
        }

        private void TimerTiempo_Tick(object sender, EventArgs e)
        {
            if (!esperandoUsuario) { timerTiempo.Stop(); return; }

            int elapsed = (int)timerTiempo.Tag;
            elapsed += timerTiempo.Interval;
            timerTiempo.Tag = elapsed;

            double ratio = 1.0 - (double)elapsed / tiempoLimiteMs;
            if (ratio < 0) ratio = 0;
            int bar = (int)(ratio * pbTiempo.Maximum);
            pbTiempo.Value = Math.Max(0, Math.Min(pbTiempo.Maximum, bar));

            if (elapsed >= tiempoLimiteMs)
            {
                // tiempo acabado -> pierdes
                timerTiempo.Stop();
                Invoke(new Action(() => Perder("Se terminó el tiempo.")));
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // strict: requerir buena confianza
            if (e.Result.Confidence < 0.55) return;

            var text = e.Result.Text.ToLower();

            // comandos globales
            if (text == "empezar")
            {
                BeginInvoke(new Action(() =>
                {
                    if (!esperandoUsuario && secuencia.Count == 0) StartGame();
                    else if (!esperandoUsuario && secuencia.Count > 0)
                    {
                        // si estamos mostrando secuencia y reconoció 'empezar' lo ignoramos
                    }
                }));
                return;
            }
            if (text == "reiniciar")
            {
                BeginInvoke(new Action(() => StartGame()));
                return;
            }
            if (text == "rendirse")
            {
                BeginInvoke(new Action(() => Perder("Te rendiste.")));
                return;
            }

            // colores
            if (!esperandoUsuario) return;

            Color c = Color.Empty;
            if (text == "rojo") c = Color.Red;
            if (text == "verde") c = Color.Green;
            if (text == "azul") c = Color.Blue;
            if (text == "amarillo") c = Color.Yellow;

            if (c != Color.Empty)
            {
                BeginInvoke(new Action(() => ProcesarEntrada(c)));
            }
        }

        // Permite entrada por click en panel (útil si micrófono falla)
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            // dividir panel en 4 para permitir selección por clic (opcional)
            if (!esperandoUsuario) return;
            var p = pnlDisplay.PointToClient(Cursor.Position);
            int w = pnlDisplay.Width, h = pnlDisplay.Height;
            Color elegido = Color.Black;

            if (p.Y < h / 2 && p.X < w / 2) elegido = Color.Red;
            else if (p.Y < h / 2 && p.X >= w / 2) elegido = Color.Green;
            else if (p.Y >= h / 2 && p.X < w / 2) elegido = Color.Blue;
            else if (p.Y >= h / 2 && p.X >= w / 2) elegido = Color.Yellow;

            if (elegido != Color.Black)
                ProcesarEntrada(elegido);
        }

        private async void ProcesarEntrada(Color color)
        {
            if (!esperandoUsuario) return;

            entradaUsuario.Add(color);

            int idx = entradaUsuario.Count - 1;

            // mostrar retroalimentación inmediata
            pnlDisplay.BackColor = color;
            DecirColor(color);
            SonidoPitido();
            Vibrar(40000, 180);
            await Task.Delay(250);
            pnlDisplay.BackColor = Color.Black;

            if (secuencia[idx] != entradaUsuario[idx])
            {
                esperandoUsuario = false;
                timerTiempo.Stop();
                Perder("Entrada incorrecta. El maestro te venció.");
                return;
            }

            // si pasa, reiniciar el contador de tiempo para la siguiente entrada
            timerTiempo.Tag = 0;

            if (entradaUsuario.Count == secuencia.Count)
            {
                // nivel completado
                esperandoUsuario = false;
                timerTiempo.Stop();
                nivel++;
                lblNivel.Text = "Nivel: " + nivel;
                lblEstado.Text = "¡Nivel superado! Preparando siguiente ronda...";
                await Task.Delay(800);

                // aumentar la secuencia
                secuencia.Add(GenerarColorAleatorio());
                await MostrarSecuencia();
                HabilitarEntrada();
            }
        }

        private void Perder(string razon)
        {
            esperandoUsuario = false;
            timerTiempo.Stop();

            voz.SpeakAsync("Has sido vencido por el Maestro.");
            MessageBox.Show(razon + "\nLlegaste al nivel " + nivel, "Has Perdido", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            // Opcional: dar opción de reiniciar
            var r = MessageBox.Show("¿Quieres intentarlo de nuevo?", "Reintentar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                StartGame();
            }
            else
            {
                this.Close();
            }
        }

        private void SonidoPitido()
        {
            try { Console.Beep(1200, 100); }
            catch { /* en algunos ambientes no funciona Console.Beep */ }
        }

        private void DecirColor(Color color)
        {
            string texto = "";
            if (color == Color.Red) texto = "Rojo";
            if (color == Color.Green) texto = "Verde";
            if (color == Color.Blue) texto = "Azul";
            if (color == Color.Yellow) texto = "Amarillo";
            voz.SpeakAsync(texto);
        }

        private void Vibrar(int intensidad, int tiempo)
        {
            try
            {
                if (!control.IsConnected) return;
                var motor = new Vibration
                {
                    LeftMotorSpeed = (ushort)Math.Min(65535, Math.Max(0, intensidad)),
                    RightMotorSpeed = (ushort)Math.Min(65535, Math.Max(0, intensidad))
                };
                control.SetVibration(motor);

                Task.Delay(tiempo).ContinueWith(_ =>
                {
                    try { control.SetVibration(new Vibration()); } catch { }
                });
            }
            catch { /* ignora si XInput no está disponible */ }
        }

        private void MasterTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (recognizer != null)
                {
                    recognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
                    recognizer.RecognizeAsyncCancel();
                    recognizer.Dispose();
                    recognizer = null;
                }
            }
            catch { }
        }
    }
}
