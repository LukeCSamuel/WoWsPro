using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.DB.Models
{
	internal partial class DiscordRole
	{
		public DiscordRole() { }

		public long RoleKeyId { get; set; }
		public long RoleId { get; set; }
		public long GuildId { get; set; }
		public string Name { get; set; }

		public virtual DiscordGuild Guild { get; set; }
	}
}
