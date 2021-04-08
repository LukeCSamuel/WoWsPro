using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi.Models
{
	public class AccountSearch
	{
		[JsonPropertyName("nickname")]
		public string Nickname { get; set; }

		[JsonPropertyName("account_id")]
		public long AccountId { get; set; }
	}
}
