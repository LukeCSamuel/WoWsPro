using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Data.Attributes;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.Operations
{
	public partial class TeamOperations
	{
		[Public]
		public Shared.Models.TournamentTeam GetTeamById (long teamId) => Context.TournamentTeams.SingleOrDefault(t => t.TeamId == teamId) ?? throw new KeyNotFoundException();

		[Public]
		public Shared.Models.TournamentTeam GetJoinedTeam (long accountId, long tournamentId)
		{
			var account = Context.Accounts.SingleOrDefault(a => a.AccountId == accountId);
			if (account is null)
			{
				throw new KeyNotFoundException();
			}
			else
			{
				foreach (var player in account.WarshipsAccounts)
				{
					foreach (var participation in player.Participations)
					{
						if (participation.Team.TournamentId == tournamentId && participation.Status == ParticipantStatus.Accepted)
						{
							return participation.Team;
						}
					}
				}
				return null;
			}
		}

		[Public]
		public long CreateNewTeam (Shared.Models.TournamentTeam team)
		{
			// Verify that all rules are adhered to
			throw new NotImplementedException();
		}

		[Authorize(nameof(Permissions.ManageTeam))]
		public void UpdateTeam (Shared.Models.TournamentTeam team)
		{
			var context = Context.TournamentTeams.SingleOrDefault(t => t.TeamId == team.TeamId);
			if (context is null)
			{
				throw new KeyNotFoundException();
			}
			else
			{
				context.Tag = team.Tag is string && team.Tag.Length > 1 && team.Tag.Length < 6 ? team.Tag : context.Tag;
				context.Name = team.Name is string && team.Name.Length > 0 ? team.Name : context.Name;
				context.Icon = team.Icon;
				context.Description = team.Description;

				// FIXME: set status to pending?
				Context.SaveChanges();
			}
		}
	}
}
