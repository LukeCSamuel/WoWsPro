using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Data.Attributes;

namespace WoWsPro.Data.Operations
{
	public partial class TournamentOperations
	{
		[Public]
		public Shared.Models.Tournament GetTournament (long id) => Context.Tournaments.SingleOrDefault(t => t.TournamentId == id) ?? throw new KeyNotFoundException();

		[Public]
		public IEnumerable<Shared.Models.Tournament> ListTournaments () 
			=> Context.Tournaments
			.Cast<Shared.Models.Tournament>()
			.AsEnumerable();

		[Public]
		public IEnumerable<Shared.Models.TournamentTeam> ListTeams (long tournamentId) 
			=> Context.TournamentTeams
			.Where(t => t.TournamentId == tournamentId)
			.Cast<Shared.Models.TournamentTeam>()
			.AsEnumerable();
	}
}
