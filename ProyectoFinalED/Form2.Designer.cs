namespace ProyectoFinalED
{
    partial class SeleccionMinijuegos
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnColorSwitch = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnMasterTest = new System.Windows.Forms.Button();
            this.btnVolver = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnColorSwitch
            // 
            this.btnColorSwitch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnColorSwitch.Location = new System.Drawing.Point(23, 78);
            this.btnColorSwitch.Name = "btnColorSwitch";
            this.btnColorSwitch.Size = new System.Drawing.Size(368, 226);
            this.btnColorSwitch.TabIndex = 0;
            this.btnColorSwitch.Text = "Ir a ColorSwitch";
            this.btnColorSwitch.UseVisualStyleBackColor = true;
            this.btnColorSwitch.Click += new System.EventHandler(this.btnColorSwitch_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(425, 78);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(368, 226);
            this.button2.TabIndex = 1;
            this.button2.Text = "Sensorial Labyrint";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnMasterTest
            // 
            this.btnMasterTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMasterTest.Location = new System.Drawing.Point(243, 329);
            this.btnMasterTest.Name = "btnMasterTest";
            this.btnMasterTest.Size = new System.Drawing.Size(368, 226);
            this.btnMasterTest.TabIndex = 2;
            this.btnMasterTest.Text = "MasterTest";
            this.btnMasterTest.UseVisualStyleBackColor = true;
            this.btnMasterTest.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnVolver
            // 
            this.btnVolver.Location = new System.Drawing.Point(808, 673);
            this.btnVolver.Name = "btnVolver";
            this.btnVolver.Size = new System.Drawing.Size(131, 71);
            this.btnVolver.TabIndex = 3;
            this.btnVolver.Text = "Volver a Inicio";
            this.btnVolver.UseVisualStyleBackColor = true;
            this.btnVolver.Click += new System.EventHandler(this.btnVolver_Click);
            // 
            // SeleccionMinijuegos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 756);
            this.Controls.Add(this.btnVolver);
            this.Controls.Add(this.btnMasterTest);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnColorSwitch);
            this.Name = "SeleccionMinijuegos";
            this.Text = "Seleccion de Minijuegos";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnColorSwitch;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnMasterTest;
        private System.Windows.Forms.Button btnVolver;
    }
}