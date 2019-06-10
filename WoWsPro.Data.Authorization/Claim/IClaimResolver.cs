using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization.Claim
{
	public interface IClaimResolver
	{
		IEnumerable<IClaim> GetClaims ();
	}
}
