using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WoWsPro.Server
{
	public class Program
	{
		public static void Main (string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder (string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => {
					webBuilder.UseStartup<Startup>();
				});
				
				// .ConfigureAppConfiguration((hostingContext, config) =>
				// {
				// 	config.SetBasePath(Directory.GetCurrentDirectory());
				// 	config.AddCommandLine(args);
				// 	config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
				// 	if (hostingContext.HostingEnvironment.EnvironmentName is string env)
				// 	{
				// 		config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
				// 	}
				// })
				// .ConfigureLogging(logging =>
				// {
				// 	logging.ClearProviders();
				// 	logging.AddConsole();
				// })
				// .UseStartup<Startup>();
	}
}
