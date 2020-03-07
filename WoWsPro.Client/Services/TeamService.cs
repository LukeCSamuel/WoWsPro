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
		Task<TournamentTeam> GetTeamByIdAsync (long teamId);
		Task<long?> CreateTeamAsync (TournamentTeam team);
		Task<bool> UpdateTeamAsync (TournamentTeam team);
	}

	public class TeamService : ITeamService
	{
		HttpClient Http { get; }

		public TeamService (HttpClient http) => Http = http;

		public async Task<TournamentTeam> GetTeamByIdAsync (long teamId)
		{
			try
			{
				return await Http.GetJsonAsync<TournamentTeam>($"api/team/{teamId}");
			}
			catch
			{
				return null;
			}
		}

		public async Task<long?> CreateTeamAsync (TournamentTeam team)
		{
			try
			{
				return await Http.PostJsonAsync<long>($"api/team/create", team);
			}
			catch
			{
				return null;
			}
		}

		public async Task<bool> UpdateTeamAsync (TournamentTeam team)
		{
			try
			{
				return await Http.PostJsonAsync<bool>($"api/team/update", team);
			}
			catch
			{
				return false;
			}
		}
	}

	public static class TeamServiceProvider
	{
		public static IServiceCollection AddTeamService (this IServiceCollection services)
			=> services.AddScoped<ITeamService, TeamService>();
	}
}
