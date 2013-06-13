using System;

using TechBot.Library;

namespace TechBot.Commands.Common
{
	public abstract class BugCommand : Command
	{
		public BugCommand()
		{
		}

        public string BugID
        {
            get { return Parameters; }
            set { Parameters = value; }
        }

        public override void ExecuteCommand()
        {
            if (string.IsNullOrEmpty(BugID))
            {
                Say("Please provide a bug number.");
            }
            else
            {
                Say(BugUrl.ToString(), BugID);
            }
        }

        protected abstract Uri BugUrl { get; }
	}
}
