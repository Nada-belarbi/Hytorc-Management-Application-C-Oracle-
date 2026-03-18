using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UUM_Hytroc
{
    public partial class Hytorc_Telecharger : Form
    {
        public Hytorc_Telecharger()
        {
            InitializeComponent();
        }

        private void Telecharger_fichier_consulter_Click(object sender, EventArgs e)
        {
            string cheminFichierPDF = @"C:\Users\223114186\Desktop\Projet Hytorc version final\Code\UUM_Hytroc_nada\Instructions technique 2.pdf";

            // Vérifier si le fichier existe
            if (System.IO.File.Exists(cheminFichierPDF))
            {
                try
                {
                    // Ouvrir le fichier PDF à l'aide de l'application par défaut associée aux fichiers PDF
                    Process.Start(cheminFichierPDF);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur s'est produite lors de l'ouverture du fichier PDF : " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Le fichier d'aide n'existe pas à l'emplacement spécifié.");
            }
        }

        private void Hytorc_Telecharger_Load(object sender, EventArgs e)
        {

        }
    }
}
