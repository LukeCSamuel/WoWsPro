using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.Authorization.Claim;
using WoWsPro.Data.Authorization.Scope;

namespace WoWsPro.Data.Authorization
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class AuthorizeAttribute : Attribute
	{
		public Actions Actions { get; }
		public Type Scope
		{
			get => _scope;
			set => _scope = typeof(IScope).IsAssignableFrom(value) ? value : throw new InvalidScopeException(value);
		}
		private Type _scope = null;

		public string Permission { get; }

		public AuthorizeAttribute (Actions actions, string permission)
		{
			Actions = actions;
			Permission = permission;
		}

		public Func<IScopable, Actions> GetActionsDelegate (IClaim claim) => entity => claim.Permission == Permission && Scope.IsInScope(entity, claim) ? Actions : Actions.None;
	}
}
