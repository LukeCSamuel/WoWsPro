using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Server.Extensions;

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
		// TODO: public static User FromAccount (WoWsPro.Shared.Models.Account account) => throw new NotImplementedException();

		public override string ToString () => IsLoggedIn ? PreferredName : "Logged Out";
	}

	public interface IUserService
	{
		User User { get; }
		bool IsLoggedIn { get; }
		string PreferredName { get; }

		// TODO: WoWsPro.Shared.Models.Account Account { get; }
		// TODO: void Login (WoWsPro.Shared.Models.WarshipsPlayer player)
		// TODO: void Login (WoWsPro.Shared.Models.DiscordUser user)

		// TODO: Remove once data-driven login is complete
		void TestLogin (long id, string nickname);

		void Logout ();
	}

	public class UserService : IUserService
	{
		private readonly ISession _session;

		public User User
		{
			get => _user ?? (_user = _session.Retrieve<User>());
			set => _session.Store(_user = value);
		}
		private User _user = null;

		public UserService (IHttpContextAccessor contextAccessor)
		{
			_session = contextAccessor.HttpContext.Session;
		}

		public bool IsLoggedIn => User.IsLoggedIn;
		public string PreferredName => User.PreferredName;

		public void Logout () => User = User.None;

		// TODO: Remove once data-driven login is complete
		public void TestLogin (long id, string nickname) => User = new User(nickname, id);
	}

	public static class UserServiceExtensions
	{
		public static IServiceCollection AddUserService (this IServiceCollection services) 
			=> services.AddScoped<IUserService, UserService>();
	}
}
