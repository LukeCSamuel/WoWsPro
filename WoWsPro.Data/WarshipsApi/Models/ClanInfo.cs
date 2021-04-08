using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi.Models
{
	public class ClanInfo
	{
		[JsonPropertyName("members_count")]
		public int MemberCount { get; set; }
		
		[JsonPropertyName("name")]
		public string Name { get; set; }
		
		[JsonPropertyName("creator_name")]
		public string CreatorName { get; set; }
		
		[JsonPropertyName("clan_id")]
		public long ClanId { get; set; }

		[JsonPropertyName("created_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? CreatedAt { get; set; }
		
		[JsonPropertyName("updated_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? UpdatedAt { get; set; }

		[JsonPropertyName("leader_name")]
		public string LeaderName { get; set; }

		[JsonPropertyName("members_ids")]
		public List<long> MembersIds { get; set; }
		
		[JsonPropertyName("creator_id")]
		public long CreatorId { get; set; }
		
		[JsonPropertyName("tag")]
		public string Tag { get; set; }

		[JsonPropertyName("old_name")]
		public string OldName { get; set; }

		[JsonPropertyName("is_clan_disbanded")]
		public bool IsDisbanded { get; set; }
		
		[JsonPropertyName("renamed_at")]
		[JsonConverter(typeof(JsonEpochConverter))]
		public DateTime? RenamedAt { get; set; }
		
		[JsonPropertyName("old_tag")]
		public string OldTag { get; set; }

		[JsonPropertyName("leader_id")]
		public long LeaderId { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }
	}
}
