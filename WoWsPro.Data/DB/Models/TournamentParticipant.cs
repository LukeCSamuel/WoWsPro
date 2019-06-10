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
	[Authorize(Actions.All, Permissions.Default, Scope = typeof(Account))]
	[Authorize(Actions.Immutable, Permissions.ManageParticipants, Scope = typeof(TournamentTeam))]
	[Authorize(Actions.All, Permissions.ManageParticipants, Scope = typeof(Tournament))]
	[Authorize(Actions.All, Permissions.AdministerParticipants)]
	internal partial class TournamentParticipant : IScopable<Tournament>, IScopable<TournamentTeam>, IScopable<Account>
	{
		public TournamentParticipant () { }

		public long ParticipantId { get; set; }
		public long TeamId { get; set; }
		public long PlayerId { get; set; }
		public ParticipantStatus Status { get; set; }

		public virtual TournamentTeam Team { get; set; }
		public virtual WarshipsPlayer Player { get; set; }

		[NotMapped]
		public long TournamentId => Team.TournamentId;
		[NotMapped]
		public long AccountId => Player.AccountId ?? -1;

		[NotMapped]
		Account IScopable<Account>.ScopeInstance => Player.Account;
		[NotMapped]
		IScope IScopable.ScopeInstance => Player.Account;
		[NotMapped]
		TournamentTeam IScopable<TournamentTeam>.ScopeInstance => Team;
		[NotMapped]
		Tournament IScopable<Tournament>.ScopeInstance => Team.Tournament;
	}
}
