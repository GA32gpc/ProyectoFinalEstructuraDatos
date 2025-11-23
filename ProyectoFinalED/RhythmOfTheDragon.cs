using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.XInput;

namespace ProyectoFinalED
{
    public partial class ShadowCommandXbox : Form
    {
        // Controller
        private Controller controller = new Controller(UserIndex.One);

        // UI
        private PictureBox picShadow;
        private Label lblAction;
        private ProgressBar pbTimer;
        private ProgressBar pbPlayerLife;
        private ProgressBar pbEnemyLife;
        private Label lblPlayerLife;
        private Label lblEnemyLife;
        private Label lblHint;
        private Timer gameTimer;
        private Random rnd = new Random();

        // Game state
        private int playerLife = 100;
        private int enemyLife = 200;
        private int actionTimeMs = 2400;
        private int elapsedMs = 0;
        private bool waitingForInput = false;
        private ShadowAction currentAction = ShadowAction.None;
        private bool paused = false;

        // vibration helper
        private bool vibrating = false;

        // Default image
        private readonly string defaultShadowImage = @"C:\Users\monca\Downloads\pne\ProyectoFinalED\ProyectoFinalED\bin\Debug\Un dragon rudo.jpg";

        private enum ShadowAction
        {
            None,
            Attack,
            Charge,
            Guard,
            Vanish
        }

