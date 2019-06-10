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
	internal partial class TournamentMatch : IScopable<Tournament>
	{
		public TournamentMatch ()
		{
			Games = new HashSet<TournamentGame>();
		}

		public long MatchId { get; set; }
		public long GroupId { get; set; }
		public int Sequence { get; set; }
		public long? AlphaTeamId { get; set; }
		public long? BravoTeamId { get; set; }
		public int MaxGames { get; set; }
		public bool Complete { get; set; }
		public DateTime StartTime { get; set; }

		public virtual TournamentGroup Group { get; set; }
		public virtual TournamentTeam AlphaTeam { get; set; }
		public virtual TournamentTeam BravoTeam { get; set; }

		public virtual ICollection<TournamentGame> Games { get; set; }

		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Group.Stage.Tournament;
	}
}
