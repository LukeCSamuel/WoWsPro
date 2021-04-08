using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Discord;
using WoWsPro.Shared.Models.Warships;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Shared.Models
{
	public class Account
	{
		public Account () { }

		public long AccountId { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; set; }


		public ICollection<AccountToken> AccountTokens { get; set; }
		public ICollection<Claim> Claims { get; set; }
		public ICollection<DiscordUser> DiscordAccounts { get; set; }
		public ICollection<WarshipsPlayer> WarshipsAccounts { get; set; }
		public ICollection<Tournament> OwnedTournaments { get; set; }
		public ICollection<TournamentTeam> OwnedTeams { get; set; }


		public DiscordUser GetPrimaryDiscordAccount () => DiscordAccounts.SingleOrDefault(e => e.IsPrimary);
		public WarshipsPlayer GetPrimaryWarshipsAccount () => WarshipsAccounts.SingleOrDefault(e => e.IsPrimary);

		public TournamentTeam GetAcceptedTeam (Tournament tournament)
		{
			// Return any team which is owned
			foreach (var team in OwnedTeams)
			{
				if (team.TournamentId == tournament.TournamentId)
				{
					return team;
				}
			}
			
			// Return any team which is accepted
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

		public Region GetBestRegion (IEnumerable<Region> choices = null) {
			if (choices is null) {
                choices = new Region[] { Region.NA, Region.EU, Region.CIS, Region.SEA };
            }

			if (WarshipsAccounts is not null && WarshipsAccounts.Count > 0)
			{
                return WarshipsAccounts.OrderByDescending(p => p.IsPrimary).ThenBy(p => p.Created).Select(p => p.Region).FirstOrDefault(region => choices.Contains(region));
            }

			return default;
		}
	}
}
