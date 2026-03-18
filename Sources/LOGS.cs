using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUM_Hytroc
{
    internal class LOGS
    {
        // readonly Mail mail = new Mail();
        readonly string logP = @ConfigurationManager.AppSettings["logprobleme"]; //where the logs are save
        readonly string logT = @ConfigurationManager.AppSettings["logtrace"]; //where the logs are save
        readonly string logH = @ConfigurationManager.AppSettings["loghistorique"]; //where the logs are save

        private async Task logProbleme(string message, string Logdir)
        {
            using (StreamWriter sw = File.AppendText(Logdir))
            {
                await sw.WriteLineAsync(message + "  date:" + (DateTime.Now).ToString()); //write the message plus  the date 
                sw.Close();
            }
        }
        //save all the database request 
        private async Task logTrace(string message, string Logdir)
        {
            using (StreamWriter sw = File.AppendText(Logdir))
            {
                await sw.WriteLineAsync(message + "  date:" + (DateTime.Now).ToString()); //write the message plus  the date 
                sw.Close();
            }
        }
        //save code state
        private async Task logHistorique(string message, string Logdir)
        {
            using (StreamWriter sw = File.AppendText(Logdir))
            {
                await sw.WriteLineAsync(message + "  date:" + (DateTime.Now).ToString()); //write the message plus  the date 
                sw.Close();
            }
        }

        //verifie l'état des log avant écriture dans le fichier 
        public void writeLog(string message, string type, int sendemail)
        {
            if (type == "log")
            {
                if (ConfigurationManager.AppSettings["logproblemeState"] == "true")
                {
                    string Pwrite_directory = logP + "/" + type + DateTime.Now.ToString("dd-MM-yyyy") + ".log";
                    _ = logProbleme(message, Pwrite_directory);
                }

            }
            if (type == "trace")
            {
                if (ConfigurationManager.AppSettings["logtraceState"] == "true")
                {
                    string Twrite_directory = logT + "/" + type + DateTime.Now.ToString("dd-MM-yyyy") + ".log";
                    _ = logTrace(message, Twrite_directory);
                }

            }
            if (type == "historique")
            {
                if (ConfigurationManager.AppSettings["loghistoriqueState"] == "true")
                {
                    string Hwrite_directory = logH + "/" + type + DateTime.Now.ToString("dd-MM-yyyy") + ".log";
                    _ = logHistorique(message, Hwrite_directory);
                }

            }
        }
    
    }
}
