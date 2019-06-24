﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.Authorization.Scope;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.ManageTeam, Scope = typeof(TournamentTeam))]
	[Authorize(Actions.All, Permissions.ManageTournament, Scope = typeof(Tournament))]
	[Authorize(Actions.All, Permissions.AdministerTournaments)]
	internal partial class TournamentTeam : IScope<TournamentTeam>, IScopable<Tournament>, IScopable<TournamentTeam>
	{
		public TournamentTeam ()
		{
			Seeds = new HashSet<TournamentSeed>();
			AlphaMatches = new HashSet<TournamentMatch>();
			GamesWon = new HashSet<TournamentGame>();
			Participants = new HashSet<TournamentParticipant>();
		}

		public long TeamId { get; set; }
		public long TournamentId { get; set; }
		public string Name { get; set; }
		public string Tag { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public long OwnerAccountId { get; set; }

		[AuthorizeValues(Permissions.ManageTeam, 
			new object[] { TeamStatus.Unregistered, TeamStatus.Registered, TeamStatus.Withdrawn },
			Scope = typeof(TournamentTeam))]
		public TeamStatus Status { get; set; }
		public Region Region { get; set; }
		public DateTime Created { get; set; }

		public virtual Tournament Tournament { get; set; }
		public virtual Account Owner { get; set; }
		
		public virtual ICollection<TournamentSeed> Seeds { get; set; }
		public virtual ICollection<TournamentMatch> AlphaMatches { get; set; }
		public virtual ICollection<TournamentMatch> BravoMatches { get; set; }
		public virtual ICollection<TournamentGame> GamesWon { get; set; }
		public virtual ICollection<TournamentParticipant> Participants { get; set; }
		public virtual ICollection<TournamentTeamClaim> Claims { get; set; }

		[NotMapped]
		public ICollection<TournamentMatch> Matches => AlphaMatches.Union(BravoMatches).ToHashSet();


		[NotMapped]
		public long? ScopedId => TeamId;
		[NotMapped]
		public Type Scope => typeof(TournamentTeam);
		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Tournament;
		[NotMapped]
		IScope IScopable.ScopeInstance => this;
		[NotMapped]
		TournamentTeam IScopable<TournamentTeam>.ScopeInstance => this;
	}
}