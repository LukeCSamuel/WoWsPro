using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.Authorization;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.Read, Permissions.Default)]
	[Authorize(Actions.All, Permissions.AdministerWGData)]
	internal partial class WarshipsClan
	{
		public WarshipsClan()
		{
			Players = new HashSet<WarshipsPlayer>();
		}

		public long ClanId { get; set; }
		public Region Region { get; set; }
		public string Name { get; set; }
		public string Tag { get; set; }
		public int MemberCount { get; set; }
		public DateTime Created { get; set; }

		public virtual ICollection<WarshipsPlayer> Players { get; set; }
	}
}
