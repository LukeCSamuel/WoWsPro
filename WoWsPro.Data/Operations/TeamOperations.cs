using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Discord;
using WoWsPro.Shared.Models.Tournaments;
using WoWsPro.Shared.Permissions;
using WoWsPro.Shared.Permissions.Claims.Team;
using WoWsPro.Shared.Validation;

namespace WoWsPro.Data.Operations
{
	public class TeamOperations : Operations
	{
		static EditTeamInfo EditTeamInfo { get; } = new EditTeamInfo();
		static EditTeamRoster EditTeamRoster { get; } = new EditTeamRoster();

		protected IWarshipsApi WarshipsApi { get; }

        public TeamOperations(Context context, IAuthorizer authorizer, IWarshipsApi warshipsApi) : base(context, authorizer) {
			WarshipsApi = warshipsApi;
		}

        /// <summary>
		/// Look up a specific team by Id
		/// </summary>
		/// <param name="teamId">Team Id</param>
        public async Task<TournamentTeam> GetTeamByIdAsync (long teamId)
		{
			return await Context.TournamentTeams
				.Where(t => t.TeamId == teamId)
				.Include(t => t.Participants).ThenInclude(p => p.Player.Account.DiscordAccounts)
				.Include(t => t.Tournament).ThenInclude(t => t.RegistrationRules).ThenInclude(t => t.RegistrationQuestions)
				.Include(t => t.QuestionResponses)
				.SingleOrDefaultAsync() ?? throw new KeyNotFoundException();
		}

		/// <summary>
		/// Gets the team that a particular player has joined
		/// </summary>
		/// <param name="playerId">Player Id</param>
		/// <param name="tournamentId">Tournament Id</param>
		public async Task<TournamentTeam> GetJoinedTeamAsync (long playerId, long tournamentId)
		{
			var teamId = await Context.TournamentParticipants
				.Include(p => p.Team)
				.Where(p => p.PlayerId == playerId && p.Team.TournamentId == tournamentId && p.Status == ParticipantStatus.Accepted)
				.Select(p => p.TeamId)
				.SingleOrDefaultAsync();

			return await GetTeamByIdAsync(teamId);
		}

		/// <summary>
		/// Get a list of teams which have invited the user
		/// </summary>
		/// <param name="tournamentId">Tournament Id</param>
		public Task<List<TournamentParticipant>> GetUserInvitesAsync (long tournamentId)
		{
			return Context.TournamentParticipants
				.Include(p => p.Team)
				.Where(p => p.Team.TournamentId == tournamentId)
				.ToListAsync()
				;
		}

		/// <summary>
		/// Get all teams that the user is authorized for EditTeamInfo or EditTeamRoster
		/// </summary>
		/// <param name="tournamentId">Tournament Id</param>
		public async Task<List<TournamentTeam>> GetManagedTeamsAsync (long tournamentId)
		{
			return (await Context.TournamentTeams
				.Where(team => team.TournamentId == tournamentId)
				.ToListAsync())
				.Where(team => Authorizer.HasClaim(EditTeamInfo, team) || Authorizer.HasClaim(EditTeamRoster, team))
				.ToList()
				;
		}

		/// <summary>
		/// Creates a tournament team
		/// </summary>
		/// <param name="team">The team that should be created</param>
		public async Task CreateTeamAsync (TournamentTeam team)
		{
			// Normalize team
			team.Tag = team.Tag.ToUpper();

			// Validate that registration rules are adhered to
			var rules = await Context.TournamentRegistrationRules
				.Include(r => r.Tournament).ThenInclude(t => t.Teams)
				.Where(r => r.TournamentId == team.TournamentId && r.Region == team.Region)
				.SingleOrDefaultAsync();
			var validator = new TournamentRegistrationRulesValidator(rules);

			validator.ValidateTeam(team, rules.Tournament.Teams);
			await SetWoWsPlayersAsync(team.Region, team.Participants);
			await validator.ValidateDiscordAsync(team, getDiscordUser);

			// Determine the team's status.  For the time being, we accept any valid team that isn't waitlisted
			var teamCount = await Context.TournamentTeams
				.Where(t => t.TournamentId == team.TournamentId && t.Region == team.Region)
				.CountAsync();
			if (teamCount < rules.Capacity)
			{
				team.Status = TeamStatus.Registered;
			}
			else
			{
				team.Status = TeamStatus.WaitListed;
			}
			team.Created = DateTime.UtcNow;

			await InitializeParticipantsAsync(rules, team);

			// Create the team
			Context.TournamentTeams.Add(team);
			await Context.SaveChangesAsync();
		}

