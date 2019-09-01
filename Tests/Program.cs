using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using WoWsPro.Data;
using WoWsPro.Data.Services;

namespace Tests
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var services = new ServiceCollection();
			ConfigureServices(services);

			var provider = services.BuildServiceProvider();
			await provider.GetService<App>().RunAsync();
		}

		public static void ConfigureServices(IServiceCollection services)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddJsonFile("appsettings.development.json")
				.Build();

			services.AddSingleton<IConfiguration>(config);
			services.AddDataManagers<ContextAuthentication>();
			services.AddTransient<App>();
		}

		public class ContextAuthentication : IContextAuthentication
		{
			public long? AccountId => 1;
		}
	}
}
