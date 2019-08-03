using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;

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
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.SetBasePath(Directory.GetCurrentDirectory());
					config.AddCommandLine(args);
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					if (hostingContext.HostingEnvironment.EnvironmentName is string env)
					{
						config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
					}
				})
				.UseStartup<Startup>()
				.Build();
	}
}
