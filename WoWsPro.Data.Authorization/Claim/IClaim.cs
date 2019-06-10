using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.Authorization.Scope;

namespace WoWsPro.Data.Authorization.Claim
{
	public interface IClaim : IScope
	{
		string Permission { get; }
	}

	public interface IClaim<T> : IClaim, IScope<T> where T : IScope<T> { }
}
