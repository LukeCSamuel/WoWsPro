using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Shared.Models;

namespace WoWsPro.Client.Services
{
	public interface ITournamentService
	{
		long CurrentTournamentId { get; }
		Tournament Current { get; }
		event EventHandler<Tournament> CurrentUpdate;


		Task SetCurrentTournamentIdAsync (long tournamentId);
		Task<IEnumerable<Tournament>> ListTournamentsAsync ();
	}

	public class TournamentService : ITournamentService
	{
		public long CurrentTournamentId { get; private set; }
		public Tournament Current { get; private set; }

		public event EventHandler<Tournament> CurrentUpdate;

		HttpClient Http { get; }

		public TournamentService (HttpClient http)
		{
			Http = http;
			CurrentUpdate += (_, tournament) => Current = tournament;
		}

		public Task<IEnumerable<Tournament>> ListTournamentsAsync () => Http.GetJsonAsync<IEnumerable<Tournament>>($"api/Tournament");

		public async Task SetCurrentTournamentIdAsync (long tournamentId)
		{
			if (CurrentTournamentId != tournamentId)
			{
				CurrentTournamentId = tournamentId;
				var current = await Http.GetJsonAsync<Tournament>($"api/Tournament/{tournamentId}");
				CurrentUpdate(this, current);
			}
		}
	}

	public static class TournamentServiceProvider
	{
		public static IServiceCollection AddTournamentService (this IServiceCollection services)
			=> services.AddScoped<ITournamentService, TournamentService>();
	}
}
