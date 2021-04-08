using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class TournamentMatch
	{
		public TournamentMatch () { }

		public long MatchId { get; set; }
		public long GroupId { get; set; }
		public int Sequence { get; set; }
		public long? AlphaTeamId { get; set; }
		public long? BravoTeamId { get; set; }
		public int MaxGames { get; set; }
		public bool Complete { get; set; }
		public DateTime StartTime { get; set; }

		public TournamentGroup Group { get; set; }
		public TournamentTeam AlphaTeam { get; set; }
		public TournamentTeam BravoTeam { get; set; }

		public ICollection<TournamentGame> Games { get; set; }
	}
}
