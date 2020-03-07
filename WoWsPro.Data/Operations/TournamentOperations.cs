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
			=> Context.TournamentTeams
			.Where(t => t.TournamentId == ScopeId)
			.Cast<Shared.Models.TournamentTeam>()
			.AsEnumerable();
	}
}