        public ShadowCommandXbox()
        {
            InitializeComponent();
            StartRound();

            this.Text = "Shadow Command - Xbox Control";
            this.ClientSize = new Size(900, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(10, 10, 18);

            // -------- IMAGEN PRINCIPAL (DRAGÓN / SOMBRA) --------
            picShadow = new PictureBox()
            {
                Size = new Size(420, 300),
                Location = new Point((this.ClientSize.Width - 420) / 2, 20),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            TryLoadImage(picShadow, defaultShadowImage);
            this.Controls.Add(picShadow);

            // -------- TEXTO DE ACCIÓN --------
            lblAction = new Label()
            {
                Text = "",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                Size = new Size(this.ClientSize.Width, 60),
                Location = new Point(0, 340),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblAction);

            // -------- BARRA DE TIEMPO --------
            pbTimer = new ProgressBar()
            {
                Size = new Size(700, 18),
                Location = new Point((this.ClientSize.Width - 700) / 2, 410),
                Maximum = 1000,
                Value = 1000
            };
            this.Controls.Add(pbTimer);

            // -------- VIDA ENEMIGO --------
            pbEnemyLife = new ProgressBar()
            {
                Size = new Size(420, 18),
                Location = new Point((this.ClientSize.Width - 420) / 2, 0),
                Maximum = 200,
                Value = enemyLife
            };
            lblEnemyLife = new Label()
            {
                Text = $"Sombra: {enemyLife}",
                ForeColor = Color.OrangeRed,
                Location = new Point(pbEnemyLife.Left, pbEnemyLife.Bottom + 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(pbEnemyLife);
            this.Controls.Add(lblEnemyLife);

            // -------- VIDA DEL JUGADOR --------
            pbPlayerLife = new ProgressBar()
            {
                Size = new Size(200, 14),
                Location = new Point(20, 460),
                Maximum = 100,
                Value = playerLife
            };
            lblPlayerLife = new Label()
            {
                Text = $"Tu vida: {playerLife}",
                ForeColor = Color.LimeGreen,
                Location = new Point(pbPlayerLife.Left, pbPlayerLife.Bottom + 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            this.Controls.Add(pbPlayerLife);
            this.Controls.Add(lblPlayerLife);

            // -------- INDICACIONES DEL CONTROL --------
            lblHint = new Label()
            {
                Text = "Atacar: A (Fuerte)   |   Defender: B (Bloquear)   |   Romper carga: X   |   Rastrear: Y",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Size = new Size(this.ClientSize.Width, 22),
                Location = new Point(0, 480),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblHint);

            // -------- TIMER PRINCIPAL --------
            gameTimer = new Timer();
            gameTimer.Interval = 40;
            gameTimer.Tick += GameTimer_Tick;

            // -------- EVENTO AL CERRAR --------
            this.FormClosing += (s, e) =>
            {
                try { StopVibration(); } catch { }
                try { gameTimer.Stop(); } catch { }
            };

            // -------- EMPEZAR LA LÓGICA DEL JUEGO --------
            StartRound();   // AHORA SÍ, AQUÍ ES CORRECTO
        }

        private void TryLoadImage(PictureBox pb, string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                    pb.Image = Image.FromFile(path);
            }
            catch { }
        }

        private void StartRound()
        {
            playerLife = 100;
            enemyLife = 200;
            actionTimeMs = 2400;
            elapsedMs = 0;
            pbTimer.Value = pbTimer.Maximum;
            pbEnemyLife.Maximum = 200;
            pbEnemyLife.Value = enemyLife;
            pbPlayerLife.Value = playerLife;
            lblEnemyLife.Text = $"Sombra: {enemyLife}";
            lblPlayerLife.Text = $"Tu vida: {playerLife}";
            lblAction.Text = "Prepárate...";
            waitingForInput = false;
            gameTimer.Start();
            Task.Delay(1000).ContinueWith(_ => BeginNewAction());
        }

        private void BeginNewAction()
        {
            var a = (ShadowAction)rnd.Next(1, 5);
            currentAction = a;
            elapsedMs = 0;
            actionTimeMs = Math.Max(1000, 2400 - (200 - enemyLife));
            string actionText = ActionText(a);
            BeginInvoke(new Action(() =>
            {
                lblAction.Text = actionText;
                lblAction.ForeColor = Color.Orange;
                pbTimer.Value = pbTimer.Maximum;
            }));
            waitingForInput = true;
        }

        private string ActionText(ShadowAction a)
        {
            switch (a)
            {
                case ShadowAction.Attack: return "¡Ataca! (Bloquear)";
                case ShadowAction.Charge: return "¡Cargando! (Romper)";
                case ShadowAction.Guard: return "¡Se blinda! (Golpe fuerte)";
                case ShadowAction.Vanish: return "¡Se desvanece! (Rastrear)";
                default: return "";
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (paused) return;

            if (waitingForInput && currentAction != ShadowAction.None)
            {
                elapsedMs += gameTimer.Interval;
                int remaining = Math.Max(0, actionTimeMs - elapsedMs);
                double ratio = remaining / (double)actionTimeMs;
                int barVal = (int)(ratio * pbTimer.Maximum);
                pbTimer.Value = Math.Max(0, Math.Min(pbTimer.Maximum, barVal));

                if (ratio < 0.33) lblAction.ForeColor = Color.Red;
                else if (ratio < 0.66) lblAction.ForeColor = Color.Yellow;
                else lblAction.ForeColor = Color.Orange;

                if (remaining <= 0)
                {
                    waitingForInput = false;
                    OnPlayerFailed("No respondiste a tiempo");
                    Task.Delay(700).ContinueWith(_ => BeginNewAction());
                    return;
                }
            }

            ReadController();
        }

        private void ReadController()
        {
            try
            {
                if (!controller.IsConnected) return;
                var state = controller.GetState();
                var buttons = state.Gamepad.Buttons;

                if (waitingForInput)
                {
                    if ((buttons & GamepadButtonFlags.A) == GamepadButtonFlags.A)
                    {
                        HandlePlayerInput(ShadowAction.Guard);
                        return;
                    }
                    if ((buttons & GamepadButtonFlags.B) == GamepadButtonFlags.B)
                    {
                        HandlePlayerInput(ShadowAction.Attack);
                        return;
                    }
                    if ((buttons & GamepadButtonFlags.X) == GamepadButtonFlags.X)
                    {
                        HandlePlayerInput(ShadowAction.Charge);
                        return;
                    }
                    if ((buttons & GamepadButtonFlags.Y) == GamepadButtonFlags.Y)
                    {
                        HandlePlayerInput(ShadowAction.Vanish);
                        return;
                    }
                }
                else
                {
                    if ((buttons & GamepadButtonFlags.A) == GamepadButtonFlags.A)
                    {
                        BeginNewAction();
                    }
                }
            }
            catch { }
        }

        private void HandlePlayerInput(ShadowAction pressed)
        {
            if (!waitingForInput) return;
            waitingForInput = false;

            if (pressed == ExpectedInputFor(currentAction))
            {
                int dmg = DamageForSuccess(currentAction);
                enemyLife = Math.Max(0, enemyLife - dmg);
                pbEnemyLife.Value = enemyLife;
                lblEnemyLife.Text = $"Sombra: {enemyLife}";
                lblAction.Text = "¡ACERTASTE! -" + dmg;
                lblAction.ForeColor = Color.Lime;
                TryVibrate(35000, 180);
                FlashPicture(Color.FromArgb(160, Color.Red), 220);
                actionTimeMs = Math.Max(900, actionTimeMs - 40);
                Task.Delay(600).ContinueWith(_ => BeginNewAction());
            }
            else
            {
                OnPlayerFailed("Botón incorrecto");
                Task.Delay(700).ContinueWith(_ => BeginNewAction());
            }
        }

        private ShadowAction ExpectedInputFor(ShadowAction act)
        {
            return act;
        }

        private int DamageForSuccess(ShadowAction act)
        {
            switch (act)
            {
                case ShadowAction.Attack: return 20;
                case ShadowAction.Charge: return 28;
                case ShadowAction.Guard: return 24;
                case ShadowAction.Vanish: return 30;
                default: return 0;
            }
        }

        private void OnPlayerFailed(string reason)
        {
            int dmg = 18;
            playerLife = Math.Max(0, playerLife - dmg);
            pbPlayerLife.Value = playerLife;
            lblPlayerLife.Text = $"Tu vida: {playerLife}";
            lblAction.Text = reason + " - Daño recibido -" + dmg;
            lblAction.ForeColor = Color.OrangeRed;
            TryVibrate(22000, 300);
            FlashPicture(Color.FromArgb(120, Color.Orange), 300);
            CheckEndConditions();
        }

        private void CheckEndConditions()
        {
            if (enemyLife <= 0)
            {
                gameTimer.Stop();
                MessageBox.Show("¡Has vencido a la Sombra! 🎉", "Victoria", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            if (playerLife <= 0)
            {
                gameTimer.Stop();
                MessageBox.Show("Te derrotó la Sombra... 😵", "Derrota", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();
            }
        }

        private async void FlashPicture(Color overlay, int ms)
        {
            try
            {
                var bmp = new Bitmap(picShadow.Width, picShadow.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    if (picShadow.Image != null)
                        g.DrawImage(picShadow.Image, 0, 0, picShadow.Width, picShadow.Height);
                    using (Brush b = new SolidBrush(Color.FromArgb(120, overlay)))
                        g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);
                }
                var old = picShadow.Image;
                picShadow.Image = bmp;
                await Task.Delay(ms);
                TryLoadImage(picShadow, defaultShadowImage);
                old?.Dispose();
            }
            catch { }
        }

        private void TryVibrate(int intensity, int timeMs)
        {
            if (vibrating) return;
            try
            {
                if (!controller.IsConnected) return;
                vibrating = true;
                var v = new Vibration()
                {
                    LeftMotorSpeed = (ushort)Math.Min(65535, intensity),
                    RightMotorSpeed = (ushort)Math.Min(65535, intensity)
                };
                controller.SetVibration(v);
                Task.Delay(timeMs).ContinueWith(_ =>
                {
                    try { controller.SetVibration(new Vibration()); } catch { }
                    vibrating = false;
                });
            }
            catch { vibrating = false; }
        }

        // NUEVO: método para detener vibración (soluciona CS0103)
        private void StopVibration()
        {
            try
            {
                if (controller != null && controller.IsConnected)
                    controller.SetVibration(new Vibration());
            }
            catch { }
            vibrating = false;
        }

        public void PauseGame() { paused = true; gameTimer.Stop(); }
        public void ResumeGame() { paused = false; gameTimer.Start(); }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!gameTimer.Enabled) gameTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            try { StopVibration(); } catch { }
            try { gameTimer.Stop(); } catch { }
        }
    }
}