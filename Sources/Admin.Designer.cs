namespace UUM_Hytroc
{
    partial class Admin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Admin));
            this.Aide_Boutton = new System.Windows.Forms.Button();
            this.SSO_Champ = new System.Windows.Forms.TextBox();
            this.Mot_De_Passe = new System.Windows.Forms.TextBox();
            this.Se_Connecter_Boutton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Aide_Boutton
            // 
            this.Aide_Boutton.BackColor = System.Drawing.Color.CadetBlue;
            this.Aide_Boutton.ForeColor = System.Drawing.Color.Black;
            this.Aide_Boutton.Location = new System.Drawing.Point(993, 25);
            this.Aide_Boutton.Name = "Aide_Boutton";
            this.Aide_Boutton.Size = new System.Drawing.Size(87, 32);
            this.Aide_Boutton.TabIndex = 10;
            this.Aide_Boutton.Text = "Aide";
            this.Aide_Boutton.UseVisualStyleBackColor = false;
            this.Aide_Boutton.Click += new System.EventHandler(this.Aide_Boutton_Click);
            // 
            // SSO_Champ
            // 
            this.SSO_Champ.Location = new System.Drawing.Point(515, 199);
            this.SSO_Champ.Name = "SSO_Champ";
            this.SSO_Champ.Size = new System.Drawing.Size(327, 22);
            this.SSO_Champ.TabIndex = 11;
            // 
            // Mot_De_Passe
            // 
            this.Mot_De_Passe.Location = new System.Drawing.Point(515, 252);
            this.Mot_De_Passe.Name = "Mot_De_Passe";
            this.Mot_De_Passe.Size = new System.Drawing.Size(327, 22);
            this.Mot_De_Passe.TabIndex = 12;
            // 
            // Se_Connecter_Boutton
            // 
            this.Se_Connecter_Boutton.BackColor = System.Drawing.Color.Crimson;
            this.Se_Connecter_Boutton.ForeColor = System.Drawing.Color.Black;
            this.Se_Connecter_Boutton.Location = new System.Drawing.Point(611, 302);
            this.Se_Connecter_Boutton.Name = "Se_Connecter_Boutton";
            this.Se_Connecter_Boutton.Size = new System.Drawing.Size(130, 39);
            this.Se_Connecter_Boutton.TabIndex = 13;
            this.Se_Connecter_Boutton.Text = "Se connecter";
            this.Se_Connecter_Boutton.UseVisualStyleBackColor = false;
            this.Se_Connecter_Boutton.Click += new System.EventHandler(this.Se_Connecter_Boutton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.CadetBlue;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.label1.Location = new System.Drawing.Point(607, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 25);
            this.label1.TabIndex = 14;
            this.label1.Text = "Identification";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.CadetBlue;
            this.label2.Location = new System.Drawing.Point(521, 180);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 16);
            this.label2.TabIndex = 15;
            this.label2.Text = "SSO";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.CadetBlue;
            this.label3.Location = new System.Drawing.Point(521, 233);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 16);
            this.label3.TabIndex = 16;
            this.label3.Text = "Mot de passe";
            // 
            // Admin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1092, 617);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Se_Connecter_Boutton);
            this.Controls.Add(this.Mot_De_Passe);
            this.Controls.Add(this.SSO_Champ);
            this.Controls.Add(this.Aide_Boutton);
            this.DoubleBuffered = true;
            this.Name = "Admin";
            this.Text = "Admin";
            this.Load += new System.EventHandler(this.Admin_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Aide_Boutton;
        private System.Windows.Forms.TextBox SSO_Champ;
        private System.Windows.Forms.TextBox Mot_De_Passe;
        private System.Windows.Forms.Button Se_Connecter_Boutton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}