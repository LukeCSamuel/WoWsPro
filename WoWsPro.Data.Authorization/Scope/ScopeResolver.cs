using System;
using WoWsPro.Data.Authorization.Claim;

namespace WoWsPro.Data.Authorization.Scope
{
	internal static class ScopeResolver
	{
		public static long? GetIdForScope (this IScope scoped, Type scope)
		{
			if (scope is null)
			{
				return null;
			}

			var scopedType = typeof(IScope<>).MakeGenericType(scope);
			if (scopedType.IsAssignableFrom(scoped.GetType()))
			{
				return (long?)scopedType.GetProperty(nameof(IScope.ScopedId)).GetValue(scoped);
			}
			else if (scoped.Scope == scope)
			{
				return scoped.ScopedId;
			}
			else
			{
				return null;
			}
		}
		public static IScope GetInstanceForScope (this IScopable scoped, Type scope)
		{
			if (scope is null)
			{
				return scoped.ScopeInstance;
			}

			var scopedType = typeof(IScopable<>).MakeGenericType(scope);
			if (scopedType.IsAssignableFrom(scoped.GetType()))
			{
				return (IScope)scopedType.GetProperty(nameof(IScopable.ScopeInstance)).GetValue(scoped);
			}
			else
			{
				return null;
			}
		}

		public static bool IsInScope (this Type scope, IScopable entity, IClaim claim) => entity.GetInstanceForScope(scope)?.GetIdForScope(scope) == claim.GetIdForScope(scope);
	}
}
