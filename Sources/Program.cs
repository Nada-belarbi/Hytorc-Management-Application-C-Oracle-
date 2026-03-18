using System;
using System.Windows.Forms;

namespace UUM_Hytroc
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Chargement de la configuration DB (chemin fourni via variable d'environnement)
                var ot = new Connect_db();
                string dbRoot = Environment.GetEnvironmentVariable("DB_ROOT");

                if (string.IsNullOrWhiteSpace(dbRoot))
                {
                    MessageBox.Show(
                        "Variable d'environnement DB_ROOT introuvable.\n" +
                        "Veuillez la définir avant de lancer l'application.",
                        "Configuration manquante",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                ot.setDB_root(dbRoot);
                ot.readDBfile();

                // Test de connexion au démarrage : permet d'échouer tôt si la config est invalide
                ot.Connect();
                ot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur d'initialisation (configuration/connexion DB) :\n" + ex.Message,
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Lancement de l'IHM
            Application.Run(new Hytorc_Valide());
        }
    }
}