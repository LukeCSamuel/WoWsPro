using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Warships;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class TournamentGame
	{
		public TournamentGame () { }

		public long GameId { get; set; }
		public long MatchId { get; set; }
		public long? WinnerTeamId { get; set; }
		public bool Complete { get; set; }
		public long? MapId { get; set; }

		public TournamentMatch Match { get; set; }
		public TournamentTeam WinningTeam { get; set; }
		public WarshipsMap Map { get; set; }
	}
}
