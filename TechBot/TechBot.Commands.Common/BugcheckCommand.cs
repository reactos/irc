using System;
using System.Xml;

using TechBot.Library;

namespace TechBot.Commands.Common
{
	[Command("bc", Help = "!bc <value>", Description = "Get the name of the given bugcheck code")]
	public class BugcheckCommand : Command
	{
		public BugcheckCommand ()
		{
		}
		public override void ExecuteCommand()
		{
            if (string.IsNullOrEmpty(Parameters))
            {
                Say("Please provide a valid BUGCHECK value.");
            }
            else
            {
                Say("KebugCheckEx( PHASE0_INITIALIZATION_FAILED, NO_PAGES_AVAILABLE, MORAL_EXCEPTION_ERROR, DATA_COHERENCY_EXCEPTION );");
			}
		}
	}
}

