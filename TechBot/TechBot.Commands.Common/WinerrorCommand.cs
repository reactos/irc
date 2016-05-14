using System;
using System.Xml;

using TechBot.Library;

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
            Exception ex = null;
            // Get a text description for the error using host's error messages.
            try
            {
#warning FIXME : should display exceptions in English regardless of the host machine language.
                //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                ex = new System.ComponentModel.Win32Exception((int)winerror);
            }
            catch (Exception)
            { }

            XmlElement root = base.m_XmlDocument.DocumentElement;
			XmlNode node = root.SelectSingleNode(String.Format("Winerror[@value='{0}']",
                                                               winerror.ToString()));
            if (node != null)
            {
                XmlAttribute text = node.Attributes["text"];
                if (text == null)
                    throw new Exception("Node has no text attribute.");
                if (ex != null)
                    return text.Value + ": " + ex.Message;
                else
                    return text.Value;
            }
            else if (ex != null)
                return ex.Message;
            else
                return null;
		}
	}
}
