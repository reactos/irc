using System;
using System.Collections.Generic;
using System.Text;

using TechBot.Library;

namespace TechBot.Commands.Common
{
    [Command("winebug", Help = "!winebug <number>" , Description="Will give you a link to the reqested Wine bug")]
    class WineBugUrl : BugCommand
    {
        public WineBugUrl()
        {
        }

        protected override Uri BugUrl
        {
            get { return new Uri("http://bugs.winehq.org/show_bug.cgi?id={0}"); }
        }
    }
}
