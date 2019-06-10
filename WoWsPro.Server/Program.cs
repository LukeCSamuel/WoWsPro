using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WoWsPro.Server
{
	public class Program
	{
		public static void Main (string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost (string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseConfiguration(new ConfigurationBuilder()
					.AddCommandLine(args)
					.AddJsonFile("appsettings.json")
					.AddJsonFile("appsettings.development.json") // TODO: how not include for prod??
					.Build())
				.UseStartup<Startup>()
				.Build();
	}
}
