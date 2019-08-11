using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class TournamentClaim : IClaim<Tournament>
	{

		public TournamentClaim() { }

		public long TournamentId { get; set; }
		public long AccountId { get; set; }
		public long ClaimId { get; set; }

		public virtual Tournament Tournament { get; set; }
		public virtual Account Account { get; set; }
		public virtual Claim Claim { get; set; }

		[NotMapped]
		Tournament IClaim<Tournament>.Scope => Tournament;
		[NotMapped]
		public string Permission => Claim.Permission;
	}
}
