using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.Authorization.Claim;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.AdministerAccounts)]
	internal partial class AccountClaim : IClaim<Account>
	{
		public AccountClaim () { }
		
		public long AccountId { get; set; }
		public long ClaimId { get; set; }

		public virtual Account Account { get; set; }
		public virtual Claim Claim { get; set; }

		[NotMapped]
		public string Permission => Claim.Permission;
		[NotMapped]
		public long? ScopedId => AccountId;
		[NotMapped]
		public Type Scope => typeof(Account);
	}
}
