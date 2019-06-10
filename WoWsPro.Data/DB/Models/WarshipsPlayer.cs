using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.Authorization.Scope;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.Default, Scope = typeof(Account))]
	[Authorize(Actions.All, Permissions.AdministerAccounts)]
	internal partial class WarshipsPlayer : IScopable<Account>
	{
		public WarshipsPlayer ()
		{
			Participations = new HashSet<TournamentParticipant>();
		}

		public long PlayerId { get; set; }
		public long? AccountId { get; set; }
		public Region Region { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; set; }
		public long ClanId { get; set; }
		public string ClanRole { get; set; }
		public DateTime JoinedClan { get; set; }
		public bool IsPrimary { get; set; }

		public virtual Account Account { get; set; }
		public virtual WarshipsClan Clan { get; set; }

		public virtual ICollection<TournamentParticipant> Participations { get; set; }

		[NotMapped]
		Account IScopable<Account>.ScopeInstance => Account;
	}
}
