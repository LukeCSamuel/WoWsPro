using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class TournamentGroup
	{
		public TournamentGroup () { }

		public long GroupId { get; set; }
		public long TournamentStageId { get; set; }
		public string Name { get; set; }
		public int Capacity { get; set; }

		public TournamentStage Stage { get; set; }

		public ICollection<TournamentSeed> Seeds { get; set; }
		public ICollection<TournamentMatch> Matches { get; set; }
	}
}
