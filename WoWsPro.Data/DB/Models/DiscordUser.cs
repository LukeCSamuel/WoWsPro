using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class DiscordUser 
	{
		public DiscordUser() { }

		public long DiscordId { get; set; }
		public long? AccountId { get; set; }
		public string Username { get; set; }
		public string Discriminator { get; set; }
		public string Avatar { get; set; }
		public bool IsPrimary { get; set; }


		public virtual Account Account { get; set; }


		public static implicit operator Shared.Models.DiscordUser (DiscordUser user) => new Shared.Models.DiscordUser()
		{
			DiscordId = user.DiscordId,
			AccountId = user.AccountId,
			Username = user.Username,
			Discriminator = user.Discriminator,
			Avatar = user.Avatar,
			IsPrimary = user.IsPrimary,
			Account = user.Account
		};

		public static implicit operator DiscordUser (Shared.Models.DiscordUser user) => new DiscordUser()
		{
			DiscordId = user.DiscordId,
			AccountId = user.AccountId,
			Username = user.Username,
			Discriminator = user.Discriminator,
			Avatar = user.Avatar,
			IsPrimary = user.IsPrimary,
			Account = user.Account
		};

	}
}
