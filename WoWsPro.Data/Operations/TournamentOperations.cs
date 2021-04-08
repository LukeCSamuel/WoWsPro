using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Tournaments;
using WoWsPro.Shared.Permissions;

namespace WoWsPro.Data.Operations
{
	public class TournamentOperations : Operations
	{
        public TournamentOperations(Context context, IAuthorizer authorizer) : base(context, authorizer) { }

        /// <summary>
        /// Returns the specified tournament without additional information, such as the teams registered
        /// </summary>
        public Tournament GetTournament (long id)
		{
			return Context.Tournaments
				.Include(t => t.RegistrationRules).ThenInclude(r => r.RegistrationQuestions)
				.Where(t => t.TournamentId == id)
				.SingleOrDefault() ?? throw new KeyNotFoundException();
		}

		public IEnumerable<Tournament> ListTournaments ()
			=> Context.Tournaments
				.AsEnumerable();

		public IAsyncEnumerable<TournamentTeam> ListTeamsAsync (long tournamentId)
		{
			return Context.TournamentTeams
				.Include(t => t.Participants).ThenInclude(p => p.Player)
				.Where(t => t.TournamentId == tournamentId)
				.AsAsyncEnumerable();
		}

		public IEnumerable<long> GetWaitList (long tournamentId, Region region)
		{
            return Context.TournamentTeams
                .OrderBy(t => t.Created)
                .Where(t => t.TournamentId == tournamentId && t.Region == region && t.Status == TeamStatus.WaitListed)
				.Select(t => t.TeamId)
                .ToList();
        }
	}
}
