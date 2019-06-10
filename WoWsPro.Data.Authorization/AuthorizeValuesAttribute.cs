using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WoWsPro.Data.Authorization.Claim;
using WoWsPro.Data.Authorization.Scope;

namespace WoWsPro.Data.Authorization
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class AuthorizeValuesAttribute : Attribute
	{
		public Type Scope
		{
			get => _scope;
			set => _scope = typeof(IScope).IsAssignableFrom(value) ? value : throw new InvalidScopeException(value);
		}
		private Type _scope = null;

		public string Permission { get; }

		public object[] Values { get; } = null;
		public int? RangeBegin { get; } = null;
		public int? RangeEnd { get; } = null;
		public Type AuthorizerType { get; } = null;
		public IAuthorizer Authorizer { get; } = null;


		public AuthorizeValuesAttribute (string permission) => Permission = permission;
		public AuthorizeValuesAttribute (string permission, object value) : this(permission) => Values = new object[] { value };
		public AuthorizeValuesAttribute (string permission, object[] values) : this(permission) => Values = values;
		public AuthorizeValuesAttribute (string permission, int begin, int end) : this(permission)
		{
			RangeBegin = begin;
			RangeEnd = end;
		}
		public AuthorizeValuesAttribute (string permission, Type authorizer) : this(permission)
		{
			if (typeof(IAuthorizer).IsAssignableFrom(authorizer))
			{
				AuthorizerType = authorizer;
				var constructor = authorizer.GetConstructor(Type.EmptyTypes);
				if (constructor is ConstructorInfo)
				{
					Authorizer = (IAuthorizer)constructor.Invoke(new object[] { });
				}
				else
				{
					throw new ArgumentException($"The supplied authorizer does not contain a default constructor and cannot be instantiated.");
				}
			}
			else
			{
				throw new InvalidAuthorizerException(authorizer);
			}
		}

		public bool? IsAuthorizedValue (object value)
		{
			if (Authorizer is IAuthorizer)
			{
				return Authorizer.IsAuthorized(value);
			}

			if (Values is object[])
			{
				return Values.Contains(value);
			}

			if (RangeBegin is int begin && RangeEnd is int end)
			{
				int val = Convert.ToInt32(value);
				return begin <= val && end >= val;
			}

			return true;
		}

		public Func<IScopable, object, bool?> GetAuthorizationDelegate (IClaim claim) => (entity, value) => claim.Permission == Permission && Scope.IsInScope(entity, claim) ? IsAuthorizedValue(value) : null;
	}
}
