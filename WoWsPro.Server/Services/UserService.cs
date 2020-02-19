using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;
using WoWsPro.Server.Extensions;
using WoWsPro.Shared.Models;

namespace WoWsPro.Server.Services
{
	public class User
	{
		[SessionVar]
		public bool IsLoggedIn { get; set; }
		[SessionVar]
		public string PreferredName { get; set; }
		[SessionVar]
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

		public static User None => new User();

		public override string ToString () => IsLoggedIn ? PreferredName : "Logged Out";
	}

	public interface IUserService
	{
		User User { get; }
		bool IsLoggedIn { get; }
		string PreferredName { get; }

		Account Account { get; }
		// TODO: void Login (WoWsPro.Shared.Models.DiscordUser user)

		void Login (WarshipsPlayer player);
		void Logout ();
	}

	public class UserService : IUserService, IAuthenticator
	{
		private readonly ISession _session;

		public User User
		{
			get => _user ?? (_user = _session.Retrieve<User>());
			set => _session.Store(_user = value);
		}
		private User _user = null;

		AdminAccountOperations AccountOps { get; }

		public UserService (IHttpContextAccessor contextAccessor, AdminAccountOperations accountOps)
		{
			_session = contextAccessor.HttpContext.Session;
			AccountOps = accountOps;
		}

		public bool IsLoggedIn => User.IsLoggedIn;
		public string PreferredName => User.PreferredName;

		bool IAuthenticator.LoggedIn => User.IsLoggedIn;
		long? IAuthenticator.AccountId => User.AccountId;

		public Account Account => User.AccountId is long accountId ? AccountOps.GetAccount(accountId) : null;

		public void Logout () => User = User.None;

		public void Login (WarshipsPlayer player)
		{
			if (IsLoggedIn)
			{
				AccountOps.AddOrMergeWarshipsAccount((long)User.AccountId, player);
			}
			else
			{
				var (nickname, accountId) = AccountOps.GetOrCreateAccount(player);
				User = new User(nickname, accountId);
			}
		}


	}

	public static class UserServiceExtensions
	{
		public static IServiceCollection AddUserService (this IServiceCollection services) 
			=> services.AddScoped<IUserService, UserService>();
	}
}
