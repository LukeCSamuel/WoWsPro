using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WoWsPro.Data.Authorization.Claim;
using WoWsPro.Data.Authorization.Scope;

namespace WoWsPro.Data.Authorization.Model
{
	internal class AuthorizationModel
	{
		public Type Type { get; }

		List<AuthorizeAttribute> EntityAuths { get; set;  }
		Dictionary<PropertyInfo, List<AuthorizeValuesAttribute>> PropertyAuths { get; set; }

		
		public AuthorizationModel (Type applied)
		{
			Type = applied;
			EntityAuths = ((AuthorizeAttribute[])applied.GetCustomAttributes(typeof(AuthorizeAttribute), true)).ToList();
			// Verify that type is scopeable to scoped auths
			foreach (var auth in EntityAuths)
			{
				if (auth.Scope is Type scope)
				{
					if (!typeof(IScopable<>).MakeGenericType(scope).IsAssignableFrom(applied))
					{
						throw new InvalidScopeException(applied, scope);
					}
				}
			}

			// Get property authorizations
			PropertyAuths = new Dictionary<PropertyInfo, List<AuthorizeValuesAttribute>>();
			var properties = applied.GetProperties();
			foreach (var property in properties)
			{
				var auths = ((AuthorizeValuesAttribute[])property.GetCustomAttributes(typeof(AuthorizeValuesAttribute), true)).ToList();
				if (auths.Count > 0)
				{
					PropertyAuths[property] = auths;
				}
			}
		}

		public void Authorize (EntityEntry entry, IEnumerable<IClaim> claims)
		{
			var entryType = entry.Entity.GetType();
			if (entryType != Type)
			{
				throw new InvalidOperationException($"Cannot authorize an entry of type '{entryType.Name}' with this {nameof(AuthorizationModel)}.  This model is built for to '{Type.Name}'.");
			}

			var scopable = typeof(IScopable).IsAssignableFrom(entryType) ? (IScopable)entry.Entity : new NullScope();
			var action = entry.State switch
			{
				EntityState.Unchanged => Actions.Read,
				EntityState.Modified => Actions.Modify,
				EntityState.Added => Actions.Create,
				EntityState.Deleted => Actions.Delete,
				_ => Actions.Read
			};

			foreach (var claim in claims)
			{
				// Does the claim authorize the entry?
				var allowedActions = Actions.None;
				foreach (var auth in EntityAuths)
				{
					allowedActions |= auth.GetActionsDelegate(claim)(scopable);
					if ((allowedActions & action) == action)
					{
						break;
					}
				}

				if ((allowedActions & action) != action)
				{
					// Action is not permitted
					continue;
				}

				// Does the claim authorize any property changes?
				var allowed = true;
				foreach (var property in PropertyAuths.Keys)
				{
					var pEntry = entry.Property(property.Name);
					if (pEntry.IsModified)
					{
						// Check auths for property
						bool? allowedValue = null;
						foreach (var auth in PropertyAuths[property])
						{
							bool? result = auth.GetAuthorizationDelegate(claim)(scopable, pEntry.CurrentValue);
							if (result is true)
							{
								allowedValue = result;
								break;
							}
							allowedValue = result ?? allowedValue;
						}
						if (allowedValue is false)
						{
							allowed = false;
							break;
						}
					}
				}
				if (!allowed)
				{
					continue;
				}

				// Successfully found an authorization for the action
				return;
			}

			throw new UnauthorizedException()
			{
				Action = action,
				Entity = entry.Entity
			};
		}

		private class NullScope : IScopable
		{
			public IScope ScopeInstance => null;
		}

	}
}
