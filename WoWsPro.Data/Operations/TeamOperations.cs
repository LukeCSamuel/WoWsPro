﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WoWsPro.Data.Attributes;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.Operations
{
	public partial class TeamOperations
	{
		[Public]
		public Shared.Models.TournamentTeam GetTeamById (long teamId) => Context.TournamentTeams.SingleOrDefault(t => t.TeamId == teamId) ?? throw new KeyNotFoundException();

		/// <summary>
		/// Gets the team to which an account belongs, if one exists.
		/// </summary>
		[Public]
		public Shared.Models.TournamentTeam GetJoinedTeam (long playerId, long tournamentId)
		{
			var player = Context.WarshipsPlayers.SingleOrDefault(p => p.PlayerId == playerId);
			if (player is null)
			{
				throw new KeyNotFoundException();
			}
			else
			{
				var teams = player.Participations
					.Where(p => p.Team.TournamentId == tournamentId && p.Status == ParticipantStatus.Accepted);

				if (teams.SingleOrDefault() is DB.Models.TournamentParticipant result)
				{
					// Return the player's team.
					return result.Team;
				}
				else if (teams.Count() > 0)
				{
					// On multiple teams... theoretically this never happens
					throw new InvalidOperationException("A players is on multiple teams for the same tournament...");
				}
				else
				{
					// Not on any team
					return null;
				}
			}
		}

		[Public]
		public long CreateNewTeam (Shared.Models.TournamentTeam team)
		{
			// Verify that all rules are adhered to
			var rules = Context.TournamentRegistrationRules.SingleOrDefault(r => r.TournamentId == team.TournamentId && r.Region == team.Region);
			var owner = Context.Accounts.SingleOrDefault(a => a.AccountId == team.OwnerAccountId);
			if (rules is null || owner is null)
			{
				throw new KeyNotFoundException();
			}
			else
			{
				// Verify registration is open
				if (rules.Open > DateTime.UtcNow || rules.Close < DateTime.UtcNow)
				{
					throw new NotSupportedException("Registration period has ended.");
				}

				// Verify team size
				if (team.Participants.Count < rules.MinTeamSize || team.Participants.Count > rules.MaxTeamSize)
				{
					throw new NotSupportedException("Team does not meet size requirements.");
				}

				// Verify creator does not have team
				foreach (var player in owner.WarshipsAccounts)
				{
					if (GetJoinedTeam(player.PlayerId, team.TournamentId) is Shared.Models.TournamentTeam)
					{
						throw new NotSupportedException("Owner already belongs to a team.");
					}
				}

				// Check rules flags
				
				// Team Owner Discord Rule
				if (rules.Rules.HasFlag(RegistrationRules.RequireTeamOwnerDiscord))
				{
					if (owner.DiscordAccounts.Count < 1)
					{
						throw new NotSupportedException("Owner must have a connected discord account.");
					}
				}

				// TODO: There HAS to be a better way to implement this kind of functionality...
				//       Should this really be a registration check, or should it be an "acceptance" check?
				//       May need to slightly rework RegistrationRules flags
				// Team members Discord Role
				//if (rules.Rules.HasFlag(RegistrationRules.RequireParticipantsDiscord))
				//{
				//	foreach (var participant in team.Participants)
				//	{
				//		var acc = Context.WarshipsPlayers.SingleOrDefault(p => p.PlayerId == participant.PlayerId)?.Account;
				//		if (acc is null || acc.DiscordAccounts.Count < 1)
				//		{
				//			throw new NotSupportedException("All team members must have a connected discord account.");
				//		}
				//	}
				//}

				// Check capacity
				var initStatus = TeamStatus.Registered;
				if (rules.Capacity <= rules.Tournament.Teams.Count(t => t.Region == rules.Region))
				{
					initStatus = TeamStatus.WaitListed;
				}

				// Check that there are no duplicate participants
				if (team.Participants.GroupBy(p => p.PlayerId).Any(g => g.Count() > 1))
				{
					// Contains duplicates, reject!
					throw new NotSupportedException("Duplicate participatns are not allowed.");
				}

				// Ready to create the team
				var dbTeam = new DB.Models.TournamentTeam()
				{
					Name = team.Name,
					Tag = team.Tag,
					Icon = team.Icon,
					Created = DateTime.UtcNow,
					Description = team.Description,
					Region = team.Region,
					OwnerAccountId = team.OwnerAccountId,
					Status = initStatus,
					TournamentId = team.TournamentId
				};
				Context.TournamentTeams.Add(dbTeam);
				Context.SaveChanges();

				// Add required claim to owner
				Context.TournamentTeamClaims.Add(new DB.Models.TournamentTeamClaim()
				{
					AccountId = owner.AccountId,
					ClaimId = Context.Claims.Single(c => c.Permission == nameof(Permissions.ManageTeam)).ClaimId,
					TeamId = dbTeam.TeamId
				});
				Context.TournamentTeamClaims.Add(new DB.Models.TournamentTeamClaim()
				{
					AccountId = owner.AccountId,
					ClaimId = Context.Claims.Single(c => c.Permission == nameof(Permissions.ManageTeamClaims)).ClaimId,
					TeamId = dbTeam.TeamId
				});
				Context.SaveChanges();

				// Add participants to the team
				foreach (var participant in team.Participants)
				{
					var status = ParticipantStatus.Accepted;
					// Check if the participant already has a team if doing accept only
					try
					{
						if (rules.Rules.HasFlag(RegistrationRules.RequireInvitationAccept) || GetJoinedTeam(participant.PlayerId, team.TournamentId) is Shared.Models.TournamentTeam)
						{
							// Player does have team, can only invite them
							status = ParticipantStatus.Invited;
						}
					}
					catch (KeyNotFoundException)
					{
						// Player hasn't been recorded yet, so all is good!
					}

					// Update data for the player
					WarshipsApi.GetPlayerInfoAsync(dbTeam.Region, participant.PlayerId).GetAwaiter().GetResult();

					// Add the participant to the team
					var dbParticipant = new DB.Models.TournamentParticipant()
					{
						TeamId = dbTeam.TeamId,
						PlayerId = participant.PlayerId,
						Status = status
					};
					Context.TournamentParticipants.Add(dbParticipant);
				}
				Context.SaveChanges();

				return dbTeam.TeamId;
			}
		}

		[Authorize(nameof(Permissions.ManageTeam))]
		public bool UpdateTeam (Shared.Models.TournamentTeam team)
		{
			var rules = Context.TournamentRegistrationRules.SingleOrDefault(r => r.TournamentId == team.TournamentId && r.Region == team.Region);
			var context = Context.TournamentTeams.SingleOrDefault(t => t.TeamId == team.TeamId);
			if (context is null)
			{
				throw new KeyNotFoundException();
			}
			else if (rules.Close < DateTime.UtcNow)
			{
				throw new NotSupportedException("Registration has closed.");
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

			return true;
		}

		[Authorize(nameof(Permissions.ManageTeam))]
		public bool UpdateTeamMembers (Shared.Models.TournamentTeam team)
		{
			var rules = Context.TournamentRegistrationRules.SingleOrDefault(r => r.TournamentId == team.TournamentId && r.Region == team.Region);
			var context = Context.TournamentTeams.SingleOrDefault(t => t.TeamId == team.TeamId);
			if (rules is null || context is null)
			{
				throw new KeyNotFoundException();
			}
			else if (rules.Close < DateTime.UtcNow && !rules.Rules.HasFlag(RegistrationRules.AllowRosterChanges))
			{
				throw new NotSupportedException("Registration is closed and roster changes are not allowed.");
			}
			else
			{
				var toRemove = context.Participants.Where(cp => !team.Participants.Any(tp => tp.PlayerId == cp.PlayerId));
				var toAdd = team.Participants.Where(tp => !context.Participants.Any(cp => tp.PlayerId == cp.PlayerId));

				foreach (var remove in toRemove)
				{
					Context.TournamentParticipants.Remove(remove);
				}
				Context.SaveChanges();

				foreach (var add in toAdd)
				{
					var status = ParticipantStatus.Accepted;
					// Check if the participant already has a team if doing accept only
					try
					{
						if (rules.Rules.HasFlag(RegistrationRules.RequireInvitationAccept) || GetJoinedTeam(add.PlayerId, team.TournamentId) is Shared.Models.TournamentTeam)
						{
							// Player does have team, can only invite them
							status = ParticipantStatus.Invited;
						}
					}
					catch (KeyNotFoundException)
					{
						// Player hasn't been recorded yet, so all is good!
					}

					// Update data for the player
					WarshipsApi.GetPlayerInfoAsync(context.Region, add.PlayerId).GetAwaiter().GetResult();

					// Add the participant to the team
					var dbParticipant = new DB.Models.TournamentParticipant()
					{
						TeamId = context.TeamId,
						PlayerId = add.PlayerId,
						Status = status
					};
					Context.TournamentParticipants.Add(dbParticipant);
				}
				Context.SaveChanges();
			}

			return true;
		}
	}
}
