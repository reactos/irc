using System;
using System.Xml;
using System.Data;

using TechBot.Library;
using System.Net;
using System.IO;
using System.Globalization;

namespace TechBot.Commands.Common
{
    [Command("bc", Help = "!bc <value>", Description = "Get the symbolic name of the given bugcheck code")]
    public class BugcheckCommand : Command
    {
        NumberParser Parser;
        object Lock = new object();
        DataTable Bugs;

        const string MessageId = "MessageId";
        const string SymbolicName = "SymbolicName";
        const string Severity = "Severity";

        public BugcheckCommand()
        {
            Bugs = new DataTable();
            Bugs.Columns.Add(MessageId, typeof(ulong));
            Bugs.Columns.Add(SymbolicName, typeof(string));
            Parser = new NumberParser();
        }
        public override void ExecuteCommand()
        {
            ulong BugCode;
            if(!Parser.TryParse(Parameters, out BugCode))
            {
                Say("Please provide a valid BUGCHECK value.");
            }
            else if (Bugs.Rows.Count == 0)
            {
                Say("KebugCheckEx( PHASE0_INITIALIZATION_FAILED, NO_PAGES_AVAILABLE, MORAL_EXCEPTION_ERROR, DATA_COHERENCY_EXCEPTION );");
            }
            else
            {
                DataRow[] Found = Bugs.Select(MessageId+"=" + BugCode);
                if (Found.Length == 1)
                    //Say(Parameters + " is " + Found[0]["SymbolicName"].ToString());
                    Say("KeBugCheck( " + Found[0]["SymbolicName"].ToString() + " );");
                else
                    Say("KeBugCheck( " + Parameters + " ) not found.");
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            lock (Lock)
            {
                if (Bugs.Rows.Count == 0)
                    FetchCodes();
            }
        }
        /// <summary>
        /// Download bug codes from reactos server and parse it into a DataTable.
        /// </summary>
        private void FetchCodes()
        {
            WebClient Client = new WebClient();
            string FileName;
            try
            {
                FileName = Path.GetTempFileName();
                Client.DownloadFile(Settings.Default.Bugcodes, FileName);
            }
            catch (IOException)
            {
                return;
            }
            using (StreamReader sr = File.OpenText(FileName))
            {
                string s = "";
                string[]words;
                while ((s = sr.ReadLine()) != null)
                {
                    words = s.Split('=');
                    switch (words[0])
                    {
                        case MessageId:
                            DataRow Row = Bugs.NewRow();                            
                            ulong Code = 0;
                            if (!Parser.TryParse(words[1], out Code))
                                continue;
                            Row[MessageId] = Code;
                            while ((s = sr.ReadLine()) != null)
                            {
                                words = s.Split('=');
                                if (words[0] == Severity && words[1] != "Success")
                                    break;
                                if (words[0] == ".")
                                {
                                    Bugs.Rows.Add(Row);
                                    break;
                                }
                                if (words[0] == SymbolicName)
                                    Row[SymbolicName] = words[1];
                            }
                            break;
                    }
                }
            }
        }
    }
}