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
	public interface ITeamService
	{
		Task<TournamentTeam> GetTeamById (long teamId);
	}
	
	public class TeamService : ITeamService
	{
		HttpClient Http { get; }

		public TeamService (HttpClient http)
		{
			Http = http;
		}

		public Task<TournamentTeam> GetTeamById (long teamId) => Http.GetJsonAsync<TournamentTeam>($"api/team/{teamId}");
	}

	public static class TeamServiceProvider
	{
		public static IServiceCollection AddTeamService (this IServiceCollection services)
			=> services.AddScoped<ITeamService, TeamService>();
	}
}
