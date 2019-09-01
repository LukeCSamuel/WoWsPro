using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Data.Managers;
using WoWsPro.Shared.Models;

namespace WoWsPro.Data.Services
{
	public interface IContextManager
	{
		internal Context Context { get; }
	}

	public class ContextManager : IContextManager
	{
		internal Context Context { get; }
		Context IContextManager.Context => Context;

		public ContextManager (IConfiguration configuration)
		{
			Context = new Context(configuration);
		}

		public void Dispose () => ((IDisposable)Context).Dispose();
	}

	public static class ManagerProvider
	{
		public static IServiceCollection AddDataManagers<T> (this IServiceCollection services) where T : class, IContextAuthentication
			=> services
			.AddContextAuthorization<T>()
			.AddSingleton<IWarshipsApi, WarshipsApi>()
			.AddScoped<IApplicationSettingManager, ApplicationSettingManager>()
			.AddScoped<IAccountManager, AccountManager>()
			.AddScoped<IDiscordManager, DiscordManager>()
			.AddScoped<IWarshipsManager, WarshipsManager>()
			;
	}

}
