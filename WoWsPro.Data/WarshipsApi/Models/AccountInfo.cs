using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi.Models
{
	public class AccountInfo
	{
		[JsonPropertyName("last_battle_time")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? LastBattleTime { get; set; }

		[JsonPropertyName("account_id")]
		public long AccountId { get; set; }

		[JsonPropertyName("leveling_tier")]
		public int? LevelingTier { get; set; }

		[JsonPropertyName("leveling_points")]
		public int? LevelingPoints { get; set; }

		[JsonPropertyName("created_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? Created { get; set; }

		[JsonPropertyName("updated_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? Updated { get; set; }

		[JsonPropertyName("hidden_profile")]
		public bool IsHidden { get; set; }

		[JsonPropertyName("karma")]
		public int? Karma { get; set; }

		[JsonPropertyName("nickname")]
		public string Nickname { get; set; }

		[JsonPropertyName("logout_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? LogoutTime { get; set; }

		[JsonPropertyName("stats_updated_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? StatsUpdated { get; set; }
	}
}
