using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.Authorization.Scope;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.ManageTournament, Scope = typeof(Tournament))]
	[Authorize(Actions.All, Permissions.AdministerTournaments)]
	internal partial class Tournament : IScope<Tournament>, IScopable<Tournament>
	{
		public Tournament()
		{
			Claims = new HashSet<TournamentClaim>();
			Stages = new HashSet<TournamentStage>();
			Teams = new HashSet<TournamentTeam>();
		}

		public long TournamentId { get; set; }
		public long? GuildId { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Description { get; set; }
		public long? OwnerAccountId { get; set; }
		public DateTime Created { get; set; }
		public int Capacity { get; set; }

		public virtual DiscordGuild Guild { get; set; }
		public virtual Account Owner { get; set; }

		public virtual ICollection<TournamentClaim> Claims { get; set; }
		public virtual ICollection<TournamentStage> Stages { get; set; }
		public virtual ICollection<TournamentTeam> Teams { get; set; }

		[NotMapped]
		public long? ScopedId => TournamentId;
		[NotMapped]
		public Type Scope => typeof(Tournament);
		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => this;
	}
}
