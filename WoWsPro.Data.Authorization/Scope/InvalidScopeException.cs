using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization.Scope
{
	public class InvalidScopeException : Exception
	{
		public InvalidScopeException (Type type) : base($"The type '{type.Name}' does not implement the interface '{typeof(IScope).Name}'.") { }
		public InvalidScopeException (Type applied, Type scope) : base($"The type '{applied.Name}' does not implement the interface '{typeof(IScopable<>).MakeGenericType(scope).Name}'.") { }
	}
}
