using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.DB
{
	internal interface IClaim
	{
		string Permission { get; }
		long AccountId { get; }
		long ScopeId { get; }
	}
	
	internal interface IClaim<T> : IClaim where T : IScope
	{
		T Scope { get; }
		long IClaim.ScopeId => Scope.ScopeId;
	}

	internal interface IScope
	{
		long ScopeId { get; }
	}
}
