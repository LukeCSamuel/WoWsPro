using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Client.Utils;

namespace WoWsPro.Client.Services
{
	public class User
	{
		public bool IsLoggedIn { get; set; }
		public string PreferredName { get; set; }
		public long? AccountId { get; set; }

		public User () { }

		public override string ToString () => IsLoggedIn ? PreferredName : "Guest";
	}

	public interface IUserService
	{
		Cache<User> User { get; }
	}

	public class UserService : IUserService
	{
		HttpClient Http { get; }

		public Cache<User> User { get; }

		public UserService (HttpClient http)
		{
			Http = http;
			User = new Cache<User>(() => Http.GetJsonAsync<User>("api/Account/User"));
		}

	}

	public static class UserServiceProvider
	{
		public static IServiceCollection AddUserService (this IServiceCollection services)
		{
			services.AddScoped<IUserService, UserService>();
			return services;
		}
	}
}
