using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWsPro.Client.Utils;
using WoWsPro.Shared.Models;
using WoWsPro.Shared.Permissions;

namespace WoWsPro.Client.Services
{
	public interface IAccountService : IUserProvider
	{
		Cache<Account> UserAccount { get; }
	}
	
	public class AccountService : IDisposable, IAccountService
	{

		public Cache<Account> UserAccount { get; private set; }

		HttpClient Http { get; }
		IUserService UserService { get; }
		NavigationManager Navi { get; }

        bool IUserProvider.IsLoggedIn => _userAccount is not null;

        Account _userAccount = null;
        Account IUserProvider.Account => _userAccount;

        public AccountService (HttpClient http, IUserService userService, NavigationManager navi)
		{
			Http = http;
			Navi = navi;
			UserService = userService;
			UserService.User.Updated += OnUserUpdated;
			UserAccount = new Cache<Account>(GetUserAccount);
		}

		async Task<Account> GetUserAccount ()
		{
			if ((await UserService.User)?.IsLoggedIn is true)
			{
				_userAccount = await Http.GetAsAsync<Account>($"api/Account");
                return _userAccount;
            }
			else
			{
				return null;
			}
		}

		public Task<Account> GetAccount (long id) => Http.GetAsAsync<Account>($"api/Account/{id}");

        private async void OnUserUpdated(object sender, User user)
        {
            await UserAccount.UpdateAsync();
        }

        public void Dispose () => UserService.User.Updated -= OnUserUpdated;
	}

	public static class AccountServiceProvider
	{
		public static IServiceCollection AddAccountService (this IServiceCollection services)
			=> services
				.AddScoped<IAccountService, AccountService>()
				.AddScoped<IUserProvider, AccountService>();
	}
}
