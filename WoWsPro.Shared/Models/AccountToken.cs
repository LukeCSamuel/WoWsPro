using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Shared.Models
{
	public class AccountToken
	{
		public AccountToken () { }

		public long TokenId { get; set; }
		public Guid Token { get; set; }
		public long AccountId { get; set; }
		public DateTime Created { get; set; }


		public Account Account { get; set; }
	}
}
