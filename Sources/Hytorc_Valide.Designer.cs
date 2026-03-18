using System;
using System.Windows;

namespace UUM_Hytroc
{
    partial class Hytorc_Valide
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Hytorc_Valide));
            this.Liste_outil = new System.Windows.Forms.ComboBox();
            this.Valider = new System.Windows.Forms.Button();
            this.Couple = new System.Windows.Forms.TextBox();
            this.Pression = new System.Windows.Forms.TextBox();
            this.List_outil_2 = new System.Windows.Forms.ComboBox();
            this.Champ_Type = new System.Windows.Forms.TextBox();
            this.Admin = new System.Windows.Forms.Button();
            this.Type = new System.Windows.Forms.Label();
            this.Numero = new System.Windows.Forms.Label();
            this.Cpl = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.date_control = new System.Windows.Forms.TextBox();
            this.champ_nombre_utilisation = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.Aide_Boutton = new System.Windows.Forms.Button();
            this.Indication = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Liste_outil
            // 
            this.Liste_outil.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Liste_outil.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Liste_outil.FormattingEnabled = true;
            this.Liste_outil.Location = new System.Drawing.Point(633, 305);
            this.Liste_outil.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Liste_outil.Name = "Liste_outil";
            this.Liste_outil.Size = new System.Drawing.Size(529, 24);
            this.Liste_outil.TabIndex = 0;
            this.Liste_outil.TabStop = false;
            this.Liste_outil.SelectedIndexChanged += new System.EventHandler(this.Liste_outil_SelectedIndexChanged);
            // 
            // Valider
            // 
            this.Valider.BackColor = System.Drawing.Color.CadetBlue;
            this.Valider.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Valider.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Valider.Location = new System.Drawing.Point(874, 370);
            this.Valider.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Valider.Name = "Valider";
            this.Valider.Size = new System.Drawing.Size(97, 38);
            this.Valider.TabIndex = 1;
            this.Valider.Text = "Valider";
            this.Valider.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.Valider.UseVisualStyleBackColor = false;
            this.Valider.Click += new System.EventHandler(this.Valider_Click);
            // 
            // Couple
            // 
            this.Couple.Location = new System.Drawing.Point(633, 383);
            this.Couple.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Couple.Name = "Couple";
            this.Couple.Size = new System.Drawing.Size(200, 22);
            this.Couple.TabIndex = 2;
            // 
            // Pression
            // 
            this.Pression.Enabled = false;
            this.Pression.Location = new System.Drawing.Point(997, 383);
            this.Pression.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Pression.Name = "Pression";
            this.Pression.Size = new System.Drawing.Size(165, 22);
            this.Pression.TabIndex = 3;
            // 
            // List_outil_2
            // 
            this.List_outil_2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.List_outil_2.FormattingEnabled = true;
            this.List_outil_2.Location = new System.Drawing.Point(633, 235);
            this.List_outil_2.Name = "List_outil_2";
            this.List_outil_2.Size = new System.Drawing.Size(529, 24);
            this.List_outil_2.TabIndex = 7;
            this.List_outil_2.SelectedIndexChanged += new System.EventHandler(this.List_outil_2_SelectedIndexChanged);
            // 
            // Champ_Type
            // 
            this.Champ_Type.Enabled = false;
            this.Champ_Type.Location = new System.Drawing.Point(633, 478);
            this.Champ_Type.Name = "Champ_Type";
            this.Champ_Type.Size = new System.Drawing.Size(529, 22);
            this.Champ_Type.TabIndex = 20;
            this.Champ_Type.TabStop = false;
            // 
            // Admin
            // 
            this.Admin.BackColor = System.Drawing.Color.CadetBlue;
            this.Admin.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Admin.ForeColor = System.Drawing.Color.Black;
            this.Admin.Location = new System.Drawing.Point(1158, 24);
            this.Admin.Name = "Admin";
            this.Admin.Size = new System.Drawing.Size(126, 44);
            this.Admin.TabIndex = 9;
            this.Admin.Text = "Admin";
            this.Admin.UseVisualStyleBackColor = false;
            this.Admin.Click += new System.EventHandler(this.Admin_Click);
            // 
            // Type
            // 
            this.Type.AutoSize = true;
            this.Type.BackColor = System.Drawing.Color.CadetBlue;
            this.Type.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Type.ForeColor = System.Drawing.Color.Black;
            this.Type.Location = new System.Drawing.Point(628, 196);
            this.Type.Name = "Type";
            this.Type.Size = new System.Drawing.Size(312, 25);
            this.Type.TabIndex = 21;
            this.Type.Text = "Choisissez le type de votre clé.";
            // 
            // Numero
            // 
            this.Numero.AutoSize = true;
            this.Numero.BackColor = System.Drawing.Color.CadetBlue;
            this.Numero.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Numero.ForeColor = System.Drawing.Color.Black;
            this.Numero.Location = new System.Drawing.Point(628, 262);
            this.Numero.Name = "Numero";
            this.Numero.Size = new System.Drawing.Size(343, 25);
            this.Numero.TabIndex = 22;
            this.Numero.Text = "Choisissez le numéro de votre clé.";
            // 
            // Cpl
            // 
            this.Cpl.AutoSize = true;
            this.Cpl.BackColor = System.Drawing.Color.CadetBlue;
            this.Cpl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cpl.ForeColor = System.Drawing.Color.Black;
            this.Cpl.Location = new System.Drawing.Point(628, 340);
            this.Cpl.Name = "Cpl";
            this.Cpl.Size = new System.Drawing.Size(129, 25);
            this.Cpl.TabIndex = 23;
            this.Cpl.Text = "Couple(Nm)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.CadetBlue;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(1019, 340);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 25);
            this.label3.TabIndex = 24;
            this.label3.Text = "Pression(bar)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.CadetBlue;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(628, 432);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(436, 25);
            this.label4.TabIndex = 25;
            this.label4.Text = "Type , numéro fournisseur et numéro de clé.";
            // 
            // date_control
            // 
            this.date_control.Enabled = false;
            this.date_control.Location = new System.Drawing.Point(922, 568);
            this.date_control.Name = "date_control";
            this.date_control.Size = new System.Drawing.Size(240, 22);
            this.date_control.TabIndex = 26;
            this.date_control.TabStop = false;
            // 
            // champ_nombre_utilisation
            // 
            this.champ_nombre_utilisation.Enabled = false;
            this.champ_nombre_utilisation.Location = new System.Drawing.Point(633, 568);
            this.champ_nombre_utilisation.Name = "champ_nombre_utilisation";
            this.champ_nombre_utilisation.Size = new System.Drawing.Size(283, 22);
            this.champ_nombre_utilisation.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.CadetBlue;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(628, 529);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(288, 25);
            this.label5.TabIndex = 28;
            this.label5.Text = "Nombre d\'utilisation de la clé";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.CadetBlue;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(992, 529);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(170, 25);
            this.label6.TabIndex = 29;
            this.label6.Text = "Date de contrôle";
            // 
            // Aide_Boutton
            // 
            this.Aide_Boutton.BackColor = System.Drawing.Color.CadetBlue;
            this.Aide_Boutton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Aide_Boutton.ForeColor = System.Drawing.Color.Black;
            this.Aide_Boutton.Location = new System.Drawing.Point(1158, 684);
            this.Aide_Boutton.Name = "Aide_Boutton";
            this.Aide_Boutton.Size = new System.Drawing.Size(126, 44);
            this.Aide_Boutton.TabIndex = 30;
            this.Aide_Boutton.Text = "Aide";
            this.Aide_Boutton.UseVisualStyleBackColor = false;
            this.Aide_Boutton.Click += new System.EventHandler(this.Aide_Boutton_Click);
            // 
            // Indication
            // 
            this.Indication.AutoSize = true;
            this.Indication.BackColor = System.Drawing.Color.CadetBlue;
            this.Indication.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Indication.ForeColor = System.Drawing.Color.Black;
            this.Indication.Location = new System.Drawing.Point(583, 691);
            this.Indication.Name = "Indication";
            this.Indication.Size = new System.Drawing.Size(546, 25);
            this.Indication.TabIndex = 32;
            this.Indication.Text = "En cas de soucis , contacter Eric Meyer au bâtiment 31 ";
            // 
            // Hytorc_Valide
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.CadetBlue;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1296, 745);
            this.Controls.Add(this.Indication);
            this.Controls.Add(this.Aide_Boutton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.champ_nombre_utilisation);
            this.Controls.Add(this.date_control);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Cpl);
            this.Controls.Add(this.Numero);
            this.Controls.Add(this.Type);
            this.Controls.Add(this.Admin);
            this.Controls.Add(this.Champ_Type);
            this.Controls.Add(this.List_outil_2);
            this.Controls.Add(this.Liste_outil);
            this.Controls.Add(this.Couple);
            this.Controls.Add(this.Pression);
            this.Controls.Add(this.Valider);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.CadetBlue;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Hytorc_Valide";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "UUM_Hytorc";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Liste_outil;
        private System.Windows.Forms.Button Valider;
        
        

        private System.Windows.Forms.TextBox Couple;
        
        private System.Windows.Forms.TextBox Pression;
        private System.Windows.Forms.ComboBox List_outil_2;
        private System.Windows.Forms.TextBox Champ_Type;
        private System.Windows.Forms.Button Admin;
        private System.Windows.Forms.Label Type;
        private System.Windows.Forms.Label Numero;
        private System.Windows.Forms.Label Cpl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox date_control;
        private System.Windows.Forms.TextBox champ_nombre_utilisation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button Aide_Boutton;
        private System.Windows.Forms.Label Indication;
    }
}

