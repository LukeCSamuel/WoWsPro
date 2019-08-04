using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class Claim
	{
		public Claim ()
		{
			AccountClaims = new HashSet<AccountClaim>();
			TournamentClaims = new HashSet<TournamentClaim>();
			TeamClaims = new HashSet<TournamentTeamClaim>();
		}

		public long ClaimId { get; set; }
		public string Permission { get; set; }

		public virtual ICollection<AccountClaim> AccountClaims { get; set; }
		public virtual ICollection<TournamentClaim> TournamentClaims { get; set; }
		public virtual ICollection<TournamentTeamClaim> TeamClaims { get; set; }
	}
}
