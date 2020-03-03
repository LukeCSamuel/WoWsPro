using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models
{
	public class Account
	{
		public Account () { }

		public long AccountId { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; set; }


		public ICollection<DiscordUser> DiscordAccounts { get; set; }
		public ICollection<WarshipsPlayer> WarshipsAccounts { get; set; }
		public ICollection<Tournament> OwnedTournaments { get; set; }
		public ICollection<TournamentTeam> OwnedTeams { get; set; }


		public DiscordUser GetPrimaryDiscordAccount () => DiscordAccounts.SingleOrDefault(e => e.IsPrimary);
		public WarshipsPlayer GetPrimaryWarshipsAccount () => WarshipsAccounts.SingleOrDefault(e => e.IsPrimary);

		public TournamentTeam GetAcceptedTeam (Tournament tournament)
		{
			foreach (var player in WarshipsAccounts)
			{
				foreach (var participation in player.Participations)
				{
					var team = participation.Team;
					if (team.TournamentId == tournament.TournamentId && participation.Status == ParticipantStatus.Accepted)
					{
						return team;
					}
				}
			}
			return null;
		}

		public IEnumerable<TournamentParticipant> GetInvitations (Tournament tournament)
		{
			foreach (var player in WarshipsAccounts)
			{
				foreach (var participant in player.Participations)
				{
					if (participant.Team.TournamentId == tournament.TournamentId
						&& participant.Status == ParticipantStatus.Invited 
						|| participant.Status == ParticipantStatus.Declined)
					{
						yield return participant;
					}
				}
			}
		}
	}
}
