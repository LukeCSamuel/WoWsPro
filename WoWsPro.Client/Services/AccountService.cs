using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Client.Utils;
using WoWsPro.Shared.Models;

namespace WoWsPro.Client.Services
{
	public interface IAccountService
	{
		Cache<Account> UserAccount { get; }
	}
	
	public class AccountService : IDisposable, IAccountService
	{
		public Cache<Account> UserAccount { get; private set; }

		HttpClient Http { get; }
		IUserService UserService { get; }
		NavigationManager Navi { get; }

		public AccountService (HttpClient http, IUserService userService, NavigationManager navi)
		{
			Http = http;
			Navi = navi;
			UserService = userService;
			UserService.User.Updated += OnUserUpdated;
			UserAccount = new Cache<Account>(GetUserAccount);
		}

		Task<Account> GetUserAccount ()
		{
			try
			{
				return Http.GetJsonAsync<Account>($"api/Account");
			}
			catch
			{
				Navi.NavigateTo("/account/login");
				return Task.FromResult<Account>(null);
			}
		}

		public Task<Account> GetAccount (long id) => Http.GetJsonAsync<Account>($"api/Account/{id}");

		private async void OnUserUpdated (object sender, User user) => await UserAccount.UpdateAsync();

		public void Dispose () => UserService.User.Updated -= OnUserUpdated;
	}

	public static class AccountServiceProvider
	{
		public static IServiceCollection AddAccountService (this IServiceCollection services)
			=> services.AddScoped<IAccountService, AccountService>();
	}
}
