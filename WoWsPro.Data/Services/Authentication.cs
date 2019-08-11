using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Services
{
	public interface IContextAuthentication
	{
		long? AccountId { get; }
	}

	public static class ContextAuthenticationProvider
	{
		public static IServiceCollection AddContextAuthentication<T> (this IServiceCollection services) where T : class, IContextAuthentication
			=> services.AddScoped<IContextAuthentication, T>();
	}
}
