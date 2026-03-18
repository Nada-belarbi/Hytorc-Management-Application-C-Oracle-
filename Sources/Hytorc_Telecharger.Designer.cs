namespace UUM_Hytroc
{
    partial class Hytorc_Telecharger
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Hytorc_Telecharger));
            this.Telecharger_fichier_consulter = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Telecharger_fichier_consulter
            // 
            this.Telecharger_fichier_consulter.Location = new System.Drawing.Point(654, 208);
            this.Telecharger_fichier_consulter.Name = "Telecharger_fichier_consulter";
            this.Telecharger_fichier_consulter.Size = new System.Drawing.Size(143, 28);
            this.Telecharger_fichier_consulter.TabIndex = 14;
            this.Telecharger_fichier_consulter.Text = "Télécharger";
            this.Telecharger_fichier_consulter.UseVisualStyleBackColor = true;
            this.Telecharger_fichier_consulter.Click += new System.EventHandler(this.Telecharger_fichier_consulter_Click);
            // 
            // Hytorc_Telecharger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1072, 504);
            this.Controls.Add(this.Telecharger_fichier_consulter);
            this.DoubleBuffered = true;
            this.Name = "Hytorc_Telecharger";
            this.Text = "Form4";
            this.Load += new System.EventHandler(this.Hytorc_Telecharger_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Telecharger_fichier_consulter;
    }
}