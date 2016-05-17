using System;
using System.Xml;

using TechBot.Library;
using System.Runtime.InteropServices;
using System.Text;

namespace TechBot.Commands.Common
{
    [Command("winerror", Help = "!winerror <value>", Description="Get the name of the given System Error code")]
    public class WinErrorCommand : XmlLookupCommand
	{
        public WinErrorCommand()
		{
		}

        public override string XmlFile
        {
            get { return Settings.Default.WinErrorXml; }
        }

		public override void ExecuteCommand()
		{
            if (string.IsNullOrEmpty(Text))
            {
                Say("Please provide a valid System Error Code value.");
            }
            else
            {
                NumberParser np = new NumberParser();
                long winerror = np.Parse(Text);
                if (np.Error)
                {
                    Say("{0} is not a valid System Error Code value.", Text);
                    return;
                }

                string description = GetWinerrorDescription(winerror);
                if (description != null)
                {
                    Say("{0} is {1}.",
                        Text,
                        description);
                }
                else
                {
                    Say("I don't know about System Error Code {0}.", Text);
                }
            }
		}

		public string GetWinerrorDescription(long winerror)
		{
            // Try and get an english description of the error from the system messages.
            StringBuilder errorMsg = new StringBuilder(450); // longest IRC message = 512 bytes
            try
            {
                NativeMethods.FormatMessage(NativeMethods.FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, (uint)winerror, NativeMethods.EnglishCI, errorMsg, errorMsg.Capacity, null);
            }
            finally { }

            XmlElement root = base.m_XmlDocument.DocumentElement;
			XmlNode node = root.SelectSingleNode(String.Format("Winerror[@value='{0}']",
                                                               winerror.ToString()));
            if (node != null)
            {
                XmlAttribute text = node.Attributes["text"];
                if (text == null)
                    throw new Exception("Node has no text attribute.");
                if (errorMsg.Length > 0)
                    return text.Value + ": " + errorMsg.ToString();
                else
                    return text.Value;
            }
            else
                return null;
		}
	}
    static partial class NativeMethods
    {        
        [DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = true)]
        public static extern int FormatMessage(int dwFlags, IntPtr source, uint dwMessageId,
            int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);
        internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        internal static int EnglishCI = 0x409; // new CultureInfo("en-US").LCID :-)
    }
}
