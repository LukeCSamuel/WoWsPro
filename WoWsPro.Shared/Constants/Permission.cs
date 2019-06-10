using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Constants
{
	public static class Permissions
	{
		public const string AdministerApplicationSettings = nameof(AdministerApplicationSettings);
		public const string AdministerAccounts = nameof(AdministerAccounts);
		public const string AdministerClaims = nameof(AdministerClaims);
		public const string AdministerTournaments = nameof(AdministerTournaments);
		public const string AdministerParticipants = nameof(AdministerParticipants);
		public const string AdministerWGData = nameof(AdministerWGData);

		public const string ManageTournament = nameof(ManageTournament);
		public const string ManageTournamentClaims = nameof(ManageTournamentClaims);
		public const string ManageTeam = nameof(ManageTeam);
		public const string ManageTeamClaims = nameof(ManageTeamClaims);
		public const string ManageParticipants = nameof(ManageParticipants);

		public const string Default = nameof(Default);
	}
}
