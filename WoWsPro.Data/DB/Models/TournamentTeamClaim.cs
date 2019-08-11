using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class TournamentTeamClaim : IClaim<TournamentTeam>
	{
		public TournamentTeamClaim () { }

		public long TeamId { get; set; }
		public long AccountId { get; set; }
		public long ClaimId { get; set; }

		public virtual TournamentTeam Team { get; set; }
		public virtual Account Account { get; set; }
		public virtual Claim Claim { get; set; }

		[NotMapped]
		TournamentTeam IClaim<TournamentTeam>.Scope => Team;
		[NotMapped]
		public string Permission => Claim.Permission;

	}
}
