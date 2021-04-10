using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Discord;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Shared.Validation
{
	public class TournamentRegistrationRulesValidator
	{
		private TournamentRegistrationRules _rules;

		public TournamentRegistrationRulesValidator (TournamentRegistrationRules rules)
		{
			if (rules is null)
			{
				throw new ArgumentNullException(nameof(rules));
			}
			_rules = rules;
		}

		public void ValidateTeam (TournamentTeam team) => ValidateTeam(team, null);
		public void ValidateTeam (TournamentTeam team, IEnumerable<TournamentTeam> allTeams)
		{
			// validate tag
			if (!Regex.IsMatch(team.Tag, @"^[a-zA-Z0-9_-]{2,5}$"))
			{
				throw new ArgumentException($"Tag must be 2-5 latin characters, digits, '-', or '_'.");
			}

			// validate unique tag
			if (allTeams?.Any(t => t.Tag == team.Tag) ?? false)
			{
				throw new ArgumentException($"Cannot use tag {team.Tag} because it is already in use.");
			}

			// validate team size
			if (team.Participants.Count < _rules.MinTeamSize)
			{
				throw new ArgumentException("Team does not have enough players.");
			}
			if (team.Participants.Count > _rules.MaxTeamSize)
			{
				throw new ArgumentException("Team has too many players.");
			}

			// validate there are no default values for players
			foreach (var player in team.Participants)
			{
				if (player.PlayerId == 0)
				{
					throw new ArgumentException($"Could not find a World of Warships account for {player.Player?.Nickname ?? "one or more players"}.");
				}
			}

			// validate no duplicate members
			if (team.Participants.GroupBy(p => p.PlayerId).Any(g => g.Count() > 1))
			{
				throw new ArgumentException("There are duplicate players.");
			}

			// validate rep count
			int repCount = team.Participants.Count(p => p.TeamRep);
			if (_rules.MinReps is int minReps && minReps > repCount)
			{
				throw new ArgumentException($"Team must have at least {minReps} Team Reps.");
			}
			if (_rules.MaxReps is int maxReps && maxReps < repCount)
			{
				throw new ArgumentException($"Team may not have more than {maxReps} Team Reps.");
			}

			// validate there are responses for required registration questions
			if (_rules.RegistrationQuestions is not null) {
				foreach (var question in _rules.RegistrationQuestions.Where(q => q.IsRequired))
				{
					if (!team.QuestionResponses.Any(r => r.RegistrationQuestionId == question.RegistrationQuestionId))
					{
						throw new ArgumentException($"Missing responses to one or more required questions.");
					}
				}
			}
		}

		/// <summary>
		/// Validates a team has the correct discord presense using a supplied asynchronous function to look up discord users
		/// </summary>
		/// <param name="team">The team to validate</param>
		/// <param name="discordLookup">An asynchronous function that accepts an Account Id and returns a DiscordUser</param>
		public async Task ValidateDiscordAsync (TournamentTeam team, Func<long, Task<DiscordUser>> discordLookup)
		{
			bool all = _rules.Rules.HasFlag(RegistrationRules.RequireParticipantsDiscord);
			if (all || _rules.Rules.HasFlag(RegistrationRules.RequireTeamRepsDiscord))
			{
				foreach (var account in team.Participants.Where(p => all || p.TeamRep).Select(p => p.Player.AccountId))
				{
					if (!(account is long id && (await discordLookup(id)) is not null))
					{
						throw new ArgumentException($"All {(all ? "Team Members" : "Team Reps")} must have connected Discord accounts.");
					}
				}
			}
		}

		public bool CanEditTeamInfo ()
		{
			var now = DateTime.UtcNow;
			return now > _rules.Open && now < _rules.Close;
		}

		public bool CanEditTeamRoster ()
		{
			var now = DateTime.UtcNow;
			return _rules.Rules.HasFlag(RegistrationRules.AllowRosterChanges) || (now > _rules.Open && now < _rules.Close);
		}
	}
}
