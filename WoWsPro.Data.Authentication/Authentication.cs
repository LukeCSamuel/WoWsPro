using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authentication
{
	public class Authentication : IAuthentication
	{
		// TODO: Actually come up with an authentication implementation
		public long? AccountId => 1;
		public bool IsAuthenticated => true;
	}

	public static class AuthenticationInjector
	{
		public static IServiceCollection AddAuthentication (this IServiceCollection services) => services.AddScoped<IAuthentication, Authentication>();
	}
}
