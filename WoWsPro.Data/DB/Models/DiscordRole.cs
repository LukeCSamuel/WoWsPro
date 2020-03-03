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

		public static implicit operator Shared.Models.DiscordRole (DiscordRole role) => role.ConvertObject<DiscordRole, Shared.Models.DiscordRole>();
		public static implicit operator DiscordRole (Shared.Models.DiscordRole role) => role.ConvertObject<Shared.Models.DiscordRole, DiscordRole>();
	}
}
