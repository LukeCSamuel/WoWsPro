using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Data.Attributes;

namespace WoWsPro.Data.Operations
{
	public partial class TournamentOperations
	{
		/// <summary>
		/// Returns the specified tournament without additional information, such as the teams registered
		/// </summary>
		[Public]
		public Shared.Models.Tournament GetTournament (long id)
		{
			return Context.Tournaments.Include(t => t.RegistrationRules)  //.Include(t => t.Teams).ThenInclude(t => t.)
				.Where(t => t.TournamentId == id)
				.Select(t => new Shared.Models.Tournament()
				{
					TournamentId = t.TournamentId,
					Name = t.Name,
					Icon = t.Icon,
					OwnerAccountId = t.OwnerAccountId,
					GuildId = t.GuildId,
					Description = t.Description,
					Created = t.Created,
					RegistrationRules = t.RegistrationRules
						.Select(r => new Shared.Models.TournamentRegistrationRules()
						{
							TournamentRegistrationRulesId = r.TournamentRegistrationRulesId,
							TournamentId = r.TournamentId,
							Region = r.Region,
							Rules = r.Rules,
							Capacity = r.Capacity,
							MaxTeamSize = r.MaxTeamSize,
							MinTeamSize = r.MinTeamSize,
							Close = r.Close,
							Open = r.Open,
							RegionParticipantRoleId = r.RegionParticipantRoleId,
							RegionTeamOwnerRoleId = r.RegionTeamOwnerRoleId
						}).ToList()
				})
				.SingleOrDefault() ?? throw new KeyNotFoundException();
		}

		[Public]
		public IEnumerable<Shared.Models.Tournament> ListTournaments ()
			=> Context.Tournaments
			.Cast<Shared.Models.Tournament>()
			.AsEnumerable();

		[Public]
		public IEnumerable<Shared.Models.Tournament> PreviewListTournaments ()
		{
			var tournaments = Context.Tournaments.AsEnumerable();
			foreach (var tournament in tournaments)
			{
				yield return new Shared.Models.Tournament()
				{
					TournamentId = tournament.TournamentId,
					Name = tournament.Name,
					Icon = tournament.Icon,
					Description = tournament.Description
				};
			}
		}

		[Public]
		public IEnumerable<Shared.Models.TournamentTeam> ListTeams ()
		{
			return Context.TournamentTeams
				.Include(t => t.Participants).ThenInclude(p => p.Player)
				.Where(t => t.TournamentId == ScopeId)
				.Select(t => new Shared.Models.TournamentTeam()
				{
					TournamentId = t.TournamentId,
					TeamId = t.TeamId,
					Name = t.Name,
					Tag = t.Tag,
					Description = t.Description,
					Icon = t.Icon,
					OwnerAccountId = t.OwnerAccountId,
					Status = t.Status,
					Region = t.Region,
					Created = t.Created,
					Participants = t.Participants.Select(p => new Shared.Models.TournamentParticipant()
					{
						ParticipantId = p.ParticipantId,
						TeamId = p.TeamId,
						PlayerId = p.PlayerId,
						Status = p.Status,
						Player = new Shared.Models.WarshipsPlayer()
						{
							PlayerId = p.PlayerId,
							Nickname = p.Player.Nickname,
							AccountId = p.Player.AccountId,
							ClanId = p.Player.ClanId,
							ClanRole = p.Player.ClanRole,
							Created = p.Player.Created,
							IsPrimary = p.Player.IsPrimary,
							JoinedClan = p.Player.JoinedClan,
							Region = p.Player.Region
						}
					}).ToList()
				})
				.AsEnumerable();
		}
	}
}
