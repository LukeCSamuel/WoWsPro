using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WoWsPro.Data.Operations;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Server.Extensions;
using WoWsPro.Shared.Models;
using WoWsPro.Shared.Models.Discord;
using WoWsPro.Shared.Models.Warships;
using WoWsPro.Shared.Permissions;

namespace WoWsPro.Server.Services
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

		public static User None => new User();

		public override string ToString () => IsLoggedIn ? PreferredName : "Logged Out";
	}

	public interface IUserService : IUserProvider
	{
		ISession Session { get; }
		User User { get; }
		string PreferredName { get; }

		void Login (DiscordToken token);
		void Login (WarshipsPlayer player);
		void Logout ();
	}

	public class UserService : IUserService
	{
		private const string _cookieName = ".WoWsPro.Auth";

		public ISession Session { get; }
		IRequestCookieCollection RequestCookies { get; }
		IResponseCookies ResponseCookies { get; }

		public User User
		{
			get => GetUser();
			set => SetUser(value);
		}
		private User _user = null;

		AdminAccountOperations AccountOps { get; }

		public UserService (IHttpContextAccessor contextAccessor, AdminAccountOperations accountOps)
		{
			Session = contextAccessor.HttpContext.Session;
			RequestCookies = contextAccessor.HttpContext.Request.Cookies;
			ResponseCookies = contextAccessor.HttpContext.Response.Cookies;
			AccountOps = accountOps;
		}

		public bool IsLoggedIn => User.IsLoggedIn;
		public string PreferredName => User.PreferredName;

		private Account _account = null;
		public Account Account => _account ??= User.AccountId is long accountId ? AccountOps.GetAccount(accountId) : null;

		public void Logout () => User = User.None;

		public void Login (WarshipsPlayer player)
		{
			if (IsLoggedIn)
			{
				AccountOps.AddOrMergeAccount((long)User.AccountId, player);
			}
			else
			{
				var (nickname, accountId) = AccountOps.GetOrCreateAccount(player);
				User = new User(nickname, accountId);
			}
		}

		public void Login (DiscordToken token)
		{
			if (IsLoggedIn)
			{
				AccountOps.AddOrMergeAccount((long)User.AccountId, token);
			}
			else
			{
				var (nickname, accountId) = AccountOps.GetOrCreateAccount(token);
				User = new User(nickname, accountId);
			}
		}

		private User GetUser ()
		{
			if (_user is User)
			{
				return _user;
			}

			// Verify cookie has token
			var cookie = RequestCookies[_cookieName];
			if (cookie is string)
			{
				var token = CookieToken.FromString(RequestCookies[_cookieName]);
				if (token.User?.AccountId is long accountId) {
					bool isValid = AccountOps.VerifyToken(accountId, token.Token);
					if (isValid)
					{
						// Replace the old cookie with a new one
						SetUser(token.User);
						return _user;
					}
				}

				// Invalid cookie, delete the cookie
				ResponseCookies.Delete(_cookieName);
			}

			_account = null;
			return _user = User.None;
		}

		private void SetUser (User user)
		{
			if (!user.IsLoggedIn)
			{
				// Delete the cookie if set to the null user
				ResponseCookies.Delete(_cookieName);
				_user = user;
				_account = null;
			}
			else if (_user?.AccountId != user.AccountId)
			{
				// Only set the user if it's not already been set.
				CookieToken.AppendCookie(AccountOps, ResponseCookies, user);
				_user = user;
				_account = null;
			}
		}

		private class CookieToken
		{
			public Guid Token { get; set; }
			public User User { get; set; }

			public static CookieToken FromString (string cookie)
			{
				// Get token from Base64
				return JsonSerializer.Deserialize<CookieToken>(Encoding.UTF8.GetString(Convert.FromBase64String(cookie)));
			}

			public static void AppendCookie (AdminAccountOperations accountOps, IResponseCookies jar, User user)
			{
				if (user.AccountId is long accountId)
				{
					var token = new CookieToken()
					{
						User = user,
						Token = Guid.NewGuid()
					};
					accountOps.InsertToken(accountId, token.Token);
					// Convert token to Base64
					var cookieValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(token)));
					jar.Append(_cookieName, cookieValue, new CookieOptions()
					{
						MaxAge = AdminAccountOperations.TokenExpiration,
						Expires = DateTime.UtcNow + AdminAccountOperations.TokenExpiration,
						IsEssential = true,
						Secure = true
					});
				}
			}
		}
	}

	public static class UserServiceProvider
	{
		public static IServiceCollection AddUserService (this IServiceCollection services) 
			=> services
				.AddScoped<IUserService, UserService>()
				.AddScoped<IUserProvider, UserService>();
	}
}
