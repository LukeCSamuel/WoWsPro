using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.Authorization.Claim;
using WoWsPro.Data.Authorization.Scope;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.ManageTeamClaims, Scope = typeof(TournamentTeam))]
	[Authorize(Actions.All, Permissions.ManageTournamentClaims, Scope = typeof(Tournament))]
	[Authorize(Actions.All, Permissions.AdministerClaims)]
	internal partial class TournamentTeamClaim : IClaim<TournamentTeam>, IScopable<Tournament>, IScopable<TournamentTeam>
	{
		public TournamentTeamClaim () { }

		public long TeamId { get; set; }
		public long AccountId { get; set; }
		public long ClaimId { get; set; }

		public virtual TournamentTeam Team { get; set; }
		public virtual Account Account { get; set; }
		public virtual Claim Claim { get; set; }

		[NotMapped]
		public string Permission => Claim.Permission;
		[NotMapped]
		public long? ScopedId => TeamId;
		[NotMapped]
		public Type Scope => typeof(TournamentTeam);

		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Team.Tournament;
		[NotMapped]
		TournamentTeam IScopable<TournamentTeam>.ScopeInstance => Team;
		[NotMapped]
		IScope IScopable.ScopeInstance => Team;
	}
}
