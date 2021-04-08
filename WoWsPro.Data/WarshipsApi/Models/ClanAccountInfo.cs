using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi.Models
{
	public class ClanAccountInfo
	{
		[JsonPropertyName("role")]
		public string Role { get; set; }

		[JsonPropertyName("clan_id")]
		public long? ClanId { get; set; }

		[JsonPropertyName("joined_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? JoinedAt { get; set; }

		[JsonPropertyName("account_id")]
		public long AccountId { get; set; }

		[JsonPropertyName("account_name")]
		public string Nickname { get; set; }

	}
}
