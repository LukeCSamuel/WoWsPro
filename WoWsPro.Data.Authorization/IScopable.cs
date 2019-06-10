using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.Authorization.Scope;

namespace WoWsPro.Data.Authorization
{
	public interface IScopable
	{
		IScope ScopeInstance { get; }
	}

	public interface IScopable<T> : IScopable where T : IScope
	{
		new T ScopeInstance { get; }
		IScope IScopable.ScopeInstance => ScopeInstance;
	}
}
