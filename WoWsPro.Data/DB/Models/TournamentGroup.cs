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
	internal partial class TournamentGroup : IScopable<Tournament>
	{
		public TournamentGroup ()
		{
			Seeds = new HashSet<TournamentSeed>();
			Matches = new HashSet<TournamentMatch>();
		}

		public long GroupId { get; set; }
		public long TournamentStageId { get; set; }
		public string Name { get; set; }
		public int Capacity { get; set; }

		public virtual TournamentStage Stage { get; set; }

		public virtual ICollection<TournamentSeed> Seeds { get; set; }
		public virtual ICollection<TournamentMatch> Matches { get; set; }

		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Stage.Tournament;
	}
}
