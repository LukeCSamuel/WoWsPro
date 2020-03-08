using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Client.Utils;
using WoWsPro.Shared.Models;

namespace WoWsPro.Client.Services
{
	public interface ITournamentService
	{
		long CurrentTournamentId { get; }
		Cache<Tournament> Current { get; }
		Cache<List<Tournament>> Previews { get; }
		Cache<List<TournamentTeam>> CurrentTeams { get; }


		Task SetCurrentTournamentIdAsync (long tournamentId);
	}

	public class TournamentService : ITournamentService
	{
		public long CurrentTournamentId { get; private set; }
		public Cache<Tournament> Current { get; }
		public Cache<List<Tournament>> Previews { get;}
		public Cache<List<TournamentTeam>> CurrentTeams { get; }


		HttpClient Http { get; }

		public TournamentService (HttpClient http)
		{
			Http = http;
			Previews = new Cache<List<Tournament>>(() => Http.GetJsonAsync<List<Tournament>>($"api/Tournament"));
			Current = new Cache<Tournament>(GetTournament);
			CurrentTeams = new Cache<List<TournamentTeam>>(GetCurrentTeams);
		}

		public async Task<Tournament> GetTournament ()
		{
			try
			{
				if (CurrentTournamentId == 0)
				{
					return null;
				}

				return await Http.GetJsonAsync<Tournament>($"api/Tournament/{CurrentTournamentId}");
			}
			catch
			{
				return null;
			}
		}

		public async Task<List<TournamentTeam>> GetCurrentTeams ()
		{
			try
			{
				if (CurrentTournamentId == 0)
				{
					return null;
				}

				return await Http.GetJsonAsync<List<TournamentTeam>>($"api/Tournament/{CurrentTournamentId}/teams");
			}
			catch
			{
				return null;
			}
		}

		public async Task SetCurrentTournamentIdAsync (long tournamentId)
		{
			if (CurrentTournamentId != tournamentId)
			{
				CurrentTournamentId = tournamentId;
				await Current.UpdateAsync();
			}
		}
	}

	public static class TournamentServiceProvider
	{
		public static IServiceCollection AddTournamentService (this IServiceCollection services)
			=> services.AddScoped<ITournamentService, TournamentService>();
	}
}
