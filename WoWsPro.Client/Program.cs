using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WoWsPro.Client.Services;
using WoWsPro.Shared.Services;

namespace WoWsPro.Client
{
	public class Program
	{
		public static async Task Main (string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			ConfigureServices(builder.Services);

			await builder.Build().RunAsync();
		}

		public static void ConfigureServices (IServiceCollection services)
		{
			services.AddUserService();
            services.AddAccountService();
            services.AddAuthorizer();
			services.AddTournamentService();
			services.AddTeamService();
			services.AddWarshipsService();
			services.AddWebAssemblyRenderSource();
		}
	}
}
