using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models.Warships
{
	public class WarshipsClan
	{
		public WarshipsClan() { }

		public long ClanId { get; set; }
		public Region Region { get; set; }
		public string Name { get; set; }
		public string Tag { get; set; }
		public int MemberCount { get; set; }
		public DateTime Created { get; set; }

		public ICollection<WarshipsPlayer> Players { get; set; }
	}
}
