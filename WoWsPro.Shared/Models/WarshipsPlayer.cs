using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models
{
	public class WarshipsPlayer
	{
		public WarshipsPlayer() { }

		public long PlayerId { get; set; }
		public long? AccountId { get; set; }
		public Region Region { get; set; }
		public string Nickname { get; set; }
		public DateTime? Created { get; set; }
		public long? ClanId { get; set; }
		public string ClanRole { get; set; }
		public DateTime? JoinedClan { get; set; }
		public bool IsPrimary { get; set; }

		public Account Account { get; set; }

		// TODO: navigation properties
	}
}
