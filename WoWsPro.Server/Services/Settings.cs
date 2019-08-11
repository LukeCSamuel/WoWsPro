using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoWsPro.Server.Services
{
	public interface ISettings
	{
		string WoWsProConnectionString { get; }
		string BaseUrl { get; }
	}

	public class Settings : ISettings
	{
		readonly IConfiguration _config;

		public Settings (IConfiguration config)
		{
			_config = config;
		}

		public string WoWsProConnectionString => _config["ConnectionStrings:WoWsPro"];
		public string BaseUrl => _config["Urls:BaseUrl"];
	}

	public static class SettingsProvider
	{
		public static IServiceCollection AddSettings (this IServiceCollection services) 
			=> services.AddSingleton<ISettings, Settings>();
	}
}
