using System;
using System.Windows.Forms;
using System.DirectoryServices;
using Oracle.ManagedDataAccess.Client;

namespace UUM_Hytroc
{
    public partial class Admin : Form
    {
        public Admin()
        {
            InitializeComponent();
        }

        // Authentification via Active Directory (LDAP)
        // NOTE recruteur : permet de valider l'identité utilisateur côté entreprise
        private bool Authenticate(string userName, string password, string domain)
        {
            bool authentic = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain, userName, password);
                object nativeObject = entry.NativeObject;
                authentic = true;
            }
            catch (DirectoryServicesCOMException)
            {
                // volontairement vide : retourne false
            }
            return authentic;
        }

        private void Se_Connecter_Boutton_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Vérification des champs AVANT tout
                if (String.IsNullOrEmpty(SSO_Champ.Text) || String.IsNullOrEmpty(Mot_De_Passe.Text))
                {
                    Mot_De_Passe.Clear();
                    MessageBox.Show("Entrez votre SSO et votre mot de passe");
                    return;
                }

                Connect_db ot = new Connect_db();

                // 2. Vérifier si l'utilisateur existe en base (autorisation applicative)
                int count;
                ot.Connect();
                count = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_ADMINISTRATION WHERE SSO_ADMIN = :sso",
                    new OracleParameter(":sso", SSO_Champ.Text)
                );
                ot.Close();

                // 3. Vérifier authentification Active Directory
                bool userValid = Authenticate(SSO_Champ.Text, Mot_De_Passe.Text, "logon.ds.ge.com");

                // 4. Logique métier (inchangée)
                if (userValid && count != 0)
                {
                    Hide();
                    var form3 = new Hytorc_Admin();
                    form3.ShowDialog();
                    Show();
                }
                else if (userValid && count == 0)
                {
                    Mot_De_Passe.Clear();
                    MessageBox.Show("vous avez pas les droits d'accès.");
                }
                else
                {
                    Mot_De_Passe.Clear();
                    MessageBox.Show("SSO ou mot de passe incorrect.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message + "\nStack Trace : " + ex.StackTrace);
            }
        }

        private void Admin_Load(object sender, EventArgs e)
        {
            // Masquer le mot de passe
            Mot_De_Passe.UseSystemPasswordChar = true;
        }

        private void Aide_Boutton_Click(object sender, EventArgs e)
        {
            Hide();
            var form4 = new Hytorc_Telecharger();
            form4.ShowDialog();
            Show();
        }

        private void label1_Click(object sender, EventArgs e) { }

        private void SSO_Champ_TextChanged(object sender, EventArgs e) { }
    }
}
/*namespace UUM_Hytroc
{
    public partial class Admin : Form
    {
        public Admin()
        {
            InitializeComponent();
        }
        private bool Authenticate(string userName, string password, string domain) //We use the active directory to check if user information is correct
        {
            bool authentic = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain,userName, password);
                object nativeObject = entry.NativeObject;
                authentic = true;
            }
            catch (DirectoryServicesCOMException) { }
            return authentic;
        }

        private void Se_Connecter_Boutton_Click(object sender, EventArgs e)
        {
            try
            {
                string sqlqry = "SELECT COUNT(*) AS NOMBRE FROM TBSCT.EPE_UUM_HYTORC_ADMINISTRATION WHERE SSO_ADMIN = '" + SSO_Champ.Text + "'"; //good
                Connect_db ot = new Connect_db();
                ot.Connect();
                OracleDataReader dr = ot.SelectData(sqlqry);
                int count = 0;
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        count = Convert.ToInt32(dr["NOMBRE"]);
                    }
                }
                ot.Close();
                bool userValid = Authenticate(SSO_Champ.Text, Mot_De_Passe.Text, "logon.ds.ge.com");
                if (userValid && count != 0)
                {

                    Hide(); // Cache la fenêtre actuelle
                    var form3 = new Hytorc_Admin(); // Crée une nouvelle instance de Form4
                    form3.ShowDialog(); // Affiche la nouvelle Form4 de manière modale
                    Show(); // Affiche à                             
                }
                else if (userValid && count == 0)
                {
                    Mot_De_Passe.Clear();
                    MessageBox.Show("vous avez pas les drois d'accés.");
                }
                else if (String.IsNullOrEmpty(SSO_Champ.Text) || String.IsNullOrEmpty(Mot_De_Passe.Text))
                {
                    Mot_De_Passe.Clear();
                    MessageBox.Show("Entrez votre SSO et votre mot de passe");
                }
                else
                {
                    Mot_De_Passe.Clear();
                    MessageBox.Show("SSO ou mot de passe incorrect.");
                }
                 
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Erreur : " + ex.Message + "\nStack Trace : " + ex.StackTrace);
            }

        }
        
        private void Admin_Load(object sender, EventArgs e)
        {
            Mot_De_Passe.UseSystemPasswordChar = true;
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Aide_Boutton_Click(object sender, EventArgs e)
        {
            Hide(); // Cache la fenêtre actuelle
            var form4 = new Hytorc_Telecharger(); // Crée une nouvelle instance de Form4
            form4.ShowDialog(); // Affiche la nouvelle Form4 de manière modale
            Show(); // Affiche à 
        }

        private void SSO_Champ_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
*/