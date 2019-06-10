using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.Authorization.Claim;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.ManageTournamentClaims, Scope = typeof(Tournament))]
	[Authorize(Actions.All, Permissions.AdministerClaims)]
	internal partial class TournamentClaim : IClaim<Tournament>, IScopable<Tournament>
	{

		public TournamentClaim() { }

		public long TournamentId { get; set; }
		public long AccountId { get; set; }
		public long ClaimId { get; set; }

		public virtual Tournament Tournament { get; set; }
		public virtual Account Account { get; set; }
		public virtual Claim Claim { get; set; }

		[NotMapped]
		public string Permission => Claim.Permission;
		[NotMapped]
		public long? ScopedId => TournamentId;
		[NotMapped]
		public Type Scope => typeof(Tournament);

		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Tournament;
	}
}
