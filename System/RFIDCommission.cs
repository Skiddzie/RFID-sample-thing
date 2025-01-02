using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AscentSolutions.ZebraRfid
{
    public class RFIDCommission
    {
        // the service URL to this query
        private const string _ServiceURL = "/services/apexrest/PBSI/RFIDCommissionTag";

        private class RFIDCommissionTag
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("id")]
            public string ID { get; set; }

            [JsonProperty("locationid")]
            public string LocationID { get; set; }
        }

        private class RFIDCommissionTagRoot
        {
            public RFIDCommissionTag RFIDCommissionTag { get; set; }
        }

        public class RFIDCommissionResponse
        {
            [JsonProperty("epc")]
            public string EPC { get; set; }

            [JsonProperty("error")]
            public bool Error { get; set; }

            [JsonProperty("errorMsg")]
            public string ErrorMsg { get; set; }
        }

        // attempt to commission the RFID tag
        public static async Task<RFIDCommissionResponse> CommissionTag(AscentWebService ws, string strTagID, string strLocationID)
        {
            RFIDCommissionResponse rcr = null;

            // build the commissioning object to send
            // NOTE: If no location is specified (either a blank or null location),
            // we MUST send a NULL to the web service.  
            // Sending an blank string causes an error in the webservice.
            RFIDCommissionTag tag = new RFIDCommissionTag();
            tag.Type = "item";
            tag.ID = strTagID;
            tag.LocationID = (string.IsNullOrWhiteSpace(strLocationID)) ? null : strLocationID;  // pass a null if no location
            RFIDCommissionTagRoot tr = new RFIDCommissionTagRoot();
            tr.RFIDCommissionTag = tag;

            // authenticate and get the data from the web service
            rcr = await ws.PostURL<RFIDCommissionResponse>(_ServiceURL, tr);

            // log the commission
            WriteToCommissionLogEPC(true, rcr.EPC, rcr.Error, rcr.ErrorMsg);

            // done
            return rcr;
        }

        // the log file name
        private const string _LogFileName = "AscentCommission.Log";

        ///////////////////////////////////////////////////////////////
        // commission log utilities

        // for diagnostics, copy the log file to the SDCard
        public static string GetLogFilePath()
        {
            // the internal location
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), _LogFileName);
        }

        // log a preformatted Comission message to the log
        // specifically used by the Commission and Decommission calls
        public static void WriteToCommissionLogEPC(bool bCommission, string strEPC, bool bError, string strErrorMessage)
        {
            try
            {
                // format the message
                string strMessage = string.Format("{0}, EPC: {1}, Error: {2}", (bCommission ? "Commission" : "Decommission"), strEPC, bError);

                // write the message
                WriteToCommissionLogMultiLine(strMessage, strErrorMessage);
            }
            catch { }       // ignore exceptions while formatting
        }

        // write to log with formatting
        public static void WriteToCommissionLog(string format, params object[] args)
        {
            try
            {
                // format
                WriteToCommissionLogMultiLine(string.Format(format, args), "");
            }
            catch { }
        }

        // the method that actually writes to the log
        public static void WriteToCommissionLogMultiLine(params string[] args)
        {
            try
            {
                // build the file name
                string strLogFile = GetLogFilePath();

                try
                {
                    // check the file size for backup
                    FileInfo fi = new FileInfo(strLogFile);
                    if (fi.Length > 1000000)
                    {
                        // copy the old log
                        string strRename = Path.ChangeExtension(strLogFile, ".bak");
                        File.Delete(strRename);
                        File.Move(strLogFile, strRename);
                    }
                }
                catch { }       // ignore exceptions while formatting

                // open the file
                using (StreamWriter sw = new StreamWriter(strLogFile, true))
                {
                    // write the line
                    sw.WriteLine(DateTime.Now.ToString());

                    // write the lines
                    foreach (string strArg in args)
                    {
                        // write the line
                        sw.WriteLine(strArg);
                    }

                    // blank line after each entry
                    sw.WriteLine();
                }
            }
            catch { }       // ignore exceptions while formatting
        }
    }
}
