using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class TournamentSeed
	{
		public TournamentSeed () { }

		public long GroupSeedId { get; set; }
		public long GroupId { get; set; }
		public long? TeamId { get; set; }
		public int Seed { get; set; }

		public virtual TournamentGroup Group { get; set; }
		public virtual TournamentTeam Team { get; set; }

	}
}
