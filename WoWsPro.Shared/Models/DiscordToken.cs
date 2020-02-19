using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Models
{
	public class DiscordToken
	{
		public DiscordToken () { }

		public long DiscordTokenId { get; set; }
		public long DiscordId { get; set; }
		public string AccessToken { get; set; }
		public string TokenType { get; set; }
		public DateTime Expires { get; set; }
		public string RefreshToken { get; set; }
		public string Scope { get; set; }

		public DiscordUser DiscordUser { get; set; }
	}
}
