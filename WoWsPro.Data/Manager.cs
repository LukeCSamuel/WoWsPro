using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.Authentication;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.DB;
using WoWsPro.Data.Managers;
using WoWsPro.Shared.Models;

namespace WoWsPro.Data
{
	public interface IContextManager
	{
		internal Context Context { get; }
	}

	public class ContextManager : IContextManager
	{
		internal Context Context { get; }
		Context IContextManager.Context => Context;

		public ContextManager (IAuthorization authorization, IAuthentication authentication, IConfiguration configuration)
		{
			Context = new Context(authorization, authentication, configuration);
		}

		public void Dispose () => ((IDisposable)Context).Dispose();
	}

	public static class ManagerFactoryInjector
	{
		public static IServiceCollection AddDataManagers (this IServiceCollection services)
			=> services
			.AddAuthorization()
			.AddAuthentication()
			.AddScoped<IContextManager, ContextManager>()
			.AddScoped<ApplicationSettingManager, ApplicationSettingManager>()
			.AddScoped<ClaimManager, ClaimManager>()
			;
	}

}
