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
		Account UserAccount { get; }
		Task<Account> GetUserAccountAsync ();
		Task UpdateUserAccountAsync ();
		event EventHandler<Account> UserAccountUpdate;
	}
	
	public class AccountService : IDisposable, IAccountService
	{
		public Account UserAccount {

			get => _userAccount;
			private set
			{
				_userAccount = value;
				UserAccountUpdate(this, _userAccount);
			}
		}
		private Account _userAccount;

		public HttpClient Http { get; }
		IUserService UserService { get; }

		public event EventHandler<Account> UserAccountUpdate;

		public AccountService (HttpClient http, IUserService userService)
		{
			Http = http;
			UserService = userService;
			UserService.UserUpdate += OnUserUpdate;
		}

		public async Task<Account> GetUserAccountAsync ()
		{
			if (UserAccount is null)
			{
				await UpdateUserAccountAsync();
			}
			return UserAccount;
		}
		public async Task UpdateUserAccountAsync () => UserAccount = await Http.GetJsonAsync<Account>($"api/Account");

		public async Task<Account> GetAccount () => await Http.GetJsonAsync<Account>($"api/Account/{UserService.User?.AccountId}");

		private async void OnUserUpdate (object sender, User user) => await UpdateUserAccountAsync();

		public void Dispose () => UserService.UserUpdate -= OnUserUpdate;
	}

	public static class AccountServiceProvider
	{
		public static IServiceCollection AddAccountService (this IServiceCollection services)
			=> services.AddScoped<IAccountService, AccountService>();
	}
}
