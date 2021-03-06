using System;
using System.Collections.Generic;
using System.Text;

using TechBot.Library;

namespace TechBot.Commands.Common
{
    [Command("sambabug", Help = "!sambabug <number>", Description = "Will give you a link to the reqested Samba bug")]
    class SambaBugUrl : BugCommand
    {
        public SambaBugUrl()
        {
        }

        protected override Uri BugUrl
        {
            get { return new Uri("https://bugzilla.samba.org/show_bug.cgi?id={0}"); }
        }
    }
}
