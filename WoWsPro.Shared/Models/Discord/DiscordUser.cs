using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Models.Discord
{
	public class DiscordUser
	{
		public DiscordUser () { }

		public long DiscordId { get; set; }
		public long? AccountId { get; set; }
		public string Username { get; set; }
		public string Discriminator { get; set; }
		public string Avatar { get; set; }
		public bool IsPrimary { get; set; }

		public Account Account { get; set; }
		public ICollection<DiscordToken> DiscordTokens { get; set; }
	}
}
