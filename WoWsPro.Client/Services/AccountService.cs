﻿using Microsoft.AspNetCore.Components;
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
		public Account UserAccount { get; private set; }

		HttpClient Http { get; }
		IUserService UserService { get; }
		NavigationManager Navi { get; }

		public event EventHandler<Account> UserAccountUpdate;

		public AccountService (HttpClient http, IUserService userService, NavigationManager navi)
		{
			Http = http;
			Navi = navi;
			UserService = userService;
			UserService.UserUpdate += OnUserUpdate;
			UserAccountUpdate += (_, account) => UserAccount = account;
		}

		public async Task<Account> GetUserAccountAsync ()
		{
			if (UserAccount is null)
			{
				await UpdateUserAccountAsync();
			}
			return UserAccount;
		}
		public async Task UpdateUserAccountAsync ()
		{
			try
			{
				var account = await Http.GetJsonAsync<Account>($"api/Account");
				UserAccountUpdate(this, account);
			}
			catch
			{
				Navi.NavigateTo("/account/login");
			}
		}

		public Task<Account> GetAccount () => Http.GetJsonAsync<Account>($"api/Account/{UserService.User?.AccountId}");

		private async void OnUserUpdate (object sender, User user) => await UpdateUserAccountAsync();

		public void Dispose () => UserService.UserUpdate -= OnUserUpdate;
	}

	public static class AccountServiceProvider
	{
		public static IServiceCollection AddAccountService (this IServiceCollection services)
			=> services.AddScoped<IAccountService, AccountService>();
	}
}
