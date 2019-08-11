using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.DB.Models;

namespace WoWsPro.Data.Services
{
	public interface IContextAuthorization
	{
		internal long? AccountId { get; }
		internal Context Context { get; }
		internal bool HasClaim<T> (string permission, T scope) where T : IScope;
		internal bool HasClaim (string permission);
	}


	public class ContextAuthorization : IContextAuthorization
	{
		long? AccountId { get; }
		long? IContextAuthorization.AccountId => AccountId;
		Context Context { get; }
		Context IContextAuthorization.Context => Context;
		Account UserAccount { get; }


		public ContextAuthorization (IContextAuthentication authentication, IContextManager contextManager)
		{
			AccountId = authentication.AccountId;
			Context = contextManager.Context;
			UserAccount = Context.Accounts.SingleOrDefault(a => a.AccountId == AccountId);
		}

		internal IEnumerable<IClaim<T>> GetClaims<T> (T scope) where T : IScope
			=> UserAccount.GetClaims<T>().Where(c => c.ScopedId == scope?.ScopedId);

		internal bool HasClaim<T> (string permission, T scope) where T : IScope
			=> GetClaims(scope).Any(c => c.Permission == permission);

		internal bool HasClaim (string permission)
			=> HasClaim(permission, UserAccount);

		bool IContextAuthorization.HasClaim<T> (string permission, T scope) => HasClaim(permission, scope);
		bool IContextAuthorization.HasClaim (string permission) => HasClaim(permission);
	}

	public static class ContextAuthorizationProvider
	{
		public static IServiceCollection AddContextAuthorization<T> (this IServiceCollection services) where T : class, IContextAuthentication
			=> services
			.AddScoped<IContextManager, ContextManager>()
			.AddContextAuthentication<T>()
			.AddScoped<IContextAuthorization, ContextAuthorization>();
	}

	public class UnauthorizedException<T> : Exception
	{
		// TODO: add key interface for all entities
		public UnauthorizedException (IContextAuthorization auth, T entity)
			: base($"Account({auth.AccountId}) attempted unauthorized access to {typeof(T).Name}({entity})")
			{ }
	}
}
