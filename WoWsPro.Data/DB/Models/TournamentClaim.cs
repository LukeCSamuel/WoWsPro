using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class TournamentClaim
	{

		public TournamentClaim() { }

		public long TournamentId { get; set; }
		public long AccountId { get; set; }
		public long ClaimId { get; set; }

		public virtual Tournament Tournament { get; set; }
		public virtual Account Account { get; set; }
		public virtual Claim Claim { get; set; }
	}
}
