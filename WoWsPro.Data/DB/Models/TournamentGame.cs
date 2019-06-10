using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.ManageTournament, Scope = typeof(Tournament))]
	[Authorize(Actions.All, Permissions.AdministerTournaments)]
	internal partial class TournamentGame : IScopable<Tournament>
	{
		public TournamentGame () { }

		public long GameId { get; set; }
		public long MatchId { get; set; }
		public long? WinnerTeamId { get; set; }
		public bool Complete { get; set; }
		public long? MapId { get; set; }

		public virtual TournamentMatch Match { get; set; }
		public virtual TournamentTeam WinningTeam { get; set; }
		public virtual WarshipsMap Map { get; set; }

		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Match.Group.Stage.Tournament;
	}
}
