using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WoWsPro.Client.Services;

namespace WoWsPro.Client
{
	public class Program
	{
		public static async Task Main (string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			ConfigureServices(builder.Services);
			builder.RootComponents.Add<App>("app");

			await builder.Build().RunAsync();
		}

		public static void ConfigureServices (IServiceCollection services)
		{
			services.AddUserService();
			services.AddAccountService();
		}
	}
}