		/// <summary>
		/// Edit the generic information about a team
		/// </summary>
		/// <param name="team">The new values for the team</param>
		public async Task EditTeamInfoAsync (TournamentTeam team)
		{
			// Normalize team
			team.Tag = team.Tag.ToUpper();

			// TODO don't use find??
			var existing = await Context.TournamentTeams
				.Where(t => t.TeamId == team.TeamId)
				.AsTracking()
				.SingleOrDefaultAsync();
			if (existing is null)
			{
				throw new KeyNotFoundException();
			}
			if (!Authorizer.HasClaim(EditTeamInfo, existing))
			{
				throw new UnauthorizedAccessException();
			}

			// Validate that the registration rules are adhered to
			var rules = await Context.TournamentRegistrationRules
				.Include(r => r.Tournament).ThenInclude(t => t.Teams)
				.Where(r => r.TournamentId == team.TournamentId && r.Region == team.Region)
				.SingleOrDefaultAsync();
			var validator = new TournamentRegistrationRulesValidator(rules);

			if (!validator.CanEditTeamInfo())
			{
				throw new ArgumentException("Team Info can no longer be adjusted.");
			}
			validator.ValidateTeam(team, rules.Tournament.Teams.Where(t => t.TeamId != team.TeamId));

			// Update properties on existing team
			existing.Name = team.Name;
			existing.Tag = team.Tag;
			existing.Description = team.Description;
			existing.Icon = team.Icon;
			existing.QuestionResponses = team.QuestionResponses;

			await Context.SaveChangesAsync();
		}

		/// <summary>
		/// Edit the roster of a team
		/// </summary>
		/// <param name="team">The new values for the team</param>
		public async Task EditTeamRosterAsync (TournamentTeam team)
		{
			var existing = await Context.TournamentTeams
				.Include(t => t.Participants).ThenInclude(p => p.Player)
				.Where(t => t.TeamId == team.TeamId)
				.AsTracking()
				.SingleOrDefaultAsync();
			if (existing is null)
			{
				throw new KeyNotFoundException();
			}
			if (!Authorizer.HasClaim(EditTeamRoster, existing))
			{
				throw new UnauthorizedAccessException();
			}

			// Validate that the registration rules are adhered to
			var rules = await Context.TournamentRegistrationRules
				.Include(r => r.Tournament).ThenInclude(t => t.Teams)
				.Where(r => r.TournamentId == team.TournamentId && r.Region == team.Region)
				.SingleOrDefaultAsync();
			var validator = new TournamentRegistrationRulesValidator(rules);

			if (!validator.CanEditTeamRoster())
			{
				throw new ArgumentNullException("Team Rosters can no longer be adjusted.");
			}
			validator.ValidateTeam(team, rules.Tournament.Teams.Where(t => t.TeamId != team.TeamId));
			await SetWoWsPlayersAsync(team.Region, team.Participants);
			await validator.ValidateDiscordAsync(team, getDiscordUser);

			// Update participants
			await InitializeParticipantsAsync(rules, team);
			var orphans = existing.Participants.Where(p => !team.Participants.Any(d => d.ParticipantId == p.ParticipantId));
			await TrimOrphanParticipantsAsync(team, orphans);

			// Update roster on existing team
			existing.Participants = team.Participants;

			await Context.SaveChangesAsync();
		}

		// TODO: Withdraw feature
		//public async Task WithdrawTeamAsync (long teamId)
		//{
		//	var existing = await Context.TournamentTeams
		//		.Include(t => t.Participants)
		//		.Where(t => t.TeamId == teamId)
		//		.SingleOrDefaultAsync();
		//}

		public async Task AddEditTeamInfoClaim (TournamentTeam team, long accountId)
		{
			if (!Authorizer.HasClaim(EditTeamInfo, team))
			{
				throw new UnauthorizedAccessException();
			}

			var account = await Context.Accounts.FindAsync(accountId);
			if (account is null)
			{
				throw new KeyNotFoundException();
			}

			var claim = EditTeamInfo.ForTeam(team.TeamId);
			claim.AccountId = accountId;

			if (await Context.Claims.Where(c => c.Title == claim.Title && c.AccountId == claim.AccountId && c.Value == claim.Value).AnyAsync())
			{
				// Already exists
				return;
			}

			Context.Claims.Add(claim);
			await Context.SaveChangesAsync();
		}

