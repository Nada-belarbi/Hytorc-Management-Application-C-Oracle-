using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Windows.Forms;




namespace UUM_Hytroc
{
    public partial class Hytorc_Valide : Form
    {
        public Hytorc_Valide()
        {
            InitializeComponent();

            // Validation de saisie côté UI pour éviter les valeurs invalides dès l'entrée.
            Couple.KeyPress += new KeyPressEventHandler(TextBoxCouple_KeyPress);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Alimenter_List_Type();
            Alimenter_List_Cle();
        }
        // Contrôle de saisie : le couple doit être numérique.
        private void TextBoxCouple_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                MessageBox.Show("Veuillez saisir uniquement des chiffres.");
            }
        }

   

        // Charge les types de clés disponibles dans la liste de filtre.
        private void Alimenter_List_Type()
        {
            Connect_db ot = new Connect_db();

            try
            {
                ot.Connect();

                string sqlqry = "SELECT DISTINCT TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE";
                OracleDataReader dr = ot.SelectData(sqlqry);

                if (dr != null && dr.HasRows)
                {
                    List_outil_2.Items.Clear();
                    List_outil_2.Items.Add("Tous");

                    while (dr.Read())
                    {
                        List_outil_2.Items.Add(dr["TYPE"].ToString());
                    }
                }

                ot.Close();
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show(ex.Message);
            }
        }

        // Charge la liste des clés actives.
        private void Alimenter_List_Cle()
        {
            Connect_db ot = new Connect_db();

            try
            {
                ot.Connect();

                string sqlqry = "SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = 'ACTIVE' ORDER BY NUMERO ASC";
                OracleDataReader dr = ot.SelectData(sqlqry);

                if (dr != null && dr.HasRows)
                {
                    Liste_outil.Items.Clear();

                    while (dr.Read())
                    {
                        Liste_outil.Items.Add(dr["NUMERO"].ToString());
                    }
                }

                ot.Close();
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show(ex.Message);
            }
        }

     

        // Valide un couple pour une clé :
        // - si la valeur exacte existe, on renvoie la pression stockée,
        // - sinon on calcule une interpolation linéaire entre deux points.
        private void Valider_Click(object sender, EventArgs e)
        {
            Pression.Text = string.Empty;

            if (String.IsNullOrEmpty(Liste_outil.Text) || String.IsNullOrEmpty(Couple.Text))
            {
                MessageBox.Show("selectioner une clé et un couple");
                return;
            }

            try
            {
                List<float> f_couple = new List<float>();
                List<float> f_pression = new List<float>();

                Connect_db ot = new Connect_db();

                // 1) Recherche d'une correspondance exacte couple -> pression
                string sqlqry =
                    "SELECT PRESSION " +
                    "FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                    "INNER JOIN TBSCT.EPE_UUM_HYTORC_CLE " +
                    "ON EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = EPE_UUM_HYTORC_CLE.ID_CLE " +
                    "WHERE EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = (" +
                        "SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :numero" +
                    ") " +
                    "AND EPE_UUM_HYTORC_COUPLES_PRESSION.COUPLE = :couple";

                ot.Connect();
                OracleDataReader dr = ot.Request(
                    sqlqry,
                    new OracleParameter(":numero", Liste_outil.Text),
                    new OracleParameter(":couple", Couple.Text)
                );

                if (dr != null && dr.HasRows)
                {
                    dr.Read();
                    Pression.Text = dr["PRESSION"].ToString();
                    ot.Close();

                    // Incrémentation du nombre d'utilisations
                    sqlqry = "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET NOMBRE_UTILISATION = NOMBRE_UTILISATION + 1 WHERE NUMERO = :numero";
                    ot.Connect();
                    ot.ExecuteNonQuery(sqlqry, new OracleParameter(":numero", Liste_outil.Text));
                    ot.Close();

                    // Recharge le compteur affiché
                    sqlqry = "SELECT NOMBRE_UTILISATION FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :numero";
                    ot.Connect();
                    dr = ot.Request(sqlqry, new OracleParameter(":numero", Liste_outil.Text));

                    if (dr != null && dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            champ_nombre_utilisation.Text = dr["NOMBRE_UTILISATION"].ToString();
                        }
                    }

                    ot.Close();
                }
                else
                {
                    // 2) Si le couple exact n'existe pas, on récupère la courbe pour interpolation
                    ot.Close();

                    sqlqry =
                        "SELECT PRESSION, COUPLE " +
                        "FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                        "INNER JOIN TBSCT.EPE_UUM_HYTORC_CLE " +
                        "ON EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = EPE_UUM_HYTORC_CLE.ID_CLE " +
                        "WHERE EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = (" +
                            "SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :numero" +
                        ") " +
                        "ORDER BY COUPLE";

                    ot.Connect();
                    dr = ot.Request(sqlqry, new OracleParameter(":numero", Liste_outil.Text));

                    float CoupleMaxi = 0;
                    float CoupleMini = 0;

                    while (dr.Read())
                    {
                        f_couple.Add(Convert.ToSingle(dr["COUPLE"]));

                        if (CoupleMini == 0)
                        {
                            CoupleMini = Convert.ToSingle(dr["COUPLE"]);
                        }

                        CoupleMaxi = Convert.ToSingle(dr["COUPLE"]);
                        f_pression.Add(Convert.ToSingle(dr["PRESSION"]));
                    }

                    ot.Close();

                    float UserInput = float.Parse(Couple.Text);

                    if (UserInput < CoupleMini)
                    {
                        MessageBox.Show("Votre couple est inférieur à la valeur du couple minimal, mettez une valeur plus grande.");
                    }
                    else if (UserInput > CoupleMaxi)
                    {
                        MessageBox.Show("Votre couple est supérieur à la valeur du couple maximal,mettez une valeur plus petite.");
                    }
                    else if (f_couple.Count < 2)
                    {
                        MessageBox.Show("Conversion impossible");
                    }
                    else
                    {
                        for (int n = 0; n < f_couple.Count - 1; n++)
                        {
                            if (double.Parse(f_couple[n].ToString()) < double.Parse(Couple.Text) &&
                                double.Parse(Couple.Text) < double.Parse(f_couple[n + 1].ToString()))
                            {
                                float f_a = (f_pression[n + 1] - f_pression[n]) / (f_couple[n + 1] - f_couple[n]);
                                float f_b = f_pression[n + 1] - f_a * f_couple[n + 1];
                                double valeur_pression = f_a * float.Parse(Couple.Text) + f_b;
                                valeur_pression = Math.Round(valeur_pression, 0);

                                Pression.Text = valeur_pression.ToString();

                                // Incrémentation du compteur d'utilisation
                                sqlqry = "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET NOMBRE_UTILISATION = NOMBRE_UTILISATION + 1 WHERE NUMERO = :numero";
                                ot.Connect();
                                ot.ExecuteNonQuery(sqlqry, new OracleParameter(":numero", Liste_outil.Text));
                                ot.Close();

                                // Recharge le compteur affiché
                                sqlqry = "SELECT NOMBRE_UTILISATION FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :numero";
                                ot.Connect();
                                dr = ot.Request(sqlqry, new OracleParameter(":numero", Liste_outil.Text));

                                if (dr != null && dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        champ_nombre_utilisation.Text = dr["NOMBRE_UTILISATION"].ToString();
                                    }
                                }

                                ot.Close();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la validation : " + ex.Message);
            }
        }

        // Affiche les informations détaillées de la clé sélectionnée.
        private void Liste_outil_SelectedIndexChanged(object sender, EventArgs e)
        {
            Connect_db ot = new Connect_db();

            try
            {
                string sqlqry =
                    "SELECT DISTINCT TYPE, NUMERO_FOURNISSEUR, NUMERO " +
                    "FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE " +
                    "INNER JOIN TBSCT.EPE_UUM_HYTORC_CLE " +
                    "ON EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE = EPE_UUM_HYTORC_CLE.TYPE_ID " +
                    "WHERE NUMERO = :numero";

                ot.Connect();
                OracleDataReader dr = ot.Request(sqlqry, new OracleParameter(":numero", Liste_outil.Text));

                if (dr != null && dr.HasRows)
                {
                    while (dr.Read())
                    {
                        Champ_Type.Text =
                            "Le type:" + dr["TYPE"].ToString() +
                            ", " + "Le numero fournisseur: " + dr["NUMERO_FOURNISSEUR"].ToString() +
                            ", " + "Le numero de la clé:" + dr["NUMERO"].ToString();
                    }
                }

                ot.Close();

                sqlqry = "SELECT DATE_DE_CONTROL FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :numero";
                ot.Connect();
                dr = ot.Request(sqlqry, new OracleParameter(":numero", Liste_outil.Text));

                if (dr != null && dr.HasRows)
                {
                    while (dr.Read())
                    {
                        date_control.Text = dr["DATE_DE_CONTROL"].ToString();
                    }
                }

                ot.Close();
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show("Erreur lors du chargement des détails : " + ex.Message);
            }
        }

        // Filtre les clés actives selon le type sélectionné.
        private void List_outil_2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedType = List_outil_2.SelectedItem.ToString();
            string sqlqry;

            Connect_db ot = new Connect_db();

            try
            {
                ot.Connect();
                Liste_outil.Items.Clear();

                OracleDataReader dr;

                if (selectedType == "Tous")
                {
                    sqlqry = "SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = 'ACTIVE' ORDER BY NUMERO ASC";
                    dr = ot.SelectData(sqlqry);
                }
                else
                {
                    sqlqry =
                        "SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE " +
                        "INNER JOIN TBSCT.EPE_UUM_HYTORC_TYPE_CLE " +
                        "ON EPE_UUM_HYTORC_CLE.TYPE_ID = EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE " +
                        "WHERE EPE_UUM_HYTORC_TYPE_CLE.TYPE = :type AND ETAT = 'ACTIVE' " +
                        "ORDER BY NUMERO ASC";

                    dr = ot.Request(sqlqry, new OracleParameter(":type", List_outil_2.Text));
                }

                if (dr != null && dr.HasRows)
                {
                    while (dr.Read())
                    {
                        Liste_outil.Items.Add(dr["NUMERO"].ToString());
                    }
                }

                ot.Close();
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show(ex.Message);
            }
        }

     

        private void Admin_Click(object sender, EventArgs e)
        {
            Hide();
            var Admin = new Admin();
            Admin.ShowDialog();
            Show();
        }


        private void Aide_Boutton_Click(object sender, EventArgs e)
        {
            Hide();
            var form4 = new Hytorc_Telecharger();
            form4.ShowDialog();
            Show();
        }

    }
}

