using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.Services;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;

namespace WoWsPro.Data.Managers
{
	public interface IAccountManager
	{
		Task<string> GetNicknameAsync (long id);
		Task<Account> GetAccountAsync (long id);
		Task<Account> SetNicknameAsync (long id, string nickname);
	}

	internal class AccountManager : IAccountManager
	{
		IContextAuthorization Authorization { get; }
		Context Context => Authorization.Context;

		public AccountManager (IContextAuthorization auth)
		{
			Authorization = auth;
		}

		

		internal async Task<string> GetNicknameAsync (long id) => (await GetAccountAsync(id)).Nickname;
		internal async Task<DB.Models.Account> GetAccountAsync (long id) => (await Context.Accounts.SingleOrDefaultAsync(e => e.AccountId == id)) ?? throw new KeyNotFoundException();
		internal async Task<DB.Models.Account> AddAccountAsync (DB.Models.Account account)
		{
			var result = Context.Accounts.Add(account);
			await Context.SaveChangesAsync();
			return result.Entity;
		}

		Task<string> IAccountManager.GetNicknameAsync (long id) => GetNicknameAsync(id);
		async Task<Account> IAccountManager.SetNicknameAsync (long id, string nickname)
		{
			var account = Authorize(await GetAccountAsync(id));
			account.Nickname = nickname;
			await Context.SaveChangesAsync();
			return account;
		}
		async Task<Account> IAccountManager.GetAccountAsync (long id) => Authorize(await GetAccountAsync(id));



		private DB.Models.Account Authorize (DB.Models.Account account)
		{
			if (Authorization.HasClaim(Permissions.ManageAccount, account)
				|| Authorization.HasAdminClaim(Permissions.AdministerAccounts))
			{
				return account;
			}
			else
			{
				throw new UnauthorizedException<DB.Models.Account>(Authorization, account);
			}
		}

	}
}
