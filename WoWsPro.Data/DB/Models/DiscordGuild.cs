using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class DiscordGuild
	{
		public DiscordGuild ()
		{
			Tournaments = new HashSet<Tournament>();
		}

		public long GuildId { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Invite { get; set; }

		public virtual ICollection<DiscordRole> Roles { get; set; }
		public virtual ICollection<Tournament> Tournaments { get; set; }
	}
}
