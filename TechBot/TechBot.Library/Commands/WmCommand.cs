using System;
using System.Xml;

namespace TechBot.Library
{
    [Command("wm" , Help = "!wm <value> or !wm <name>")]
	public class WMCommand : XmlCommand
	{
        private string m_WMText = null;

        public WMCommand()
		{
		}

        public override string XmlFile
        {
            get { return Settings.Default.WMXml; }
        }
		
        [CommandParameter("wm", "The windows message to check")]
        public string WMText
        {
            get { return m_WMText; }
            set { m_WMText = value; }
        }

		public override void ExecuteCommand()
		{
            if (WMText.Equals(String.Empty))
			{
				TechBot.ServiceOutput.WriteLine(Context,
				                        "Please provide a valid window message value or name.");
				return;
			}

			NumberParser np = new NumberParser();
            long wm = np.Parse(WMText);
			string output;
			if (np.Error)
			{
				// Assume "!wm <name>" form.
                output = GetWmNumber(WMText);
			}
			else
			{
				output = GetWmDescription(wm);
			}

			if (output != null)
			{
                TechBot.ServiceOutput.WriteLine(Context,
				                        String.Format("{0} is {1}.",
                                                      WMText,
			        	                              output));
			}
			else
			{
                TechBot.ServiceOutput.WriteLine(Context,
				                        String.Format("I don't know about window message {0}.",
                                                      WMText));
			}
		}

		private string GetWmDescription(long wm)
		{
			XmlElement root = base.m_XmlDocument.DocumentElement;
			XmlNode node = root.SelectSingleNode(String.Format("WindowMessage[@value='{0}']",
			                                                   wm));
			if (node != null)
			{
				XmlAttribute text = node.Attributes["text"];
				if (text == null)
					throw new Exception("Node has no text attribute.");
				return text.Value;
			}
			else
				return null;
		}
		
		private string GetWmNumber(string wmName)
		{
			XmlElement root = base.m_XmlDocument.DocumentElement;
			XmlNode node = root.SelectSingleNode(String.Format("WindowMessage[@text='{0}']",
			                                                   wmName));
			if (node != null)
			{
				XmlAttribute value = node.Attributes["value"];
				if (value == null)
					throw new Exception("Node has no value attribute.");
				return value.Value;
			}
			else
				return null;
		}
	}
}