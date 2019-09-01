using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.DB.Models;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.Services
{
	public interface IContextAuthorization
	{
		internal long? AccountId { get; }
		internal Context Context { get; }
		internal bool HasClaim<T> (IPermission permission, T scope) where T : IScope;
		internal bool HasAdminClaim (IAdminPermission permission);
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

		internal bool HasClaim<T> (IPermission permission, T scope) where T : IScope
			=> GetClaims(scope).Any(c => c.Permission == permission.Permission);

		internal bool HasAdminClaim (IAdminPermission permission)
			=> HasClaim(permission, UserAccount);

		bool IContextAuthorization.HasClaim<T> (IPermission permission, T scope) => HasClaim(permission, scope);
		bool IContextAuthorization.HasAdminClaim (IAdminPermission permission) => HasAdminClaim(permission);
	}

	public static class ContextAuthorizationProvider
	{
		public static IServiceCollection AddContextAuthorization<T> (this IServiceCollection services) where T : class, IContextAuthentication
			=> services
			.AddScoped<IContextManager, ContextManager>()
			.AddContextAuthentication<T>()
			.AddScoped<IContextAuthorization, ContextAuthorization>();
	}

	public abstract class UnauthorizedException : Exception
	{
		public UnauthorizedException (string message) : base(message) { }
	}

	public class UnauthorizedException<T> : UnauthorizedException
	{
		// TODO: add key interface for all entities
		public UnauthorizedException (IContextAuthorization auth, T entity)
			: base($"Account({auth.AccountId}) attempted unauthorized access to {typeof(T).Name}({entity})")
			{ }
	}
}
