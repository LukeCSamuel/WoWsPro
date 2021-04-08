using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Shared.Exceptions
{
	public class ScopeNotImplementedException : Exception
	{
		public string Scope { get; }
		public Type Type { get; }

		public ScopeNotImplementedException (string scope, Type type) : base($"Scope '{scope}' is not implemented for type {type.FullName}")
		{
			Scope = scope;
			Type = type;
		}
	}
}
