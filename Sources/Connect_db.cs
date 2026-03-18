using Oracle.ManagedDataAccess.Client;
using Org.BouncyCastle.Utilities;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace UUM_Hytroc
{
    /// <summary>
    /// Centralise l'accès Oracle pour l'application.
    /// Cette classe conserve l'API existante du projet (Connect, Close, SelectData, Request, ExecuteNonQuery)
    /// afin d'éviter de casser les autres classes.
    /// </summary>
    public class Connect_db : IDisposable
    {
        readonly LOGS log = new LOGS();
        readonly Password pass = new Password();

        // Connexion non statique : chaque instance gère sa propre connexion,
        // ce qui évite de fermer un DataReader utilisé ailleurs.
        private OracleConnection con;

        private OracleCommand cmd;
        private OracleDataReader reader;

        private static string DB_ROOT;
        private static string chaineConnexion;
        private static string environnement;

        private string password;

        public void setDB_root(string db_root)
        {
            DB_ROOT = db_root;
        }

        public string getDB_root()
        {
            return DB_ROOT;
        }

        /// <summary>
        /// Lit le fichier de configuration DB sécurisé et construit la chaîne de connexion Oracle.
        /// </summary>
        public void readDBfile()
        {
            string uid;
            string host_name;
            string port;
            string service_name;

            string db_path = getDB_root();
            db_path += "/connect_securedb.db";

            try
            {
                if (File.Exists(db_path))
                {
                    foreach (string line in File.ReadLines(db_path))
                    {
                        if (line.StartsWith("pwd"))
                        {
                            password = line.Trim();
                            password = line.Remove(0, 4);
                            password = pass.Decrypt(password);
                        }
                    }

                    var data = File
                        .ReadAllLines(db_path)
                        .Select(x => x.Split('='))
                        .Where(x => x.Length > 1)
                        .ToDictionary(x => x[0].Trim(), x => x[1]);

                    uid = data["uid"];
                    host_name = data["host_name"];
                    port = data["port"];
                    service_name = data["service_name"];
                    environnement = data["environnement"];

                    chaineConnexion = $"Data Source={host_name}:{port}/{service_name}; User Id={uid}; password={password}";
                }
                else
                {
                    MessageBox.Show("Fichier de configuration base de données introuvable : " + db_path);
                }
            }
            catch (Exception ex)
            {
                log.writeLog(ex.ToString(), "log", 1);
                MessageBox.Show(ex.Message);
            }
        }

        public string getEnv()
        {
            return environnement;
        }

        /// <summary>
        /// Ouvre la connexion Oracle si elle n'est pas déjà ouverte.
        /// </summary>
        public void Connect()
        {
            try
            {
                if (con == null)
                {
                    con = new OracleConnection();
                    con.ConnectionString = chaineConnexion;
                }

                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
            }
            catch (Exception ex)
            {
                log.writeLog(ex.ToString(), "log", 1);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Ferme proprement la connexion Oracle si elle existe.
        /// </summary>
        public void Close()
        {
            try
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }

                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }

                if (con != null)
                {
                    if (con.State != ConnectionState.Closed)
                        con.Close();

                    con.Dispose();
                    con = null;
                }
            }
            catch (Exception ex)
            {
                log.writeLog("Erreur fermeture connexion : " + ex, "log", 1);
            }
        }

        /// <summary>
        /// Legacy SELECT : conservé pour compatibilité avec le code existant.
        /// A éviter pour les entrées utilisateur non fiables.
        /// </summary>
        public OracleDataReader SelectData(string query)
        {
            try
            {
                if (con == null || con.State != ConnectionState.Open)
                    Connect();

                cmd = con.CreateCommand();
                cmd.CommandText = query;
                cmd.BindByName = true;

                reader = cmd.ExecuteReader();
                log.writeLog(query, "trace", 0);
            }
            catch (Exception ex)
            {
                log.writeLog(query, "trace", 0);
                log.writeLog(ex.ToString(), "log", 1);
                MessageBox.Show(ex.Message, "Error");
            }

            return reader;
        }

        /// <summary>
        /// SELECT paramétré.
        /// A privilégier dès qu'une valeur vient de l'utilisateur ou de l'interface.
        /// </summary>
        public OracleDataReader Request(string query, params OracleParameter[] parameters)
        {
            try
            {
                if (con == null || con.State != ConnectionState.Open)
                    Connect();

                cmd = con.CreateCommand();
                cmd.CommandText = query;
                cmd.BindByName = true;

                if (parameters != null)
                {
                    foreach (OracleParameter p in parameters)
                    {
                        if (p.Value == null)
                            p.Value = DBNull.Value;

                        cmd.Parameters.Add(p);
                    }
                }

                reader = cmd.ExecuteReader();
                log.writeLog(query, "trace", 0);
            }
            catch (Exception ex)
            {
                log.writeLog(query, "trace", 0);
                log.writeLog($"Erreur exécution d'une requete {ex.Message} {ex.Source}", "log", 1);
                MessageBox.Show(ex.Message, "Error");
            }

            return reader;
        }

        /// <summary>
        /// INSERT / UPDATE / DELETE paramétrés.
        /// Retourne le nombre de lignes affectées.
        /// </summary>
        public int ExecuteNonQuery(string query, params OracleParameter[] parameters)
        {
            int nb = 0;

            try
            {
                if (con == null || con.State != ConnectionState.Open)
                    Connect();

                cmd = con.CreateCommand();
                cmd.CommandText = query;
                cmd.BindByName = true;

                if (parameters != null)
                {
                    foreach (OracleParameter p in parameters)
                    {
                        if (p.Value == null)
                            p.Value = DBNull.Value;

                        cmd.Parameters.Add(p);
                    }
                }

                nb = cmd.ExecuteNonQuery();
                log.writeLog(query, "trace", 0);
            }
            catch (Exception ex)
            {
                log.writeLog(query, "trace", 0);
                log.writeLog($"Erreur ExecuteNonQuery : {ex.Message} {ex.Source}", "log", 1);
                MessageBox.Show(ex.Message, "Error");
            }

            return nb;
        }

        /// <summary>
        /// Utilitaire pour les COUNT(*) et autres requêtes scalaires entières.
        /// </summary>
        public int ExecuteScalarInt(string query, params OracleParameter[] parameters)
        {
            int resultValue = 0;

            try
            {
                if (con == null || con.State != ConnectionState.Open)
                    Connect();

                cmd = con.CreateCommand();
                cmd.CommandText = query;
                cmd.BindByName = true;

                if (parameters != null)
                {
                    foreach (OracleParameter p in parameters)
                    {
                        if (p.Value == null)
                            p.Value = DBNull.Value;

                        cmd.Parameters.Add(p);
                    }
                }

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    resultValue = Convert.ToInt32(result);

                log.writeLog(query, "trace", 0);
            }
            catch (Exception ex)
            {
                log.writeLog(query, "trace", 0);
                log.writeLog($"Erreur ExecuteScalarInt : {ex.Message} {ex.Source}", "log", 1);
                MessageBox.Show(ex.Message, "Error");
            }

            return resultValue;
        }

        public void Dispose()
        {
            Close();
        }
    }
    /* // Centralise l'accès Oracle (connexion + exécution de requêtes).
     // Note: le projet legacy utilise encore SelectData(string). La version paramétrée doit être privilégiée.
     internal class Connect_db
     {
         private OracleConnection con;
         private OracleCommand cmd;
         private OracleDataReader reader;

         private static string DB_ROOT;
         private static string chaineConnexion;
         private static string environnement;

         readonly Password pass = new Password();
         string password;

         public void setDB_root(string db_root) => DB_ROOT = db_root;
         public string getDB_root() => DB_ROOT;
         public string getEnv() => environnement;

         public void readDBfile()
         {
             string uid;
             string host_name;
             string port;
             string service_name;

             string db_path = getDB_root();
             db_path += "/connect_securedb.db";

             try
             {
                 if (!File.Exists(db_path)) return;

                 foreach (string line in File.ReadLines(db_path))
                 {
                     if (line.StartsWith("pwd"))
                     {
                         password = line.Trim();
                         password = line.Remove(0, 4);
                         password = pass.Decrypt(password);
                     }
                 }

                 var data = File.ReadAllLines(db_path)
                     .Select(x => x.Split('='))
                     .Where(x => x.Length > 1)
                     .ToDictionary(x => x[0].Trim(), x => x[1]);

                 uid = data["uid"];
                 host_name = data["host_name"];
                 port = data["port"];
                 service_name = data["service_name"];
                 environnement = data["environnement"];

                 chaineConnexion = $"Data Source={host_name}:{port}/{service_name}; User Id={uid}; password={password}";
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message);
             }
         }

         public void Connect()
         {
             try
             {
                 if (con == null)
                     con = new OracleConnection();

                 if (con.State != ConnectionState.Open)
                 {
                     con.ConnectionString = chaineConnexion;
                     con.Open();
                 }
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message);
             }
         }

         public void Close()
         {
             try
             {
                 if (con != null)
                 {
                     if (con.State != ConnectionState.Closed)
                         con.Close();

                     con.Dispose();
                     con = null;
                 }
             }
             catch
             {
                 // On évite de crasher sur Close()
             }
         }

         // Legacy: SELECT non paramétré (à éviter pour les entrées utilisateur). Conservé pour compatibilité.
         public OracleDataReader SelectData(string query)
         {
             try
             {
                 // Important: on ne Connect() pas ici, car ton code le fait déjà.
                 // Mais si quelqu'un oublie, on sécurise :
                 if (con == null || con.State != ConnectionState.Open)
                     Connect();

                 cmd = con.CreateCommand();
                 cmd.CommandText = query;

                 reader = cmd.ExecuteReader();
                 return reader;
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "Error");
                 return null;
             }
         }

         // SELECT paramétré (anti SQL injection). Utiliser cette méthode dès qu'une valeur vient de l'utilisateur/UI.
         public OracleDataReader SelectData(string query, Dictionary<string, object> parameters)
         {
             try
             {
                 if (con == null || con.State != ConnectionState.Open)
                     Connect();

                 cmd = con.CreateCommand();
                 cmd.CommandText = query;
                 cmd.Parameters.Clear();

                 if (parameters != null)
                 {
                     foreach (var p in parameters)
                         cmd.Parameters.Add(new OracleParameter(p.Key, p.Value ?? DBNull.Value));
                 }

                 reader = cmd.ExecuteReader();
                 return reader;
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "Error");
                 return null;
             }
         }

         // INSERT/UPDATE/DELETE. Retourne le nombre de lignes affectées.
         public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
         {
             try
             {
                 if (con == null || con.State != ConnectionState.Open)
                     Connect();

                 using (var localCmd = con.CreateCommand())
                 {
                     localCmd.CommandText = query;
                     localCmd.Parameters.Clear();

                     if (parameters != null)
                     {
                         foreach (var p in parameters)
                             localCmd.Parameters.Add(new OracleParameter(p.Key, p.Value ?? DBNull.Value));
                     }

                     return localCmd.ExecuteNonQuery();
                 }
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "Error");
                 return -1;
             }
         }

         // Utilitaire pour SELECT COUNT(*) et valeurs scalaires entières.
         public int ExecuteScalarInt(string query, Dictionary<string, object> parameters = null)
         {
             try
             {
                 if (con == null || con.State != ConnectionState.Open)
                     Connect();

                 using (var localCmd = con.CreateCommand())
                 {
                     localCmd.CommandText = query;
                     localCmd.Parameters.Clear();

                     if (parameters != null)
                     {
                         foreach (var p in parameters)
                             localCmd.Parameters.Add(new OracleParameter(p.Key, p.Value ?? DBNull.Value));
                     }

                     object result = localCmd.ExecuteScalar();
                     return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
                 }
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "Error");
                 return 0;
             }
         }
     }
    */
}