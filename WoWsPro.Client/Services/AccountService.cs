using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Shared.Models;

namespace WoWsPro.Client.Services
{
	public interface IAccountService
	{
		Task<Account> GetUserAccountAsync ();
	}
	
	public class AccountService : IAccountService
	{
		public User User { get; set; }
		public HttpClient Http { get; }
		IUserService UserService { get; }

		public AccountService (HttpClient http, IUserService userService)
		{
			Http = http;
			UserService = userService;
			UserService.UserUpdate += OnUserUpdate;
		}

		public async Task<Account> GetUserAccountAsync ()
		{
			if (User is null)
			{
				User = await UserService.GetUserAsync();
			}
			return await Http.GetJsonAsync<Account>($"api/Account/{User.AccountId}");
		}

		private void OnUserUpdate (object sender, User user)
		{
			User = user;
		}
	}

	public static class AccountServiceProvider
	{
		public static IServiceCollection AddAccountService (this IServiceCollection services)
			=> services.AddScoped<IAccountService, AccountService>();
	}
}
