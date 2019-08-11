using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Models
{
	public class Account
	{
		public Account () { }

		public long AccountId { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; set; }

		// TODO: Navigation properties
	}
}
