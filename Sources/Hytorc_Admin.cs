using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using ClosedXML.Excel;
using DateTime = System.DateTime;

namespace UUM_Hytroc
{
    public partial class Hytorc_Admin : Form
    {
        // Constantes métier / parsing
        private const string FileNameSeparator = "-°-";
        private const string EtatActive = "ACTIVE";
        private const string EtatDesactive = "DESACTIVE";

        // Etat interne de la fenêtre
        private bool erreurDetectee;

        // Dossier d'archive configuré via App.config
        private readonly string archiveFolder;

        // Suivi du traitement des fichiers
        private readonly List<string> filesWithErrors = new List<string>();
        private readonly List<string> filesToRemove = new List<string>();

        public Hytorc_Admin()
        {
            InitializeComponent();

            // Lecture du chemin depuis App.config
            archiveFolder = ConfigurationManager.AppSettings["ArchiveFolder"];
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Alimenter_Num_GE();
            Afficher_SSO();
            Alimenter_list_active();
            Alimenter_list_desactive();
            Alimenter_list_type();
        }

        // Récupère le SSO Windows de l'utilisateur connecté.
        private string GetLoggedInSSO()
        {
            string sso = WindowsIdentity.GetCurrent().Name;
            string username = sso.Split('\\')[1];
            return username;
        }

        // Affiche le SSO et le nom de l'utilisateur.
        private void Afficher_SSO()
        {
            Connect_db ot = new Connect_db();
            string sso = GetLoggedInSSO();

            string sqlqry = "SELECT NOM FROM TBSCT.EPE_UUM_HYTORC_ADMINISTRATION WHERE SSO_ADMIN = :sso";

            ot.Connect();
            OracleDataReader dr = ot.Request(
                sqlqry,
                new OracleParameter(":sso", sso)
            );

            string nom = null;
            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    nom = dr["NOM"]?.ToString();
                }
            }

