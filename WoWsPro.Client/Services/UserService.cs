using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WoWsPro.Client.Services
{
	public class User
	{
		public bool IsLoggedIn { get; set; }
		public string PreferredName { get; set; }
		public long? AccountId { get; set; }

		public User ()
		{
			IsLoggedIn = false;
			PreferredName = null;
			AccountId = null;
		}

		public User (string name, long id)
		{
			IsLoggedIn = true;
			PreferredName = name;
			AccountId = id;
		}

		public override string ToString () => IsLoggedIn ? PreferredName : "Guest";

	}

	public interface IUserService
	{
		User User { get; }
		Task<User> GetUserAsync ();
		Task UpdateAsync ();

		event EventHandler<User> UserUpdate;
	}

	public class UserService : IUserService
	{
		const string _address = "api/Account/User";
		private readonly HttpClient _http;

		public event EventHandler<User> UserUpdate;

		public User User
		{
			get => _user;
			private set
			{
				_user = value;
				UserUpdate(this, _user);
			}
		}
		private User _user;


		public UserService (HttpClient http) => _http = http;

		public async Task UpdateAsync () => User = await _http.GetJsonAsync<User>(_address);

		public async Task<User> GetUserAsync ()
		{
			if (User is null)
			{
				await UpdateAsync();
			}
			return User;
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
