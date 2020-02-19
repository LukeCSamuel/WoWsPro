using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using WoWsPro.Data.Attributes;
using WoWsPro.Data.DB;
using WoWsPro.Data.DB.Models;
using WoWsPro.Data.Exceptions;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.Services
{
	/// <summary>
	/// Represents the authenticated user, if available
	/// </summary>
	public interface IAuthenticator
	{
		bool LoggedIn { get; }
		long? AccountId { get; }
	}

	/// <summary>
	/// Describes an Authorizer object
	/// </summary>
	public interface IAuthorizer<T>
	{
		Manager<T> Manager { get; }
		AuthResult<S> Do<S> (Expression<Func<T, S>> operation);
		AuthResult Do (Expression<Action<T>> operation);
	}

	/// <summary>
	/// Represents the result of an authorization attempt
	/// </summary>
	public class AuthResult
	{
		public bool Success { get; }
		public UnauthorizedException Exception { get; }

		protected internal AuthResult (bool success, UnauthorizedException exception)
		{
			Success = success;
			Exception = exception;
		}

		internal AuthResult () : this(success: true, exception: null) { }

		internal AuthResult (UnauthorizedException exception) : this(success: false, exception) { }
	}

	/// <summary>
	/// Represents the result of an authorization attempt
	/// </summary>
	public class AuthResult<S> : AuthResult
	{
		public S Result { get; }

		private AuthResult (bool success, UnauthorizedException exception, S result) : base(success, exception)
		{
			Result = result;
		}

		internal AuthResult (UnauthorizedException exception) : this(success: false, exception, result: default) { }

		internal AuthResult (S result) : this(success: true, exception: null, result) { }

		public static explicit operator S (AuthResult<S> data) => data.Success ? data.Result : throw new InvalidCastException();
	}

	/// <summary>
	/// Verifies that the current user is authorized to access a particular resource
	/// </summary>
	public class Authorizer<M, T> : IAuthorizer<T> where M : Manager<T>
	{
		// ONEDAY: write this asynchronously
		// ONEDAY: generate data structure of methods and their permissions to use less reflection

		M Manager { get; }
		Manager<T> IAuthorizer<T>.Manager => Manager;

		Context Context { get; }
		long? UserId { get; }

		Account UserAccount { get; }

		public Authorizer (M manager, Context context, IAuthenticator authenticator)
		{
			Manager = manager;
			Context = context;
			UserId = authenticator.AccountId;

			UserAccount = Context.Accounts.SingleOrDefault(a => a.AccountId == UserId);
		}


		internal IEnumerable<IClaim> GetClaims ()
			=> UserAccount?.GetClaims(Manager.ScopeType).Where(c => c.ScopeId == Manager.CastScopeId()) ?? new List<IClaim>();

		internal bool HasClaim<R> (IPermission permission) where R : IScope
			=> GetClaims().Any(c => c.Permission == permission.Permission);


		public AuthResult<R> Do<R> (Expression<Func<T, R>> operation)
		{
			string unauth = string.Join("\r\n", GetUnauthorizedMethods(operation));
			if (!string.IsNullOrEmpty(unauth))
			{
				return new AuthResult<R>(new UnauthorizedException(new UnauthorizedException(unauth), UserId, Manager.CastScopeId(), Manager.ScopeType));
			}

			try
			{
				var result = operation.Compile()(Manager.Instance);
				return new AuthResult<R>(result);
			}
			catch (UnauthorizedException ex)
			{
				return new AuthResult<R>(new UnauthorizedException(ex, UserId, Manager.CastScopeId(), Manager.ScopeType));
			}
		}

		public AuthResult Do (Expression<Action<T>> operation)
		{
			string unauth = string.Join("\r\n", GetUnauthorizedMethods(operation));
			if (!string.IsNullOrEmpty(unauth))
			{
				return new AuthResult(new UnauthorizedException(new UnauthorizedException(unauth), UserId, Manager.CastScopeId(), Manager.ScopeType));
			}

			try
			{
				operation.Compile()(Manager.Instance);
				return new AuthResult();
			}
			catch (UnauthorizedException ex)
			{
				return new AuthResult(new UnauthorizedException(ex, UserId, Manager.CastScopeId(), Manager.ScopeType));
			}
		}

		public IEnumerable<string> GetUnauthorizedMethods (LambdaExpression operation)
		{
			var authr = new AuthorizeMethodVisitor(GetClaims().Select(c => (Permission)c.Permission));
			authr.Visit(operation.Body);
			return authr.UnauthorizedCalls;
		}

		/// <summary>
		/// Visits an expression to determine if it contains unauthorized method calls
		/// </summary>
		class AuthorizeMethodVisitor : ExpressionVisitor
		{
			public List<string> UnauthorizedCalls { get; } = new List<string>();
			IEnumerable<Permission> Permissions { get; }

			public AuthorizeMethodVisitor (IEnumerable<Permission> permissions) => Permissions = permissions;

			protected override Expression VisitMethodCall (MethodCallExpression node)
			{
				if (!IsMethodAuthorized(node, Permissions))
				{
					UnauthorizedCalls.Add($"{node.Method.DeclaringType.FullName}..{node.Method.Name}");
				}
				return node;
			}

			bool IsMethodAuthorized (MethodCallExpression method, IEnumerable<IPermission> permissions)
			{
				if (method.Method.GetCustomAttributes(typeof(PublicAttribute), inherit: false).Length > 0)
				{
					return true;
				}

				object[] auths = method.Method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);
				if (auths.Length == 0)
				{
					return true;
				}

				foreach (var attr in (IEnumerable<AuthorizeAttribute>)auths)
				{
					if (permissions.Any(p => p.Permission == attr.Permission))
					{
						return true;
					}
				}

				return false;
			}
		}
	}

	public abstract class Manager<T>
	{
		public long? ScopeId { get; set; } = null;
		internal abstract Type ScopeType { get; }
		internal abstract T Instance { get; }
		internal abstract Context Context { get; }
		internal abstract long? UserId { get; }

		public long CastScopeId ()
		{
			if (ScopeId is long scope)
			{
				return scope;
			}
			else
			{
				throw new InvalidOperationException("Scope was not set before authorization request.");
			}
		}
	}

	public static class AuthorizerProvider
	{
		public static IServiceCollection AddAuthenticator<T> (this IServiceCollection services) where T : class, IAuthenticator
			=> services.AddScoped<IAuthenticator, T>();

		internal static IServiceCollection AddAuthorizer<M, T> (this IServiceCollection services) where M : Manager<T>
			=> services
			.AddScoped<M>()
			.AddScoped<IAuthorizer<T>, Authorizer<M, T>>()
			;
	}
}




