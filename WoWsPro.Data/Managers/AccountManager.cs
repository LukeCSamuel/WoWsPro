using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Data.Services;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;

namespace WoWsPro.Data.Managers
{
	public class AccountManager
	{
		IContextAuthorization Authorization { get; }
		Context Context => Authorization.Context;

		public AccountManager (IContextAuthorization auth)
		{
			Authorization = auth;
		}

		// TODO: Asyncify this class
		// TODO: Find points of repitition which can be abstracted out

		public string GetNickname (long id) => Fetch(id)?.Nickname ?? throw new KeyNotFoundException;

		public void SetNickname (long id, string nickname)
		{
			var account = Fetch(id);

			if (account != null)
			{
				Authorize(account);
				account.Nickname = nickname;
				Context.SaveChanges();
			}
			else
			{
				throw new KeyNotFoundException();
			}
		}

		public Account GetAccount (long id)
		{
			var account = Fetch(id);

			if (account != null)
			{
				return Authorize(account);
			}
			else
			{
				throw new KeyNotFoundException();
			}
		}

		private Account Authorize (DB.Models.Account account)
		{
			if (Authorization.HasClaim(Permissions.ManageAccount)
				|| Authorization.HasClaim(Permissions.AdministerAccounts))
			{
				return account;
			}
			else
			{
				throw new UnauthorizedException<DB.Models.Account>(Authorization, account);
			}
		}

		private DB.Models.Account Fetch (long id) => Context.Accounts.SingleOrDefault(a => a.AccountId == id);

	}
}