            ot.Close();
            Utilisateur.Text = "Votre SSO: " + sso + '\n' + nom;
        }

        // Alimente la liste des clés désactivées.
        private void Alimenter_list_desactive()
        {
            string sqlqry = $"SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = '{EtatDesactive}' ORDER BY NUMERO ASC";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);

            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    List_DESACTIVE.Items.Add(dr["NUMERO"].ToString());
                }
            }

            ot.Close();
        }

        // Alimente la liste des types de clés.
        private void Alimenter_list_type()
        {
            string sqlqry = "SELECT TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE ORDER BY TYPE ASC";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);

            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    list_type.Items.Add(dr["TYPE"].ToString());
                }
            }

            ot.Close();
        }

        // Alimente la liste des clés actives.
        private void Alimenter_list_active()
        {
            string sqlqry = $"SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = '{EtatActive}' ORDER BY NUMERO ASC";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);

            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    LIST_ACTIVE.Items.Add(dr["NUMERO"].ToString());
                }
            }

            ot.Close();
        }

        // Sélectionne un ou plusieurs fichiers à traiter.
        private void Telecharger_fichier_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Sélectionner un ou plusieurs fichiers texte";
                openFileDialog.Filter = "Fichiers texte (*.txt;*.xys)|*.txt;*.xys|Tous les fichiers (*.*)|*.*";
                openFileDialog.Multiselect = true;

                string inputFolder = ConfigurationManager.AppSettings["InputFolder"];
                if (!string.IsNullOrWhiteSpace(inputFolder))
                {
                    openFileDialog.InitialDirectory = inputFolder;
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        Fichier.Items.Add(fileName);
                    }
                }
            }
        }

        // Supprime les couples / pressions existants d'une clé.
        private void DELETEDatabase(string numeroCle)
        {
            Connect_db ot = new Connect_db();

            try
            {
                string deleteQuery =
                    "DELETE FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                    "WHERE CLE_ID = (SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num)";

                ot.Connect();
                ot.ExecuteNonQuery(deleteQuery, new OracleParameter(":num", numeroCle));
                ot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la suppression : " + ex.Message);
            }
        }

        // Réinsère les nouvelles valeurs couple / pression.
        private void INSERTDatabase(string numeroCle, int pression, int couple)
        {
            Connect_db ot = new Connect_db();

            try
            {
                string insertQuery =
                    "INSERT INTO TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                    "(ID_COUPLE_PRESSION, PRESSION, COUPLE, CLE_ID) " +
                    "VALUES (TBSCT.ID_COUPLE_PRESSION.NEXTVAL, :pression, :couple, " +
                    "(SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num))";

                ot.Connect();
                ot.ExecuteNonQuery(
                    insertQuery,
                    new OracleParameter(":pression", pression),
                    new OracleParameter(":couple", couple),
                    new OracleParameter(":num", numeroCle)
                );
                ot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'insertion : " + ex.Message);
            }
        }

        // Met à jour la date de contrôle de la clé.
        private void UPDATEDATE(string DateControl, string numeroCle)
        {
            Connect_db ot = new Connect_db();

            try
            {
                string updateQuery =
                    "UPDATE TBSCT.EPE_UUM_HYTORC_CLE " +
                    "SET DATE_DE_CONTROL = TO_DATE(:dateCtrl, 'DD/MM/YYYY') " +
                    "WHERE NUMERO = :num";

                ot.Connect();
                ot.ExecuteNonQuery(
                    updateQuery,
                    new OracleParameter(":dateCtrl", DateControl),
                    new OracleParameter(":num", numeroCle)
                );
                ot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la mise à jour de la date : " + ex.Message);
            }
        }

        // Nettoie le numéro fournisseur dans le nom de fichier.
        private string CleanSupplierNumber(string input)
        {
            return input.Replace(" ", "").Replace("-", "");
        }

        // Vérifie le format attendu du nom de fichier.
        private bool IsValidFileName(string fileName)
        {
            return fileName.Contains(FileNameSeparator);
        }

        // Traite un fichier et met à jour la base.
        public void ProcessFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] fileNameParts = fileName.Split(new string[] { FileNameSeparator }, StringSplitOptions.None);

                if (!IsValidFileName(fileName))
                {
                    erreurDetectee = true;
                    MessageBox.Show($"Le nom du fichier contient des erreurs : {fileName}. Assurez-vous qu'il respecte le format spécifié.", "Erreur de format de fichier", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (fileNameParts.Length > 2)
                {
                    string numeroCle = fileNameParts[2].Trim();
                    string numeroSerie = CleanSupplierNumber(fileNameParts[1].Trim());
                    string monthYear = fileNameParts[3].Trim();
                    string month = monthYear.Substring(0, 2);
                    string year = monthYear.Substring(2, 4);
                    string dateString = month + "/" + year;

                    DateTime dateControl = DateTime.ParseExact(dateString, "MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime firstDayOfMonth = new DateTime(dateControl.Year, dateControl.Month, 1);
                    string formattedDateControl = firstDayOfMonth.ToString("dd/MM/yyyy");

                    if (lines.Length >= 2)
                    {
                        Connect_db ot = new Connect_db();
                        string sqlqry =
                            "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE " +
                            "WHERE NUMERO = :num AND NUMERO_FOURNISSEUR = :serie";

                        ot.Connect();
                        int count = ot.ExecuteScalarInt(
                            sqlqry,
                            new OracleParameter(":num", numeroCle),
                            new OracleParameter(":serie", numeroSerie)
                        );
                        ot.Close();

                        if (count != 0)
                        {
                            erreurDetectee = false;

                            try
                            {
                                DELETEDatabase(numeroCle);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }

                            for (int i = 0; i < lines.Length; i++)
                            {
                                int PR = int.Parse(lines[i].Split('\t')[0]);
                                int CL = int.Parse(lines[i].Split('\t')[1]);

                                try
                                {
                                    INSERTDatabase(numeroCle, PR, CL);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                            }

                            try
                            {
                                UPDATEDATE(formattedDateControl, numeroCle);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }

                            Modifier_Par(numeroCle);
                        }
                        else
                        {
                            erreurDetectee = true;
                            MessageBox.Show("Le numéro de clé correspondant au fichier n'exhiste pas : " + fileName);
                        }
                    }
                    else
                    {
                        erreurDetectee = true;
                        MessageBox.Show("Le fichier ne contient pas suffisamment de lignes pour être traité correctement: " + fileName);
                    }
                }
                else
                {
                    erreurDetectee = true;
                    MessageBox.Show("Format du nom de fichier incorrect. Le numéro de clé n'a pas pu être extrait: " + fileName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du traitement du fichier '{filePath}': {ex.Message}", ex);
            }
        }

        // Enregistre les fichiers et archive ceux qui ont été traités avec succès.
        private void Enregistrer_fichier_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(archiveFolder))
                {
                    MessageBox.Show("Le dossier d'archive n'est pas configuré dans App.config.");
                    return;
                }

                if (Fichier.SelectedItem != null)
                {
                    filesToRemove.Clear();
                    filesWithErrors.Clear();

                    foreach (string filePath in Fichier.SelectedItems)
                    {
                        try
                        {
                            string fileName = Path.GetFileNameWithoutExtension(filePath);

                            if (!IsValidFileName(fileName))
                            {
                                filesWithErrors.Add(filePath);
                                MessageBox.Show("Nom du fichier contient des erreurs :" + fileName);
                                continue;
                            }

                            ProcessFile(filePath);

                            if (erreurDetectee == false)
                            {
                                filesToRemove.Add(filePath);
                            }
                            else
                            {
                                filesWithErrors.Add(filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                    if (filesToRemove.Count > 0)
                    {
                        foreach (string filePathToRemove in filesToRemove)
                        {
                            string fileName = Path.GetFileName(filePathToRemove);
                            string archivedFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(fileName)}";
                            string destinationPath = Path.Combine(archiveFolder, archivedFileName);
                            File.Move(filePathToRemove, destinationPath);
                        }

                        string succesMessage = "Enregistrement effectué avec succès.";
                        foreach (string filePathToRemove in filesToRemove)
                        {
                            succesMessage += $"{Path.GetFileName(filePathToRemove)}\n";
                            Fichier.Items.Remove(filePathToRemove);
                        }

                        MessageBox.Show(succesMessage);
                    }
                    else if (filesWithErrors.Count > 0)
                    {
                        string errorMessage = "Le traitement des fichiers suivants a rencontré des erreurs:\n";
                        foreach (string errorFile in filesWithErrors)
                        {
                            errorMessage += $"{Path.GetFileName(errorFile)}\n";
                        }
                        MessageBox.Show(errorMessage);
                    }
                }
                else
                {
                    MessageBox.Show("Veuillez sélectionner un fichier dans la liste avant de consulter.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur inattendue s'est produite : {ex.Message}");
            }
        }

        // Ouvre le fichier sélectionné.
        private void Consulter_Bouton_Click(object sender, EventArgs e)
        {
            if (Fichier.SelectedItem != null)
            {
                System.Diagnostics.Process.Start(Fichier.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un fichier dans la liste avant de consulter.");
            }
        }

        // Supprime un fichier de la liste affichée.
        private void Supprimer_Click(object sender, EventArgs e)
        {
            if (Fichier.SelectedItem != null)
            {
                string filePathToDelete = Fichier.SelectedItem.ToString();

                Fichier.Items.Remove(filePathToDelete);

                if (filesWithErrors.Contains(filePathToDelete))
                {
                    filesWithErrors.Remove(filePathToDelete);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un fichier dans la liste avant de supprimer.");
            }
        }

        // Journalise les modifications en base (audit trail).
        private void Modifier_Par(string numeroCle)
        {
            var ot = new Connect_db();
            string sso = GetLoggedInSSO();

            try
            {
                ot.Connect();

                string insert =
                    "INSERT INTO TBSCT.EPE_UUM_HYTORC_MODIFICATION_CLE " +
                    "(ID_MODIFICATION, DATE_DE_MISE_A_JOUR, ID_ADMINISTRATEUR, ID_CLE_MODIFIER) " +
                    "VALUES (TBSCT.ID_MODIFICATION.NEXTVAL, SYSDATE, " +
                    "(SELECT ID_ADMIN FROM TBSCT.EPE_UUM_HYTORC_ADMINISTRATION WHERE SSO_ADMIN = :sso), " +
                    "(SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num))";

                ot.ExecuteNonQuery(
                    insert,
                    new OracleParameter(":sso", sso),
                    new OracleParameter(":num", numeroCle)
                );

                string action =
                    Ajouter.Focused ? "AJOUTER CLE" :
                    Reactiver_bouton.Focused ? "ACTIVER CLE" :
                    Desactiver_cle.Focused ? "DESACTIVER CLE" :
                    Enregister.Focused ? "MISE A JOUR CLE" :
                    "ACTION_INCONNUE";

                string updateAction =
                    "UPDATE TBSCT.EPE_UUM_HYTORC_MODIFICATION_CLE " +
                    "SET ACTION = :action " +
                    "WHERE ID_MODIFICATION = (SELECT MAX(ID_MODIFICATION) FROM TBSCT.EPE_UUM_HYTORC_MODIFICATION_CLE)";

                ot.ExecuteNonQuery(
                    updateAction,
                    new OracleParameter(":action", action)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la journalisation des modifications : " + ex.Message);
            }
            finally
            {
                ot.Close();
            }
        }

        // Ajoute une clé et son type si nécessaire.
        private void Ajouter_Click(object sender, EventArgs e)
        {
            try
            {
                string numeroGE = Numero_ge_champ.Text?.Trim();
                string numeroFournisseur = Numero_fournisseur_champ.Text?.Trim();
                string typeCle = list_type.Text?.Trim();
                string dateCtrlText = Date_control_champ.Text?.Trim();

                if (string.IsNullOrWhiteSpace(numeroGE) ||
                    string.IsNullOrWhiteSpace(numeroFournisseur) ||
                    string.IsNullOrWhiteSpace(typeCle) ||
                    string.IsNullOrWhiteSpace(dateCtrlText))
                {
                    MessageBox.Show("Veuillez renseigner touts les champs.");
                    return;
                }

                if (numeroGE.Contains(" ") || numeroGE.Contains("-"))
                {
                    MessageBox.Show("Le champ 'Numéro GE' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (numeroFournisseur.Contains(" ") || numeroFournisseur.Contains("-"))
                {
                    MessageBox.Show("Le champ 'Numéro Fournisseur' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!DateTime.TryParseExact(dateCtrlText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    MessageBox.Show("Format de date incorrect. Veuillez entrer une date au format DD/MM/YYYY.");
                    Date_control_champ.Text = string.Empty;
                    return;
                }

                var ot = new Connect_db();

                ot.Connect();
                int typeExists = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :type",
                    new OracleParameter(":type", typeCle)
                );
                ot.Close();

                ot.Connect();
                int cleExists = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num",
                    new OracleParameter(":num", numeroGE)
                );
                ot.Close();

                if (cleExists != 0)
                {
                    MessageBox.Show("Votre clé existe déjà");
                    return;
                }

                if (typeExists == 0)
                {
                    var res = MessageBox.Show(
                        $"Ce Type: {typeCle} n'existe pas, est-ce que vous voulez l'ajouter ?",
                        "Confirmation",
                        MessageBoxButtons.YesNo
                    );

                    if (res != DialogResult.Yes)
                    {
                        Date_control_champ.Text = string.Empty;
                        list_type.Text = string.Empty;
                        Numero_ge_champ.Text = string.Empty;
                        Numero_fournisseur_champ.Text = string.Empty;
                        MessageBox.Show("Votre opération est annulée.");
                        return;
                    }

                    ot.Connect();
                    ot.ExecuteNonQuery(
                        "INSERT INTO TBSCT.EPE_UUM_HYTORC_TYPE_CLE(ID_TYPE, TYPE) VALUES (TBSCT.ID_TYPE.NEXTVAL, :type)",
                        new OracleParameter(":type", typeCle)
                    );
                    ot.Close();

                    MessageBox.Show("Nouveau type ajouté !");
                    list_type.Items.Clear();
                    Alimenter_list_type();
                }

                string insertCle =
                    "INSERT INTO TBSCT.EPE_UUM_HYTORC_CLE " +
                    "(ID_CLE, NUMERO_FOURNISSEUR, DATE_DE_CONTROL, TYPE_ID, NUMERO, ETAT) " +
                    "VALUES " +
                    "(TBSCT.ID_CLE.NEXTVAL, :four, TO_DATE(:dateCtrl,'DD/MM/YYYY'), " +
                    "(SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :type), :num, 'ACTIVE')";

                ot.Connect();
                ot.ExecuteNonQuery(
                    insertCle,
                    new OracleParameter(":four", numeroFournisseur),
                    new OracleParameter(":dateCtrl", dateCtrlText),
                    new OracleParameter(":type", typeCle),
                    new OracleParameter(":num", numeroGE)
                );
                ot.Close();

                if (MessageBox.Show("Clé ajoutée avec succès.") == DialogResult.OK)
                {
                    LIST_ACTIVE.Items.Clear();
                    List_DESACTIVE.Items.Clear();
                    Alimenter_list_active();
                    Alimenter_list_desactive();

                    Num_GE_Modif.Items.Clear();
                    Alimenter_Num_GE();

                    Modifier_Par(numeroGE);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message + "\nStack Trace : " + ex.StackTrace);
            }
        }

        // Active une clé.
        private void Reactiver_bouton_Click(object sender, EventArgs e)
        {
            Connect_db ot = new Connect_db();

            if (String.IsNullOrEmpty(List_DESACTIVE.Text))
            {
                MessageBox.Show("Entrez un numéro.");
                return;
            }

            try
            {
                ot.Connect();
                int count = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num",
                    new OracleParameter(":num", List_DESACTIVE.Text)
                );
                ot.Close();

                if (count == 0)
                {
                    List_DESACTIVE.Text = string.Empty;
                    MessageBox.Show("votre cle n'existe pas.");
                    return;
                }

                string sqlqry = "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET ETAT = 'ACTIVE' WHERE NUMERO = :num";
                ot.Connect();
                ot.ExecuteNonQuery(sqlqry, new OracleParameter(":num", List_DESACTIVE.Text));
                ot.Close();

                List_DESACTIVE.Items.Clear();
                LIST_ACTIVE.Items.Clear();
                Alimenter_list_desactive();
                Alimenter_list_active();

                if (MessageBox.Show("Clé activée avec succés.") == DialogResult.OK)
                {
                    Modifier_Par(List_DESACTIVE.Text);
                }

                List_DESACTIVE.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show("Erreur lors de l'activation : " + ex.Message);
            }
        }

        // Désactive une clé.
        private void Desactiver_cle_Click(object sender, EventArgs e)
        {
            Connect_db ot = new Connect_db();

            if (String.IsNullOrEmpty(LIST_ACTIVE.Text))
            {
                MessageBox.Show("Entrez un numéro.");
                return;
            }

            try
            {
                ot.Connect();
                int count = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num",
                    new OracleParameter(":num", LIST_ACTIVE.Text)
                );
                ot.Close();

                if (count == 0)
                {
                    LIST_ACTIVE.Text = string.Empty;
                    MessageBox.Show("votre clé n'existe pas.");
                    return;
                }

                string sqlqry = "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET ETAT = 'DESACTIVE' WHERE NUMERO = :num";
                ot.Connect();
                ot.ExecuteNonQuery(sqlqry, new OracleParameter(":num", LIST_ACTIVE.Text));
                ot.Close();

                LIST_ACTIVE.Items.Clear();
                List_DESACTIVE.Items.Clear();
                Alimenter_list_active();
                Alimenter_list_desactive();

                if (MessageBox.Show("Clé désactivée avec succés.") == DialogResult.OK)
                {
                    Modifier_Par(LIST_ACTIVE.Text);
                }

                LIST_ACTIVE.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show("Erreur lors de la désactivation : " + ex.Message);
            }
        }

        // Génère un fichier Excel récapitulatif.
        private void GenererFichierNumerosCle(string filePath)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Feuille1");

                worksheet.Cell("A1").Value = "Liste des numéros de clé";
                worksheet.Range("A1:E1").Merge();

                string[] headers = { "Numero ge", "Numero fournisseur", "Type de clé", "date du dernier control", "Etat" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(2, i + 1).Value = headers[i];
                }

                string sqlqry =
                    "SELECT NUMERO, NUMERO_FOURNISSEUR, TYPE, DATE_DE_CONTROL, ETAT " +
                    "FROM TBSCT.EPE_UUM_HYTORC_CLE " +
                    "INNER JOIN TBSCT.EPE_UUM_HYTORC_TYPE_CLE ON TBSCT.EPE_UUM_HYTORC_CLE.TYPE_ID = TBSCT.EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE " +
                    "ORDER BY DATE_DE_CONTROL ASC";

                Connect_db ot = new Connect_db();
                ot.Connect();
                OracleDataReader dr = ot.SelectData(sqlqry);

                int row = 3;
                if (dr != null && dr.HasRows)
                {
                    while (dr.Read())
                    {
                        for (int col = 0; col < dr.FieldCount; col++)
                        {
                            worksheet.Cell(row, col + 1).Value = dr[col];
                        }
                        row++;
                    }
                }

                ot.Close();
                workbook.SaveAs(filePath);

                MessageBox.Show("Document Excel généré avec succès à l'emplacement: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur inattendue s'est produite : {ex.Message}");
            }
        }

        // Lance l'export Excel.
        private void Extraire_boutton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Fichiers Excel|*.xlsx";
            saveFileDialog.Title = "Enregistrer le fichier Excel";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                try
                {
                    GenererFichierNumerosCle(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur inattendue s'est produite : {ex.Message}");
                }
            }
        }

        // Affiche les détails d'une clé sélectionnée.
        private void Affichier_Cle()
        {
            string sqlqry =
                "SELECT NUMERO, TYPE, NUMERO_FOURNISSEUR, DATE_DE_CONTROL " +
                "FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE " +
                "INNER JOIN TBSCT.EPE_UUM_HYTORC_CLE " +
                "ON TBSCT.EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE = TBSCT.EPE_UUM_HYTORC_CLE.TYPE_ID " +
                "WHERE NUMERO = :num";

            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.Request(
                sqlqry,
                new OracleParameter(":num", Num_GE_Modif.Text)
            );

            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    Num_Four_Modif.Text = dr["NUMERO_FOURNISSEUR"].ToString();
                    Type_Modif.Text = dr["TYPE"].ToString();
                    Date_Modif.Text = dr["DATE_DE_CONTROL"].ToString();
                    Num_GE_Modif.Text = dr["NUMERO"].ToString();
                }
            }

            ot.Close();
        }

        private void Aide_Boutton_Click(object sender, EventArgs e)
        {
            Hide();
            var form4 = new Hytorc_Telecharger();
            form4.ShowDialog();
            Show();
        }

        // Alimente la liste des numéros GE.
        private void Alimenter_Num_GE()
        {
            string sqlqry = "SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE ORDER BY NUMERO ASC";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);

            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    Num_GE_Modif.Items.Add(dr["NUMERO"].ToString());
                }
            }

            ot.Close();
        }

        private void Num_GE_Modif_SelectedIndexChanged(object sender, EventArgs e)
        {
            Affichier_Cle();
        }

        // Recharge le numéro GE à partir du numéro fournisseur.
        private void Alimenter_champ_cle_actu()
        {
            Connect_db ot = new Connect_db();
            string sqlqry = "SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO_FOURNISSEUR = :four";

            ot.Connect();
            OracleDataReader dr = ot.Request(
                sqlqry,
                new OracleParameter(":four", Num_Four_Modif.Text)
            );

            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    Num_GE_Modif.Text = dr["NUMERO"].ToString();
                }
            }

            ot.Close();
        }

        // Modifie le numéro GE.
        private void Modifier_numéro_ge_Click(object sender, EventArgs e)
        {
            string NVnumeroGE = nouveau_ge.Text;

            if (NVnumeroGE.Contains(" ") || NVnumeroGE.Contains("-"))
            {
                MessageBox.Show("Le champ 'Numéro GE' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Connect_db ot = new Connect_db();

            ot.Connect();
            int count = ot.ExecuteScalarInt(
                "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num",
                new OracleParameter(":num", nouveau_ge.Text)
            );
            ot.Close();

            if (String.IsNullOrEmpty(nouveau_ge.Text))
            {
                MessageBox.Show("Votre champ est vide");
            }
            else if (count == 0)
            {
                string sqlqry =
                    "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET NUMERO = :newNum " +
                    "WHERE NUMERO = :oldNum";

                ot.Connect();
                ot.ExecuteNonQuery(
                    sqlqry,
                    new OracleParameter(":newNum", nouveau_ge.Text),
                    new OracleParameter(":oldNum", Num_GE_Modif.Text)
                );
                ot.Close();

                MessageBox.Show("votre numéro ge est modifié avec succés.");
                nouveau_ge.Text = string.Empty;
                Num_GE_Modif.Text = string.Empty;
                Num_GE_Modif.Items.Clear();
                Alimenter_Num_GE();
                Affichier_Cle();
                Alimenter_champ_cle_actu();
                LIST_ACTIVE.Items.Clear();
                Alimenter_list_active();
                Alimenter_list_desactive();
            }
            else
            {
                if (MessageBox.Show("Cette clé : " + nouveau_ge.Text + " existe déja, est ce que vous voulez supprimer la clé  " + Num_GE_Modif.Text + "", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ot.Connect();
                    int count_1 = ot.ExecuteScalarInt(
                        "SELECT COUNT(CLE_ID) FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                        "WHERE CLE_ID = (SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num)",
                        new OracleParameter(":num", Num_GE_Modif.Text)
                    );
                    ot.Close();

                    if (count_1 != 0)
                    {
                        string sqlqry =
                            "DELETE FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                            "WHERE CLE_ID = (SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num)";

                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry, new OracleParameter(":num", Num_GE_Modif.Text));

                        sqlqry = "DELETE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num";
                        ot.ExecuteNonQuery(sqlqry, new OracleParameter(":num", Num_GE_Modif.Text));
                        ot.Close();

                        MessageBox.Show("votre numéro ge est modifié avec succés.");
                        nouveau_ge.Text = string.Empty;
                        Num_GE_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                        Alimenter_champ_cle_actu();
                        LIST_ACTIVE.Items.Clear();
                        Alimenter_list_active();
                        Alimenter_list_desactive();
                    }
                    else
                    {
                        string sqlqry = "DELETE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num";

                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry, new OracleParameter(":num", Num_GE_Modif.Text));
                        ot.Close();

                        MessageBox.Show("votre numéro ge est modifié avec succés.");
                        nouveau_ge.Text = string.Empty;
                        Num_GE_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                        Alimenter_champ_cle_actu();
                        LIST_ACTIVE.Items.Clear();
                        Alimenter_list_active();
                        Alimenter_list_desactive();
                    }
                }
                else
                {
                    nouveau_ge.Text = string.Empty;
                    MessageBox.Show("votre opération est annulée.");
                }
            }
        }

        // Modifie le numéro fournisseur.
        private void Modifier_Num_Four_Click(object sender, EventArgs e)
        {
            string NVnumeroFR = nouveau_num_four.Text;

            if (String.IsNullOrEmpty(nouveau_num_four.Text))
            {
                MessageBox.Show("Votre champ est vide");
            }
            else if (NVnumeroFR.Contains(" ") || NVnumeroFR.Contains("-"))
            {
                MessageBox.Show("Le champ 'Numéro GE' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                Connect_db ot = new Connect_db();
                string sqlqry =
                    "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET NUMERO_FOURNISSEUR = :newFour " +
                    "WHERE NUMERO = :num";

                ot.Connect();
                ot.ExecuteNonQuery(
                    sqlqry,
                    new OracleParameter(":newFour", nouveau_num_four.Text),
                    new OracleParameter(":num", Num_GE_Modif.Text)
                );
                ot.Close();

                MessageBox.Show("votre numéro fournisseur (numéro de série) est modifié avec succés.");
                nouveau_num_four.Text = string.Empty;
                Num_Four_Modif.Text = string.Empty;
                Affichier_Cle();
            }
        }

        // Modifie le type d'une clé.
        private void Modifier_type_Click(object sender, EventArgs e)
        {
            string NVTYPE = nouveau_type.Text;

            if (NVTYPE.Contains(" ") || NVTYPE.Contains("-"))
            {
                MessageBox.Show("Le champ 'type' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (String.IsNullOrEmpty(NVTYPE))
            {
                MessageBox.Show("Votre champ est vide");
            }
            else
            {
                Connect_db ot = new Connect_db();

                ot.Connect();
                int count = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE TYPE_ID = " +
                    "(SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :oldType)",
                    new OracleParameter(":oldType", Type_Modif.Text)
                );
                ot.Close();

                if (count == 1)
                {
                    ot.Connect();
                    int count_1 = ot.ExecuteScalarInt(
                        "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :newType",
                        new OracleParameter(":newType", nouveau_type.Text)
                    );
                    ot.Close();

                    if (count_1 == 0)
                    {
                        string sqlqry =
                            "UPDATE TBSCT.EPE_UUM_HYTORC_TYPE_CLE SET TYPE = :newType " +
                            "WHERE TYPE = :oldType";

                        ot.Connect();
                        ot.ExecuteNonQuery(
                            sqlqry,
                            new OracleParameter(":newType", nouveau_type.Text),
                            new OracleParameter(":oldType", Type_Modif.Text)
                        );
                        ot.Close();

                        MessageBox.Show("votre type de clé est modifié avec succés.");
                        nouveau_type.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                    }
                    else
                    {
                        string sqlqry =
                            "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET TYPE_ID = " +
                            "(SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :newType) " +
                            "WHERE NUMERO = :num";

                        ot.Connect();
                        ot.ExecuteNonQuery(
                            sqlqry,
                            new OracleParameter(":newType", nouveau_type.Text),
                            new OracleParameter(":num", Num_GE_Modif.Text)
                        );
                        ot.Close();

                        MessageBox.Show("votre type de clé est modifié avec succés.");

                        if (MessageBox.Show("Ce type:" + Type_Modif.Text + " n'est associé à aucune clé, est ce que vous voulez le supprimer ", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            ot.Connect();
                            sqlqry = "DELETE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :oldType";
                            ot.ExecuteNonQuery(sqlqry, new OracleParameter(":oldType", Type_Modif.Text));
                            ot.Close();

                            MessageBox.Show("votre type " + Type_Modif.Text + " est supprimer avec succés.");
                            nouveau_type.Text = string.Empty;
                            Type_Modif.Text = string.Empty;
                            Num_GE_Modif.Items.Clear();
                            Alimenter_Num_GE();
                            Affichier_Cle();
                        }
                        else
                        {
                            nouveau_type.Text = string.Empty;
                            Type_Modif.Text = string.Empty;
                            Num_GE_Modif.Items.Clear();
                            Alimenter_Num_GE();
                            Affichier_Cle();
                            MessageBox.Show("La supression du type est annulée");
                        }
                    }
                }
                else
                {
                    ot.Connect();
                    int count_1 = ot.ExecuteScalarInt(
                        "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :newType",
                        new OracleParameter(":newType", nouveau_type.Text)
                    );
                    ot.Close();

                    if (count_1 != 0)
                    {
                        string sqlqry =
                            "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET TYPE_ID = " +
                            "(SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :newType) " +
                            "WHERE NUMERO = :num";

                        ot.Connect();
                        ot.ExecuteNonQuery(
                            sqlqry,
                            new OracleParameter(":newType", nouveau_type.Text),
                            new OracleParameter(":num", Num_GE_Modif.Text)
                        );
                        ot.Close();

                        MessageBox.Show("votre type de clé est modifié avec succés.");
                        nouveau_type.Text = string.Empty;
                        Type_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                    }
                    else
                    {
                        string sqlqry = "INSERT INTO TBSCT.EPE_UUM_HYTORC_TYPE_CLE(ID_TYPE,TYPE) VALUES(TBSCT.ID_TYPE.NEXTVAL, :newType)";

                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry, new OracleParameter(":newType", nouveau_type.Text));

                        sqlqry =
                            "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET TYPE_ID = " +
                            "(SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :newType) " +
                            "WHERE NUMERO = :num";

                        ot.ExecuteNonQuery(
                            sqlqry,
                            new OracleParameter(":newType", nouveau_type.Text),
                            new OracleParameter(":num", Num_GE_Modif.Text)
                        );
                        ot.Close();

                        MessageBox.Show("votre type de clé est modifié avec succés.");
                        nouveau_type.Text = string.Empty;
                        Type_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                    }
                }
            }
        }
    }
}

/*namespace UUM_Hytroc
{
    public partial class Hytorc_Admin : Form
    {
        bool erreurDetectee;
        // Dossier d'archive configuré via App.config (environnement entreprise)
        private string archiveFolder;
        List<string> filesWithErrors = new List<string>();
        List<string> filesToRemove = new List<string>();
        public Hytorc_Admin()
        {
            InitializeComponent();
            // Lecture du chemin depuis App.config
            archiveFolder = ConfigurationManager.AppSettings["ArchiveFolder"];
        }
     
        private void Form3_Load(object sender, EventArgs e)
        {
            Alimenter_Num_GE();
            //Pour afficher le sso et le nom de l'utilisateur sur l'interface.
            Afficher_SSO();
            //Pour alimenter la liste avec les clés en etat activer.
            Alimenter_list_active();
            //Pour alimenter la liste avec les clés en etat désactiver.
            Alimenter_list_desactive();
            //alimenter la list avec des types
            Alimenter_list_type();
        }
               
        // ----------------------------------------- PARTIE POUR RECUP ET AFFICHAGE DE SSO ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------     
        private string GetLoggedInSSO()
        {
            string sso = WindowsIdentity.GetCurrent().Name;
            string username = sso.Split('\\')[1];
            return username;
        }
   
        private void Afficher_SSO()
        {
            Connect_db ot = new Connect_db();
            string sso = GetLoggedInSSO();

            string sqlqry = "SELECT NOM FROM TBSCT.EPE_UUM_HYTORC_ADMINISTRATION WHERE SSO_ADMIN = :sso";

            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry, new Dictionary<string, object>
            {
                { ":sso", sso }
            });

            string nom = null;
            if (dr != null && dr.HasRows)
            {
                while (dr.Read())
                {
                    nom = dr["NOM"]?.ToString();
                }
            }

            ot.Close();
            Utilisateur.Text = "Votre SSO: " + sso + '\n' + nom;
        }
        //------------------------------------------------ PARTIE POUR ALIMENTER LES COMBOXS -------------------------------------------------------------------------------------------------
        private void Alimenter_list_desactive() 
        {
            string sqlqry = " SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = 'DESACTIVE' order by NUMERO ASC";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    List_DESACTIVE.Items.Add(dr["NUMERO"].ToString());

                }
            }
            ot.Close();
        }
        private void Alimenter_list_type()
        {
            string sqlqry= " SELECT TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE order by TYPE ASC ";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    list_type.Items.Add(dr["TYPE"].ToString());

                }
            }
            ot.Close();
        }
        private void Alimenter_list_active()
        {
            string sqlqry = " SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE ETAT = 'ACTIVE' order by NUMERO ASC ";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    LIST_ACTIVE.Items.Add(dr["NUMERO"].ToString());

                }
            }
            ot.Close();

        }
        //---------------------------------------------------------- PARTIE POUR GERER LES FICHIERS ---------------------------------------------------------------------------------------------
        private void Telecharger_fichier_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Sélectionner un ou plusieurs fichiers texte";
                openFileDialog.Filter = "Fichiers texte (*.txt;*.xys)|*.txt;*.xys|Tous les fichiers (*.*)|*.*";
                openFileDialog.Multiselect = true;
                string inputFolder = ConfigurationManager.AppSettings["InputFolder"];

                if (!string.IsNullOrWhiteSpace(inputFolder))
                {
                    openFileDialog.InitialDirectory = inputFolder;
                }
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Ajouter les chemins des fichiers sélectionnés à la ListBox
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        Fichier.Items.Add(fileName);
                    }
                }
            }
        }

        private void DELETEDatabase(string numeroCle)
        {
            Connect_db ot = new Connect_db();
            try
            {
                string deleteQuery =
                    "DELETE FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                    "WHERE CLE_ID = (SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num)";

                ot.Connect();
                ot.ExecuteNonQuery(deleteQuery, new Dictionary<string, object>
                {
                    { ":num", numeroCle }
                });
                ot.Close();
            }
            catch (Exception ex)
            {
                // ✅ pas de ex.Data["FileName"] => risque NullReference
                MessageBox.Show("Erreur lors de la suppression : " + ex.Message);
            }
        }
       
        private void INSERTDatabase(string numeroCle, int pression, int couple)
        {
            Connect_db ot = new Connect_db();
            try
            {
                string insertQuery =
                    "INSERT INTO TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION " +
                    "(ID_COUPLE_PRESSION, PRESSION, COUPLE, CLE_ID) " +
                    "VALUES (TBSCT.ID_COUPLE_PRESSION.NEXTVAL, :pression, :couple, " +
                    "(SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num))";

                ot.Connect();
                ot.ExecuteNonQuery(insertQuery, new Dictionary<string, object>
                {
                    { ":pression", pression },
                    { ":couple", couple },
                    { ":num", numeroCle }
                });
                ot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'insertion : " + ex.Message);
            }
        }
       
        private void UPDATEDATE(string DateControl, string numeroCle)
        {
            Connect_db ot = new Connect_db();
            try
            {
                string Updatequery =
                    "UPDATE TBSCT.EPE_UUM_HYTORC_CLE " +
                    "SET DATE_DE_CONTROL = TO_DATE(:dateCtrl, 'DD/MM/YYYY') " +
                    "WHERE NUMERO = :num";

                ot.Connect();
                ot.ExecuteNonQuery(Updatequery, new Dictionary<string, object>
                {
                    { ":dateCtrl", DateControl },
                    { ":num", numeroCle }
                });
                ot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la mise à jour de la date : " + ex.Message);
            }
        }
        // Fonction pour nettoyer le numéro de fournisseur
        private string CleanSupplierNumber(string input)
        {
            // Supprimer les espaces et les tirets du numéro de fournisseur
            return input.Replace(" ", "").Replace("-", "");
        }
        // trairement du fichier et mise a jour de base de données.
        public void ProcessFile(string filePath)
        {
            try
            {
           
                // Lecture du contenu du fichier

                string[] lines = File.ReadAllLines(filePath);

                // Extraction du nom du fichier

                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // Séparation des parties du nom du fichier en fonction du séparateur "-"

                string[] fileNameParts = fileName.Split(new string[] { "-°-" }, StringSplitOptions.None);

                if (!IsValidFileName(fileName))
                {
                    erreurDetectee = true;
                    MessageBox.Show($"Le nom du fichier contient des erreurs : {fileName}. Assurez-vous qu'il respecte le format spécifié.", "Erreur de format de fichier", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                
                    // Vérification s'il y a suffisamment de parties dans le nom du fichier                
                    if (fileNameParts.Length > 2)
                    {
                        // Récupération du numéro de clé

                        string numeroCle = fileNameParts[2].Trim();
                        //string numeroSerie = fileNameParts[1].Trim();
                        string numeroSerie = CleanSupplierNumber(fileNameParts[1].Trim());  // Nettoyage du numéro de fournisseur
                        string monthYear = fileNameParts[3].Trim();
                        string month = monthYear.Substring(0, 2); // Prend les deux premiers caractères pour le mois
                        string year = monthYear.Substring(2, 4);
                        string dateString = month + "/" + year;

                        DateTime dateControl = DateTime.ParseExact(dateString, "MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime firstDayOfMonth = new DateTime(dateControl.Year, dateControl.Month, 1);
                        string formattedDateControl = firstDayOfMonth.ToString("dd/MM/yyyy");
                        // Vérification s'il y a suffisamment de lignes dans le fichier

                        if (lines.Length >= 2)
                        {
                       
                        Connect_db ot = new Connect_db();
                        string sqlqry =
                            "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE " +
                            "WHERE NUMERO = :num AND NUMERO_FOURNISSEUR = :serie";

                        ot.Connect();
                        int count = ot.ExecuteScalarInt(sqlqry, new Dictionary<string, object>
                        {
                            { ":num", numeroCle },
                            { ":serie", numeroSerie }
                        });
                        ot.Close();
                        if (count != 0)
                        {

                                erreurDetectee = false;
                                try
                                {
                                    // Appel de la fonction deletdatabese pour supprimer la base actuelle.

                                    DELETEDatabase(numeroCle);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                int i = 0;

                                //boucle pour mettre à jour toutes les lignes du doc.

                                for (i = 0; i < lines.Length; i++)
                                {
                                    int PR = int.Parse(lines[i].Split('\t')[0]);
                                    int CL = int.Parse(lines[i].Split('\t')[1]);
                                    try
                                    {
                                        //appel de la methode insertdatabase pour la mise a jour des lignes supprimer

                                        INSERTDatabase(numeroCle, PR, CL);

                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }

                                }
                                // modifier la date avec la nouvelle 
                                try
                                {
                                    UPDATEDATE(formattedDateControl, numeroCle);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }

                                Modifier_Par(numeroCle);
                            }
                            else
                            {
                                erreurDetectee = true;
                                MessageBox.Show("Le numéro de clé correspondant au fichier n'exhiste pas : " + fileName);
                            }
                        }
                        else
                        {
                            erreurDetectee = true;
                            MessageBox.Show("Le fichier ne contient pas suffisamment de lignes pour être traité correctement: " + fileName);
                        }
                    }
                    else
                    {
                        erreurDetectee = true;
                        MessageBox.Show("Format du nom de fichier incorrect. Le numéro de clé n'a pas pu être extrait: " + fileName);
                    }
                
            }
            catch (Exception ex)
            {
                // Propage l'exception pour être capturée au niveau supérieur
                throw new Exception($"Erreur lors du traitement du fichier '{filePath}': {ex.Message}", ex);
            }
           
        }
        private bool IsValidFileName(string fileName)
        {
            return fileName.Contains("-°-");
        }
        // Enregistrer les modifs sur la base.
        private void Enregistrer_fichier_Click(object sender, EventArgs e)
        {
            try
            {
                if (Fichier.SelectedItem != null)
                {
                    filesToRemove.Clear();
                    filesWithErrors.Clear();
                    foreach (string filePath in Fichier.SelectedItems)
                    {
                        try
                        {
                            string fileName = Path.GetFileNameWithoutExtension(filePath);
                            if (!IsValidFileName(fileName))
                            {
                                filesWithErrors.Add(filePath);
                                MessageBox.Show("Nom du fichier contient des erreurs :" + fileName);
                                continue; // Passer au fichier suivant
                            }
                            string[] fileNameParts = fileName.Split(new string[] { "-°-" }, StringSplitOptions.None);
                            ProcessFile(filePath);

                            if (erreurDetectee==false)
                            {
                                filesToRemove.Add(filePath);
                            }
                            else
                            {
                                filesWithErrors.Add(filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Gérer l'erreur spécifique au fichier, mais continuer le traitement des autres fichiers
                            MessageBox.Show(ex.Message);
                        }
                    }

                    if (filesToRemove.Count > 0)
                    {
                         // Si des fichiers ont été traités avec succès, les déplacer vers le dossier d'archive
                        foreach (string filePathToRemove in filesToRemove)
                        {
                            string fileName = Path.GetFileName(filePathToRemove);
                            string archivedFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(fileName)}";
                            string destinationPath = Path.Combine(archiveFolder, archivedFileName);
                            File.Move(filePathToRemove, destinationPath);
                        }
                        string succesMessage = "Enregistrement effectué avec succès.";
                        foreach (string filePathToRemove in filesToRemove)
                        {
                            succesMessage += $"{Path.GetFileName(filePathToRemove)}\n";
                            Fichier.Items.Remove(filePathToRemove);                          
                        }
                        MessageBox.Show(succesMessage);

                    }
                    else if (filesWithErrors.Count > 0)
                    {
                        string errorMessage = "Le traitement des fichiers suivants a rencontré des erreurs:\n";
                        foreach (string errorFile in filesWithErrors)
                        {
                            errorMessage += $"{Path.GetFileName(errorFile)}\n";
                        }
                        MessageBox.Show(errorMessage);
                    }
                }
                else
                {
                    MessageBox.Show("Veuillez sélectionner un fichier dans la liste avant de consulter.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur inattendue s'est produite : {ex.Message}");
            }
        }
         // Consulter un fichier telecharger sur la listbox
         private void Consulter_Bouton_Click(object sender, EventArgs e)
         {
             if (Fichier.SelectedItem != null)
             {
                 // Ouvrir le fichier avec le programme par défaut
                 System.Diagnostics.Process.Start(Fichier.SelectedItem.ToString());
             }
             else
             {
                 MessageBox.Show("Veuillez sélectionner un fichier dans la liste avant de consulter.");
             }
         }

         //Supprimer un fichier telecharger sur la listbox.

         private void Supprimer_Click(object sender, EventArgs e)
         {
             if (Fichier.SelectedItem != null)
             {
                 string filePathToDelete = Fichier.SelectedItem.ToString();

                 // Supprimer le fichier de la liste visuelle
                 Fichier.Items.Remove(filePathToDelete);

                 // Supprimer le fichier de la liste des fichiers en erreur s'il est présent
                 if (filesWithErrors.Contains(filePathToDelete))
                 {
                     filesWithErrors.Remove(filePathToDelete);
                 }               
                 else
                 {
                     //MessageBox.Show("Le fichier sélectionné n'existe pas sur le système de fichiers.");
                 }
             }
             else
             {
                 MessageBox.Show("Veuillez sélectionner un fichier dans la liste avant de supprimer.");
             }
         }


        // Journalise les modifications (traçabilité) : qui a fait quoi, et quand.
        // NOTE recruteur : l'objectif est d'avoir un audit trail côté base (admin + action + clé).
        private void Modifier_Par(string numeroCle)
        {
            var ot = new Connect_db();
            string sso = GetLoggedInSSO();

            try
            {
                ot.Connect();

                // 1) Créer l'entrée de modification (audit)
                string insert =
                    "INSERT INTO TBSCT.EPE_UUM_HYTORC_MODIFICATION_CLE " +
                    "(ID_MODIFICATION, DATE_DE_MISE_A_JOUR, ID_ADMINISTRATEUR, ID_CLE_MODIFIER) " +
                    "VALUES (TBSCT.ID_MODIFICATION.NEXTVAL, SYSDATE, " +
                    "(SELECT ID_ADMIN FROM TBSCT.EPE_UUM_HYTORC_ADMINISTRATION WHERE SSO_ADMIN = :sso), " +
                    "(SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num))";

                ot.ExecuteNonQuery(insert, new Dictionary<string, object>
                {
                    { ":sso", sso },
                    { ":num", numeroCle }
                });

                // 2) Déterminer l'action en fonction du bouton utilisé
                string action =
                    Ajouter.Focused ? "AJOUTER CLE" :
                    Reactiver_bouton.Focused ? "ACTIVER CLE" :
                    Desactiver_cle.Focused ? "DESACTIVER CLE" :
                    Enregister.Focused ? "MISE A JOUR CLE" :
                    "ACTION_INCONNUE";

                // 3) Mettre à jour le champ ACTION sur la dernière modification
                string updateAction =
                    "UPDATE TBSCT.EPE_UUM_HYTORC_MODIFICATION_CLE " +
                    "SET ACTION = :action " +
                    "WHERE ID_MODIFICATION = (SELECT MAX(ID_MODIFICATION) FROM TBSCT.EPE_UUM_HYTORC_MODIFICATION_CLE)";

                ot.ExecuteNonQuery(updateAction, new Dictionary<string, object>
                {
                    { ":action", action }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la journalisation des modifications : " + ex.Message);
            }
            finally
            {
                ot.Close();
            }
        }
        //------------------------------------------- PARTIE POUR AJOUTER UNE CLE -------------------------------------------------------------------------------------------
        private void Ajouter_Click(object sender, EventArgs e)
        {
            try
            {
                string numeroGE = Numero_ge_champ.Text?.Trim();
                string numeroFournisseur = Numero_fournisseur_champ.Text?.Trim();
                string typeCle = list_type.Text?.Trim();
                string dateCtrlText = Date_control_champ.Text?.Trim();

                // Validations UI (inchangées, mais un peu sécurisées)
                if (string.IsNullOrWhiteSpace(numeroGE) ||
                    string.IsNullOrWhiteSpace(numeroFournisseur) ||
                    string.IsNullOrWhiteSpace(typeCle) ||
                    string.IsNullOrWhiteSpace(dateCtrlText))
                {
                    MessageBox.Show("Veuillez renseigner touts les champs.");
                    return;
                }

                // Vérification des espaces et des tirets
                if (numeroGE.Contains(" ") || numeroGE.Contains("-"))
                {
                    MessageBox.Show("Le champ 'Numéro GE' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (numeroFournisseur.Contains(" ") || numeroFournisseur.Contains("-"))
                {
                    MessageBox.Show("Le champ 'Numéro Fournisseur' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Date format
                if (!DateTime.TryParseExact(dateCtrlText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    MessageBox.Show("Format de date incorrect. Veuillez entrer une date au format DD/MM/YYYY.");
                    Date_control_champ.Text = string.Empty;
                    return;
                }

                var ot = new Connect_db();

                // 1) Vérifier si le type existe
                int typeExists;
                ot.Connect();
                typeExists = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :type",
                    new Dictionary<string, object> { { ":type", typeCle } }
                );
                ot.Close();

                // 2) Vérifier si la clé existe déjà
                int cleExists;
                ot.Connect();
                cleExists = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num",
                    new Dictionary<string, object> { { ":num", numeroGE } }
                );
                ot.Close();

                if (cleExists != 0)
                {
                    MessageBox.Show("Votre clé existe déjà");
                    return;
                }

                // 3) Si le type n'existe pas → proposer ajout
                if (typeExists == 0)
                {
                    var res = MessageBox.Show(
                        $"Ce Type: {typeCle} n'existe pas, est-ce que vous voulez l'ajouter ?",
                        "Confirmation",
                        MessageBoxButtons.YesNo
                    );

                    if (res != DialogResult.Yes)
                    {
                        Date_control_champ.Text = string.Empty;
                        list_type.Text = string.Empty;
                        Numero_ge_champ.Text = string.Empty;
                        Numero_fournisseur_champ.Text = string.Empty;
                        MessageBox.Show("Votre opération est annulée.");
                        return;
                    }

                    // Insert type
                    ot.Connect();
                    ot.ExecuteNonQuery(
                        "INSERT INTO TBSCT.EPE_UUM_HYTORC_TYPE_CLE(ID_TYPE, TYPE) VALUES (TBSCT.ID_TYPE.NEXTVAL, :type)",
                        new Dictionary<string, object> { { ":type", typeCle } }
                    );
                    ot.Close();

                    MessageBox.Show("Nouveau type ajouté !");
                    list_type.Items.Clear();
                    Alimenter_list_type();
                }

                // 4) Insert clé (type existe désormais)
                string insertCle =
                    "INSERT INTO TBSCT.EPE_UUM_HYTORC_CLE " +
                    "(ID_CLE, NUMERO_FOURNISSEUR, DATE_DE_CONTROL, TYPE_ID, NUMERO, ETAT) " +
                    "VALUES " +
                    "(TBSCT.ID_CLE.NEXTVAL, :four, TO_DATE(:dateCtrl,'DD/MM/YYYY'), " +
                    "(SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = :type), :num, 'ACTIVE')";

                ot.Connect();
                ot.ExecuteNonQuery(insertCle, new Dictionary<string, object>
        {
            { ":four", numeroFournisseur },
            { ":dateCtrl", dateCtrlText },
            { ":type", typeCle },
            { ":num", numeroGE }
        });
                ot.Close();

                // 5) UI refresh + audit
                if (MessageBox.Show("Clé ajoutée avec succès.") == DialogResult.OK)
                {
                    LIST_ACTIVE.Items.Clear();
                    List_DESACTIVE.Items.Clear();
                    Alimenter_list_active();
                    Alimenter_list_desactive();

                    Num_GE_Modif.Items.Clear();
                    Alimenter_Num_GE();

                    Modifier_Par(numeroGE);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message + "\nStack Trace : " + ex.StackTrace);
            }
        }

        //-------------------------------------------------------------- PARTIE POUR ACTIVER/DESACTIVER UNE CLE -----------------------------------------------------------------

        //activer
        // Activer une clé (passer ETAT de DESACTIVE -> ACTIVE)
        private void Reactiver_bouton_Click(object sender, EventArgs e)
        {
            Connect_db ot = new Connect_db();

            if (String.IsNullOrEmpty(List_DESACTIVE.Text))
            {
                MessageBox.Show("Entrez un numéro.");
                return;
            }

            try
            {
                // 1) Vérifier que la clé existe
                ot.Connect();
                int count = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num",
                    new Dictionary<string, object> { { ":num", List_DESACTIVE.Text } }
                );
                ot.Close();

                if (count == 0)
                {
                    List_DESACTIVE.Text = string.Empty;
                    MessageBox.Show("votre cle n'existe pas.");
                    return;
                }

                // 2) Mettre à jour l'état
                string sqlqry = "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET ETAT = 'ACTIVE' WHERE NUMERO = :num";
                ot.Connect();
                ot.ExecuteNonQuery(sqlqry, new Dictionary<string, object> { { ":num", List_DESACTIVE.Text } });
                ot.Close();

                // 3) Rafraîchir les listes UI
                List_DESACTIVE.Items.Clear();
                LIST_ACTIVE.Items.Clear();
                Alimenter_list_desactive();
                Alimenter_list_active();

                // 4) Journaliser l'action
                if (MessageBox.Show("Clé activée avec succés.") == DialogResult.OK)
                {
                    Modifier_Par(List_DESACTIVE.Text);
                }

                List_DESACTIVE.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show("Erreur lors de l'activation : " + ex.Message);
            }
        }
        // Désactiver une clé (passer ETAT de ACTIVE -> DESACTIVE)
        private void Desactiver_cle_Click(object sender, EventArgs e)
        {
            Connect_db ot = new Connect_db();

            if (String.IsNullOrEmpty(LIST_ACTIVE.Text))
            {
                MessageBox.Show("Entrez un numéro.");
                return;
            }

            try
            {
                // 1) Vérifier que la clé existe
                ot.Connect();
                int count = ot.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = :num",
                    new Dictionary<string, object> { { ":num", LIST_ACTIVE.Text } }
                );
                ot.Close();

                if (count == 0)
                {
                    LIST_ACTIVE.Text = string.Empty;
                    MessageBox.Show("votre clé n'existe pas.");
                    return;
                }

                // 2) Mettre à jour l'état
                string sqlqry = "UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET ETAT = 'DESACTIVE' WHERE NUMERO = :num";
                ot.Connect();
                ot.ExecuteNonQuery(sqlqry, new Dictionary<string, object> { { ":num", LIST_ACTIVE.Text } });
                ot.Close();

                // 3) Rafraîchir les listes UI
                LIST_ACTIVE.Items.Clear();
                List_DESACTIVE.Items.Clear();
                Alimenter_list_active();
                Alimenter_list_desactive();

                // 4) Journaliser l'action
                if (MessageBox.Show("Clé désactivée avec succés.") == DialogResult.OK)
                {
                    Modifier_Par(LIST_ACTIVE.Text);
                }

                LIST_ACTIVE.Text = string.Empty;
            }
            catch (Exception ex)
            {
                ot.Close();
                MessageBox.Show("Erreur lors de la désactivation : " + ex.Message);
            }
        }
        private void GenererFichierNumerosCle(string filePath)
        {
        try
        {
            // Créer un nouveau classeur Excel
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Feuille1");

            // Ajouter un titre au document
            worksheet.Cell("A1").Value = "Liste des numéros de clé";
            worksheet.Range("A1:E1").Merge();

            // En-têtes de colonne
            string[] headers = { "Numero ge", "Numero fournisseur", "Type de clé", "date du dernier control","Etat" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(2, i + 1).Value = headers[i];
            }

            // Remplir les données du tableau à partir de la base de données
            string sqlqry = " SELECT  NUMERO,NUMERO_FOURNISSEUR,TYPE,DATE_DE_CONTROL,ETAT" +
                            " FROM TBSCT.EPE_UUM_HYTORC_CLE INNER JOIN " +
                            " TBSCT.EPE_UUM_HYTORC_TYPE_CLE ON TBSCT.EPE_UUM_HYTORC_CLE.TYPE_ID = TBSCT.EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE " +                            
                            " ORDER BY DATE_DE_CONTROL ASC ";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);
            int row = 3; // Commencer à la troisième ligne après les en-têtes
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    for (int col = 0; col < dr.FieldCount; col++)
                    {
                        worksheet.Cell(row, col + 1).Value = dr[col];
                    }
                    row++;
                }
            }
            ot.Close();

            // Sauvegarder le classeur Excel
            workbook.SaveAs(filePath);
            MessageBox.Show("Document Excel généré avec succès à l'emplacement: " + filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Une erreur inattendue s'est produite : {ex.Message}");
        }
    }

    private void Extraire_boutton_Click(object sender, EventArgs e)
    {
        // Créer une boîte de dialogue pour enregistrer le fichier
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Fichiers Excel|*.xlsx";
        saveFileDialog.Title = "Enregistrer le fichier Excel";
        saveFileDialog.ShowDialog();
        // Vérifier si l'utilisateur a cliqué sur OK dans la boîte de dialogue
        if (saveFileDialog.FileName != "")
        {
            // Générer le fichier Excel et l'enregistrer à l'emplacement spécifié
            try
            {
                GenererFichierNumerosCle(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur inattendue s'est produite : {ex.Message}");
            }
        }
    }
    //----------------------------------------------------------------------
    private void Affichier_Cle()
    {
            string sqlqry = " select NUMERO,TYPE,NUMERO_FOURNISSEUR,DATE_DE_CONTROL from TBSCT.EPE_UUM_HYTORC_TYPE_CLE" +
                            " inner join TBSCT.EPE_UUM_HYTORC_CLE " +
                            " on TBSCT.EPE_UUM_HYTORC_TYPE_CLE.ID_TYPE = TBSCT.EPE_UUM_HYTORC_CLE.TYPE_ID " +
                            " where NUMERO = '" + Num_GE_Modif.Text + "'";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);

            if (dr.HasRows)
            {

                while (dr.Read())
                {
                    Num_Four_Modif.Text = dr["NUMERO_FOURNISSEUR"].ToString();
                    Type_Modif.Text = dr["TYPE"].ToString();
                    Date_Modif.Text = dr["DATE_DE_CONTROL"].ToString();
                    Num_GE_Modif.Text = dr["NUMERO"].ToString();
                }
            }

            ot.Close();

    }

    //-------------------------------------------------------------------- EVENEMENT PAS UTILISE ---------------------------------------------------------------------------
        private void Aide_Boutton_Click(object sender, EventArgs e)
        {
            Hide(); // Cache la fenêtre actuelle
            var form4 = new Hytorc_Telecharger(); // Crée une nouvelle instance de Form4
            form4.ShowDialog(); // Affiche la nouvelle Form4 de manière modale
            Show(); // Affiche à 
        }
        private void Alimenter_Num_GE()
        {
            string sqlqry = " SELECT NUMERO FROM TBSCT.EPE_UUM_HYTORC_CLE ORDER BY NUMERO ASC ";
            Connect_db ot = new Connect_db();
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    Num_GE_Modif.Items.Add(dr["NUMERO"].ToString());

                }
            }
            ot.Close();
        }
       
        private void Num_GE_Modif_SelectedIndexChanged(object sender, EventArgs e)
        {
            Affichier_Cle();
        }
        private void Alimenter_champ_cle_actu()
        {
            Connect_db ot = new Connect_db();
            string sqlqry = " SELECT NUMERO  FROM  TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO_FOURNISSEUR = '" + Num_Four_Modif.Text + "'";
            ot.Connect();
            OracleDataReader dr = ot.SelectData(sqlqry);
            if (dr.HasRows)
            {

                while (dr.Read())
                {
                    Num_GE_Modif.Text = dr["NUMERO"].ToString();

                }
            }
             ot.Close();
        }

        private void Modifier_numéro_ge_Click(object sender, EventArgs e)
        {
            string NVnumeroGE = nouveau_ge.Text;
            
            // Vérification des espaces et des tirets
            if (NVnumeroGE.Contains(" ") || NVnumeroGE.Contains("-"))
            {
                MessageBox.Show("Le champ 'Numéro GE' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }            
            Connect_db ot = new Connect_db();
            string sqlqry = " SELECT COUNT(*) AS NOMBRE FROM  TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = '" + nouveau_ge.Text + "'";
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
            if (String.IsNullOrEmpty(nouveau_ge.Text))
            {
                MessageBox.Show("Votre champ est vide");
            }
            else if (count == 0)
            {
                sqlqry = " UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET NUMERO = '" + nouveau_ge.Text + "'" +
                         " WHERE NUMERO ='" + Num_GE_Modif.Text + "'";
                ot.Connect();
                ot.ExecuteNonQuery(sqlqry);
                ot.Close();
                MessageBox.Show("votre numéro ge est modifié avec succés.");
                nouveau_ge.Text = string.Empty;
                Num_GE_Modif.Text = string.Empty;
                Num_GE_Modif.Items.Clear();
                Alimenter_Num_GE();
                Affichier_Cle();
                Alimenter_champ_cle_actu();
                LIST_ACTIVE.Items.Clear();
                Alimenter_list_active();
                Alimenter_list_desactive();
            }
            else 
            {
                if (MessageBox.Show("Cette clé : " + nouveau_ge.Text + " existe déja, est ce que vous voulez supprimer la clé  " + Num_GE_Modif.Text + "", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sqlqry = " SELECT COUNT(CLE_ID) AS NOMBRE FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION WHERE CLE_ID = (SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = '" + Num_GE_Modif.Text + "')";
                    ot.Connect();
                    dr = ot.SelectData(sqlqry);
                    int count_1 = 0;
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            count_1 = Convert.ToInt32(dr["NOMBRE"]);
                        }
                    }
                    ot.Close();
                    if (count_1 != 0)
                    {
                        sqlqry = " DELETE FROM TBSCT.EPE_UUM_HYTORC_COUPLES_PRESSION WHERE CLE_ID = (SELECT ID_CLE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO = '" + Num_GE_Modif.Text + "')";
                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry);
                        
                        sqlqry = " DELETE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO ='" + Num_GE_Modif.Text + "'";
                        
                        ot.ExecuteNonQuery(sqlqry);
                        ot.Close();
                        MessageBox.Show("votre numéro ge est modifié avec succés.");
                        nouveau_ge.Text = string.Empty;
                        Num_GE_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                        Alimenter_champ_cle_actu();
                        LIST_ACTIVE.Items.Clear();
                        Alimenter_list_active();
                        Alimenter_list_desactive();
                    }
                    else
                    {
                        sqlqry = " DELETE FROM TBSCT.EPE_UUM_HYTORC_CLE WHERE NUMERO ='" + Num_GE_Modif.Text + "'";
                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry);
                        ot.Close();
                        MessageBox.Show("votre numéro ge est modifié avec succés.");
                        nouveau_ge.Text = string.Empty;
                        Num_GE_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                        Alimenter_champ_cle_actu();
                        LIST_ACTIVE.Items.Clear();
                        Alimenter_list_active();
                        Alimenter_list_desactive();
                    }
                    
                }
                else
                {
                    nouveau_ge.Text = string.Empty;
                    MessageBox.Show("votre opération est annulée.");
                }
                
            }
        }

        private void Modifier_Num_Four_Click(object sender, EventArgs e)
        {
            string NVnumeroFR = nouveau_num_four.Text;

            // Vérification des espaces et des tirets
            
             if (String.IsNullOrEmpty(nouveau_num_four.Text))
             {
                MessageBox.Show("Votre champ est vide");
             }
            else if (NVnumeroFR.Contains(" ") || NVnumeroFR.Contains("-"))
             {
                MessageBox.Show("Le champ 'Numéro GE' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
             }
            else
            {
                Connect_db ot = new Connect_db();
                string sqlqry = " UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET NUMERO_FOURNISSEUR = '" + nouveau_num_four.Text + "'" +
                                " WHERE NUMERO ='" + Num_GE_Modif.Text + "'";
                ot.Connect();
                ot.ExecuteNonQuery(sqlqry);
                ot.Close();
                MessageBox.Show("votre numéro fournisseur (numéro de série) est modifié avec succés.");
                nouveau_num_four.Text = string.Empty;
                Num_Four_Modif.Text = string.Empty;
                Affichier_Cle();
            }
        }

        private void Modifier_type_Click(object sender, EventArgs e)
        {
            string NVTYPE = nouveau_type.Text;

            // Vérification des espaces et des tirets
            if (NVTYPE.Contains(" ") || NVTYPE.Contains("-"))
            {
                MessageBox.Show("Le champ 'type' ne doit pas contenir d'espaces ou de tirets.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (String.IsNullOrEmpty(NVTYPE))
            {
                MessageBox.Show("Votre champ est vide");
            }
            else
            {
                Connect_db ot = new Connect_db();
                string sqlqry = " SELECT COUNT(*) AS NOMBRE FROM  TBSCT.EPE_UUM_HYTORC_CLE WHERE TYPE_ID = (SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE ='" + Type_Modif.Text + "')";
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
                if (count == 1)
                {
                    sqlqry = " SELECT COUNT(*) AS NOMBRE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = '" + nouveau_type.Text + "'";
                    ot.Connect();
                    dr = ot.SelectData(sqlqry);
                    int count_1 = 0;
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            count_1 = Convert.ToInt32(dr["NOMBRE"]);
                        }
                    }
                    ot.Close();
                    if (count_1 == 0)
                    {
                        sqlqry = " UPDATE TBSCT.EPE_UUM_HYTORC_TYPE_CLE SET TYPE = '" + nouveau_type.Text + "'" +
                                 " WHERE TYPE ='" + Type_Modif.Text + "'";
                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry);
                        ot.Close();
                        MessageBox.Show("votre type de clé est modifié avec succés.");
                        nouveau_type.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                    }
                    else
                    {
                        sqlqry = " UPDATE TBSCT.EPE_UUM_HYTORC_CLE SET TYPE_ID = (SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = '" + nouveau_type.Text + "') WHERE NUMERO = '" + Num_GE_Modif.Text + "'";
                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry);
                        ot.Close();
                        MessageBox.Show("votre type de clé est modifié avec succés.");
                        if (MessageBox.Show("Ce type:" + Type_Modif.Text + " n'est associé à aucune clé, est ce que vous voulez le supprimer ", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            ot.Connect();
                            sqlqry = " DELETE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE ='" + Type_Modif.Text + "'";
                            ot.ExecuteNonQuery(sqlqry);
                            ot.Close();
                            MessageBox.Show("votre type " + Type_Modif.Text + " est supprimer avec succés.");
                            nouveau_type.Text = string.Empty;
                            Type_Modif.Text = string.Empty;
                            Num_GE_Modif.Items.Clear();
                            Alimenter_Num_GE();
                            Affichier_Cle();
                        }
                        else
                        {
                            nouveau_type.Text = string.Empty;
                            Type_Modif.Text = string.Empty;
                            Num_GE_Modif.Items.Clear();
                            Alimenter_Num_GE();
                            Affichier_Cle();
                            MessageBox.Show("La supression du type est annulée");
                        }
                    }
                }
                else
                {
                    sqlqry = " SELECT COUNT(*) AS NOMBRE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = '" + nouveau_type.Text + "'";
                    ot.Connect();
                    dr = ot.SelectData(sqlqry);
                    int count_1 = 0;
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            count_1 = Convert.ToInt32(dr["NOMBRE"]);
                        }
                    }
                    ot.Close();
                    if (count_1 != 0)
                    {
                        sqlqry = " UPDATE  TBSCT.EPE_UUM_HYTORC_CLE SET TYPE_ID = (SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = '" + nouveau_type.Text + "') WHERE NUMERO = '" + Num_GE_Modif.Text + "'";
                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry);
                        MessageBox.Show("votre type de clé est modifié avec succés.");
                        nouveau_type.Text = string.Empty;
                        Type_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                    }
                    else
                    {
                        sqlqry = " INSERT INTO TBSCT.EPE_UUM_HYTORC_TYPE_CLE(ID_TYPE,TYPE) VALUES(TBSCT.ID_TYPE.NEXTVAL,'" + nouveau_type.Text + "')";
                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry);

                        sqlqry = " UPDATE  TBSCT.EPE_UUM_HYTORC_CLE SET TYPE_ID = (SELECT ID_TYPE FROM TBSCT.EPE_UUM_HYTORC_TYPE_CLE WHERE TYPE = '" + nouveau_type.Text + "') WHERE NUMERO = '" + Num_GE_Modif.Text + "'";
                        ot.Connect();
                        ot.ExecuteNonQuery(sqlqry);
                        MessageBox.Show("votre type de clé est modifié avec succés.");
                        nouveau_type.Text = string.Empty;
                        Type_Modif.Text = string.Empty;
                        Num_GE_Modif.Items.Clear();
                        Alimenter_Num_GE();
                        Affichier_Cle();
                    }

                }
            }


        }

    }
}*/



