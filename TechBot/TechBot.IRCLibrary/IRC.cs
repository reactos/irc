using System;

namespace TechBot.IRCLibrary
{
	/// <summary>
	/// IRC constants and helper methods.
	/// </summary>
	public class IRC
	{
        private IRC()
        {
        }
		#region IRC commands

		public const string JOIN = "JOIN";
		public const string NICK = "NICK";
		public const string PART = "PART";
		public const string PING = "PING";
		public const string PONG = "PONG";
		public const string PRIVMSG = "PRIVMSG";
		public const string USER = "USER";
		public const string PASS = "PASS";
		public const string GHOST = "NICKSERV GHOST";
		public const string NOTICE = "NOTICE";

		public const string RPL_NAMREPLY = "353";
		public const string RPL_ENDOFNAMES = "366";
		public const string ERR_NICKNAMEINUSE = "433";

		#endregion
	}
}
