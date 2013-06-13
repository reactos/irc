using System;
using System.Collections.Generic;
using System.Text;

using TechBot.Library;

namespace TechBot.Commands.Common
{
    [Command("jira", Help = "!jira <number>", Description = "Will give you a link to the requested ReactOS bug")]
    class ReactOSBugUrl : BugCommand
    {
        public ReactOSBugUrl()
        {
        }

        protected override Uri BugUrl
        {
            get { return new Uri("http://jira.reactos.org/browse/{0}"); }
        }
    }
}
