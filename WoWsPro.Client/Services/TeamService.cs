using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Client.Utils;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Client.Services
{
	public interface ITeamService
	{
		Task<TournamentTeam> GetTeamByIdAsync (long teamId);

		Task<HttpResponseMessage> CreateTeamAsync (TournamentTeam team);
		Task<HttpResponseMessage> UpdateTeamInfoAsync (TournamentTeam team);
		Task<HttpResponseMessage> UpdateTeamRosterAsync (TournamentTeam team);
	}

	public class TeamService : ITeamService
	{
		HttpClient Http { get; }


        public TeamService(HttpClient http)
        {
            Http = http;
        }

        public async Task<TournamentTeam> GetTeamByIdAsync (long teamId)
		{
			try
			{
				return await Http.GetAsAsync<TournamentTeam>($"api/team/{teamId}");
			}
			catch
			{
				return null;
			}
		}

		public async Task<HttpResponseMessage> CreateTeamAsync (TournamentTeam team)
		{
			try
			{
				return await Http.PostAsAsync($"api/team/create", team);
			}
			catch
			{
				return null;
			}
		}

		public async Task<HttpResponseMessage> UpdateTeamInfoAsync (TournamentTeam team)
		{
			try
			{
				return await Http.PostAsAsync($"api/Team/update/info", team);
			}
			catch
			{
				return null;
			}
		}

		public async Task<HttpResponseMessage> UpdateTeamRosterAsync (TournamentTeam team)
		{
			try
			{
				return await Http.PostAsAsync($"api/Team/update/roster", team);
			}
			catch
			{
				return null;
			}
		}
	}

	public static class TeamServiceProvider
	{
		public static IServiceCollection AddTeamService (this IServiceCollection services)
			=> services.AddScoped<ITeamService, TeamService>();
	}
}
