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
	internal partial class TournamentSeed : IScopable<Tournament>
	{
		public TournamentSeed () { }

		public long GroupSeedId { get; set; }
		public long GroupId { get; set; }
		public long? TeamId { get; set; }
		public int Seed { get; set; }

		public virtual TournamentGroup Group { get; set; }
		public virtual TournamentTeam Team { get; set; }

		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Group.Stage.Tournament;
	}
}
