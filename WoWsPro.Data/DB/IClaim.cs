using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.DB
{
	internal interface IClaim<T> where T : IScope
	{
		string Permission { get; }
		long AccountId { get; }
		T Scope { get; }
		long ScopedId => Scope.ScopedId;
	}

	internal interface IScope
	{
		long ScopedId { get; }
	}
}
