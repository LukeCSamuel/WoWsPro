using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Models
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
