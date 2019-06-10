using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization.Scope
{
	public interface IScope
	{
		long? ScopedId { get; }
		Type Scope { get; }
	}

	public interface IScope<T> : IScope where T : IScope<T> {	}
}