/*namespace UUM_Hytroc
{
    public partial class Hytorc_Valide : Form
    {
        private void TextBoxCouple_KeyPress(object sender, KeyPressEventArgs e) //méthode pour vérifier le type des caractéres dans les champs textes.
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Empêche la saisie du caractère non autorisé
                MessageBox.Show("Veuillez saisir uniquement des chiffres."); // Affiche un message d'erreur
            }
        }
        
        public Hytorc_Valide()
        {
            InitializeComponent();
            Couple.KeyPress += new KeyPressEventHandler(TextBoxCouple_KeyPress);//appel de la methode TexBoxCouple_KeyPress pour verifier les types de caractére dans le champs couple.
            
        }
        private void Alimenter_List_Type()
        {

            Connect_db ot = new Connect_db();

            try
            {
                ot.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            string sqlqry = "select distinct TYPE from TBSCT.EPE_UUM_HYTORC_TYPE_CLE";

            OracleDataReader dr = ot.SelectData(sqlqry);

            if (dr.HasRows)
            {
                List_outil_2.Items.Clear(); // Clear existing items
                List_outil_2.Items.Add("Tous"); // Add "Tous" option

                while (dr.Read())
                {
                    List_outil_2.Items.Add(dr["TYPE"].ToString());
                }
            }
            ot.Close();
        }
        private void Alimenter_List_Cle()
        {
            Connect_db ot = new Connect_db();

            try
            {
                ot.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            string sqlqry = "select NUMERO from TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = 'ACTIVE' order by NUMERO ASC ";

            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);
            dr = ot.SelectData(sqlqry);

            if (dr.HasRows)
            {

                while (dr.Read())
                {

                    Liste_outil.Items.Add(dr["NUMERO"].ToString());
                }
            }
            ot.Close();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Alimenter_List_Type();

            Alimenter_List_Cle();
        }
        

        private void Valider_Click(object sender, EventArgs e)
        {
            
            Pression.Text = string.Empty;
            if (String.IsNullOrEmpty(Liste_outil.Text) || String.IsNullOrEmpty(Couple.Text) ) 
            {
                MessageBox.Show("selectioner une clé et un couple");
            }
            else
            {
                List<float> f_couple = new List<float>();
                List<float> f_pression = new List<float>();

                string sqlqry = "select PRESSION " +
                                "From TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                                "inner join TBSCT.EPE_UUM_HYTORC_CLE " +
                                "on EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = EPE_UUM_HYTORC_CLE.ID_CLE " +
                                "where EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = (SELECT ID_CLE " +
                                                                                "FROM TBSCT.EPE_UUM_HYTORC_CLE " +
                                                                                "WHERE NUMERO = '" + Liste_outil.Text + "')" +
                                "and EPE_UUM_HYTORC_COUPLES_PRESSION.COUPLE ='" + Couple.Text + "'";
                Connect_db ot = new Connect_db();
                ot.Connect();
                OracleDataReader dr = ot.SelectData(sqlqry);                                          
                if (dr.HasRows)
                {
                    dr.Read();
                    Pression.Text = dr["Pression"].ToString();
                    ot.Close();
                    sqlqry = "Update tbsct.EPE_UUM_HYTORC_CLE SET NOMBRE_UTILISATION = NOMBRE_UTILISATION+1 where NUMERO ='" + Liste_outil.Text + "'";
                    ot.Connect();
                    ot.SelectData(sqlqry);
                    ot.Close();
                    ot.Connect();
                    sqlqry = "Select NOMBRE_UTILISATION From tbsct.EPE_UUM_HYTORC_CLE where NUMERO ='" + Liste_outil.Text + "'";

                    dr = ot.SelectData(sqlqry);
                    if (dr.HasRows)
                    {

                        while (dr.Read())
                        {

                            champ_nombre_utilisation.Text = dr["NOMBRE_UTILISATION"].ToString();
                        }



                    }

                    ot.Close();
                }
                else
                {
                    sqlqry = "select PRESSION ,COUPLE" +
                             " from TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION" +
                             " inner join TBSCT.EPE_UUM_HYTORC_CLE " +
                             "on EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = EPE_UUM_HYTORC_CLE.ID_CLE " +
                             "where EPE_UUM_HYTORC_COUPLES_PRESSION.CLE_ID = (SELECT ID_CLE " +
                                                                 "FROM TBSCT.EPE_UUM_HYTORC_CLE " +
                                                                 "WHERE NUMERO = '" + Liste_outil.Text + "')" +
                              "order by COUPLE";
                    ot.Connect();
                    dr = ot.SelectData(sqlqry);
                    float CoupleMaxi=0;
                    float CoupleMini = 0;
                    while (dr.Read())
                    {
                        f_couple.Add(Convert.ToSingle(dr["COUPLE"]));
                        if (CoupleMini == 0)
                        {
                            CoupleMini = Convert.ToSingle(dr["COUPLE"]);  
                        }
                        CoupleMaxi = Convert.ToSingle(dr["COUPLE"]);
                        f_pression.Add(Convert.ToSingle(dr["PRESSION"]));  
                    }
                    ot.Close();
                    float UserInput = float.Parse(Couple.Text);
                    if (UserInput < CoupleMini )
                    {
                        MessageBox.Show("Votre couple est inférieur à la valeur du couple minimal, mettez une valeur plus grande.");
                    }else if (UserInput > CoupleMaxi)
                    {
                        MessageBox.Show("Votre couple est supérieur à la valeur du couple maximal,mettez une valeur plus petite.");
                    }
                    else if (f_couple.Count() < 2)
                    {
                        MessageBox.Show("Conversion impossible");
                    }

                    else
                    {
                        
                        for (int n = 0; n < f_couple.Count - 1; n++)
                        {
                            if (double.Parse(f_couple[n].ToString()) < double.Parse((Couple.Text)) && double.Parse((Couple.Text)) < double.Parse(f_couple[n + 1].ToString()))
                            {
                                float f_a = (f_pression[n + 1] - f_pression[n]) / (f_couple[n + 1] - f_couple[n]);
                                float f_b = f_pression[n + 1] - f_a * f_couple[n + 1];
                                double valeur_pression = f_a * float.Parse(Couple.Text) + f_b;
                                valeur_pression = Math.Round(valeur_pression, 0);
                                Pression.Text = valeur_pression.ToString();
                                sqlqry = "Update tbsct.EPE_UUM_HYTORC_CLE SET NOMBRE_UTILISATION = NOMBRE_UTILISATION+1 where NUMERO ='" + Liste_outil.Text + "'";
                                ot.Connect();
                                ot.SelectData(sqlqry);
                                ot.Close();
                                sqlqry = "Select NOMBRE_UTILISATION From tbsct.EPE_UUM_HYTORC_CLE where NUMERO ='" + Liste_outil.Text + "'";
                                ot.Connect();
                                dr = ot.SelectData(sqlqry);
                                if (dr.HasRows)
                                {

                                    while (dr.Read())
                                    {

                                        champ_nombre_utilisation.Text = dr["NOMBRE_UTILISATION"].ToString();
                                    }



                                }

                                ot.Close();
                                break;
                            }

                        }
                        
                    }
                    
                }
                
            }

        }

        
        
       
        private void Liste_outil_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            string sqlqry = "select distinct TYPE,NUMERO_FOURNISSEUR,NUMERO from TBSCT.EPE_UUM_HYTORC_TYPE_CLE" +
                            " inner join TBSCT.EPE_UUM_HYTORC_CLE " +
                            "on EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE = EPE_UUM_HYTORC_CLE.TYPE_ID " +
                            " where NUMERO = '" + Liste_outil.Text + "'";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);

            if (dr.HasRows)
            {

                while (dr.Read())
                {
                    Champ_Type.Text = "Le type:" + dr["TYPE"].ToString() + ", " + "Le numero fournisseur: "+dr["NUMERO_FOURNISSEUR"].ToString() + ", " + "Le numero de la clé:" + dr["NUMERO"].ToString();
                    
                }
            }

            ot.Close();
            sqlqry = "select DATE_DE_CONTROL from TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO ='"+ Liste_outil.Text + "'";
            ot.Connect();
            dr = ot.SelectData(sqlqry);

            if (dr.HasRows)
            {

                while (dr.Read())
                {
                    date_control.Text = dr["DATE_DE_CONTROL"].ToString();


                }
            }
            ot.Close();
            
        }
        private void List_outil_2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedType = List_outil_2.SelectedItem.ToString();
            string sqlqry;

            if (selectedType == "Tous")
            {
                sqlqry = "SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = 'ACTIVE' ORDER BY NUMERO ASC";
            }
            else
            {
                sqlqry = "SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE " +
                         "INNER JOIN TBSCT.EPE_UUM_HYTORC_TYPE_CLE " +
                         "ON EPE_UUM_HYTORC_CLE.TYPE_ID = EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE " +
                         "WHERE EPE_UUM_HYTORC_TYPE_CLE.TYPE = '" + List_outil_2.Text + "' AND ETAT = 'ACTIVE' " +
                         "ORDER BY NUMERO ASC";
            }

            Connect_db ot = new Connect_db();

            try
            {
                ot.Connect();

                OracleDataReader dr = ot.SelectData(sqlqry);
                Liste_outil.Items.Clear();

                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        Liste_outil.Items.Add(dr["NUMERO"].ToString());
                    }
                }

                ot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void Admin_Click(object sender, EventArgs e)
        {
           
            Hide(); // Cache la fenêtre actuelle
            var Admin = new Admin(); // Crée une nouvelle instance de Form4
            Admin.ShowDialog(); // Affiche la nouvelle Form4 de manière modale
            Show(); // Affiche à 
        }

        private void Aide_Boutton_Click(object sender, EventArgs e)
        {

            Hide(); // Cache la fenêtre actuelle
            var form4 = new Hytorc_Telecharger(); // Crée une nouvelle instance de Form4
            form4.ShowDialog(); // Affiche la nouvelle Form4 de manière modale
            Show(); // Affiche à 
        }
    }
}
*/