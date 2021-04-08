using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Client.Utils;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Warships;

namespace WoWsPro.Client.Services
{
	public interface IWarshipsService
	{
		Task<IEnumerable<WarshipsPlayer>> SearchPlayersAsync (Region region, string name);
	}
	
	public class WarshipsService : IWarshipsService
	{
		HttpClient Http { get; set; }

		public WarshipsService (HttpClient http) => Http = http;

		public Task<IEnumerable<WarshipsPlayer>> SearchPlayersAsync (Region region, string name) 
			=> Http.GetAsAsync<IEnumerable<WarshipsPlayer>>($"/api/WarshipsApi/{region}/player/{name}");
	}

	public static class WarshipsServiceProvider
	{
		public static IServiceCollection AddWarshipsService (this IServiceCollection services)
			=> services.AddScoped<IWarshipsService, WarshipsService>();
	}
}