		public async Task RemoveEditTeamInfoClaim (TournamentTeam team, long accountId)
		{
			if (!Authorizer.HasClaim(EditTeamInfo, team))
			{
				throw new UnauthorizedAccessException();
			}

			var scoped = EditTeamInfo.ForTeam(team.TeamId);
			var claims = Context.Claims
				.Where(c => c.AccountId == accountId && c.Title == scoped.Title && c.Value == scoped.Value)
				.AsEnumerable();
			Context.Claims.RemoveRange(claims);
			await Context.SaveChangesAsync();
		}

		public async Task AddEditTeamRosterClaim (TournamentTeam team, long accountId)
		{
			if (!Authorizer.HasClaim(EditTeamRoster, team))
			{
				throw new UnauthorizedAccessException();
			}

			var account = await Context.Accounts.FindAsync(accountId);
			if (account is null)
			{
				throw new KeyNotFoundException();
			}

			var claim = EditTeamRoster.ForTeam(team.TeamId);
			claim.AccountId = accountId;

			if (await Context.Claims.Where(c => c.Title == claim.Title && c.AccountId == claim.AccountId && c.Value == claim.Value).AnyAsync())
			{
				// Already exists
				return;
			}

			Context.Claims.Add(claim);
			await Context.SaveChangesAsync();
		}

		public async Task RemoveEditTeamRosterClaim (TournamentTeam team, long accountId)
		{
			if (!Authorizer.HasClaim(EditTeamRoster, team))
			{
				throw new UnauthorizedAccessException();
			}

			var scoped = EditTeamRoster.ForTeam(team.TeamId);
			var claims = Context.Claims
				.Where(c => c.AccountId == accountId && c.Title == scoped.Title && c.Value == scoped.Value)
				.AsEnumerable();
			Context.Claims.RemoveRange(claims);
			await Context.SaveChangesAsync();
		}


		private async Task SetWoWsPlayersAsync (Region region, IEnumerable<TournamentParticipant> participants)
		{
			foreach (var participant in participants)
			{
				// Update data for the player
				participant.Player = await WarshipsApi.GetPlayerInfoAsync(region, participant.PlayerId);
			}
		}

		private async Task InitializeParticipantsAsync (TournamentRegistrationRules rules, TournamentTeam team)
		{
			// Determine participants' status
			var initStatus = rules.Rules.HasFlag(RegistrationRules.RequireInvitationAccept) ? ParticipantStatus.Invited : ParticipantStatus.Accepted;
			foreach (var participant in team.Participants)
			{
				// Add claims to team reps
				if (participant.Player?.AccountId is long accountId && participant.TeamRep)
				{
					await AddEditTeamInfoClaim(team, accountId);
					await AddEditTeamRosterClaim(team, accountId);
				}
				
				// Verify the player doesn't already belong to a team
				if (initStatus == ParticipantStatus.Accepted &&
					Context.TournamentParticipants
						.Include(p => p.Team)
						.Where(p => p.Team.TournamentId == team.TournamentId && p.Team.Region == team.Region && p.PlayerId == participant.PlayerId && p.Status == ParticipantStatus.Accepted)
						.Any())
				{
					throw new ArgumentException($"{participant.Player?.Nickname ?? "One of these players"} already belongs to another team.");
				}
			}
		}

		private async Task TrimOrphanParticipantsAsync (TournamentTeam team, IEnumerable<TournamentParticipant> orphans)
		{
			foreach (var orphan in orphans)
			{
				// Remove orphans' claims on the team
				//   if trying to remove a permission that you don't have permission to remove, you don't have permission to remove the player from the team
				if (orphan.Player?.AccountId is long accountId)
				{
					await RemoveEditTeamInfoClaim(team, accountId);
					await RemoveEditTeamRosterClaim(team, accountId);
				}

				// Remove orphans from the context
				Context.TournamentParticipants.Remove(orphan);
			}
			await Context.SaveChangesAsync();
		}

		private Task<DiscordUser> getDiscordUser (long accountId)
		{
			return Context.DiscordUsers
				.Where(du => du.AccountId == accountId)
				.OrderByDescending(du => du.IsPrimary)
				.FirstOrDefaultAsync();
		}
	}
}
