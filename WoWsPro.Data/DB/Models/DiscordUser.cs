using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class DiscordUser
	{
		public DiscordUser () {
			DiscordTokens = new HashSet<DiscordToken>();
		}

		public long DiscordId { get; set; }
		public long? AccountId { get; set; }
		public string Username { get; set; }
		public string Discriminator { get; set; }
		public string Avatar { get; set; }
		public bool IsPrimary { get; set; }


		public virtual Account Account { get; set; }

		public virtual ICollection<DiscordToken> DiscordTokens { get; set; }
	}
}
