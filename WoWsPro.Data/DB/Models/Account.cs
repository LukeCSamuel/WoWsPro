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
	[Authorize(Actions.Permanent, Permissions.Default, Scope = typeof(Account))]
	[Authorize(Actions.All, Permissions.AdministerAccounts)]
	internal partial class Account : IScope<Account>, IScopable<Account>
	{
		public Account()
		{
			DiscordAccounts = new HashSet<DiscordUser>();
			WarshipsAccounts = new HashSet<WarshipsPlayer>();
			OwnedTournamnets = new HashSet<Tournament>();
			AccountClaims = new HashSet<AccountClaim>();
			TournamentClaims = new HashSet<TournamentClaim>();
			OwnedTeams = new HashSet<TournamentTeam>();
		}

		public long AccountId { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; set; }


		public virtual ICollection<DiscordUser> DiscordAccounts { get; set; }
		public virtual ICollection<WarshipsPlayer> WarshipsAccounts { get; set; }
		public virtual ICollection<Tournament> OwnedTournamnets { get; set; }
		public virtual ICollection<AccountClaim> AccountClaims { get; set; }
		public virtual ICollection<TournamentClaim> TournamentClaims { get; set; }
		public virtual ICollection<TournamentTeamClaim> TeamClaims { get; set; }
		public virtual ICollection<TournamentTeam> OwnedTeams { get; set; }

		[NotMapped]
		public long? ScopedId => AccountId;
		[NotMapped]
		public Type Scope => typeof(Account);
		[NotMapped]
		Account IScopable<Account>.ScopeInstance => this;
	}
}
