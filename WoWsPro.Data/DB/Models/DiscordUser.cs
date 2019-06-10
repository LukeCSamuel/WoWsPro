using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.Default, Scope = typeof(Account))]
	[Authorize(Actions.All, Permissions.AdministerAccounts)]
	internal partial class DiscordUser : IScopable<Account>
	{
		public DiscordUser() { }

		public long DiscordId { get; set; }
		public long? AccountId { get; set; }
		public string Username { get; set; }
		public string Discriminator { get; set; }
		public string Avatar { get; set; }
		public bool IsPrimary { get; set; }


		public virtual Account Account { get; set; }

		[NotMapped]
		Account IScopable<Account>.ScopeInstance => Account;
	}
}
