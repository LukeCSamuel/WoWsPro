using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;

namespace WoWsPro.Data.Services
{
	public class AccountManager : Manager<AccountOperations>
	{
		internal override Type ScopeType { get; } = typeof(DB.Models.Account);
		internal override AccountOperations Instance { get; }
		internal override Context Context { get; }
		internal override long? UserId { get; }

		public AccountManager (IAuthenticator authenticator, Context context)
		{
			Context = context;
			UserId = authenticator.AccountId;
			Instance = new AccountOperations(this);
		}
	}

	public static class AccountManagerProvider
	{
		public static IServiceCollection AddAccountManager (this IServiceCollection services)
			=> services.AddAuthorizer<AccountManager, AccountOperations>();
	}
}

namespace WoWsPro.Data.Operations
{
	public partial class AccountOperations : Operations<AccountOperations>
	{
		internal AccountOperations (Manager<AccountOperations> manager) => Manager = manager;
	}
}