using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Models;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Shared.Models.Discord
{
	public class DiscordGuild
	{
		public DiscordGuild () { }

		public long GuildId { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Invite { get; set; }

		public ICollection<DiscordRole> Roles { get; set; }
		public ICollection<Tournament> Tournaments { get; set; }
		
	}
}
