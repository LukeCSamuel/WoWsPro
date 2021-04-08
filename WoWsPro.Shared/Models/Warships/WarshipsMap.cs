using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Shared.Models.Warships
{
	public class WarshipsMap
	{
		public WarshipsMap () { }

		public long MapId { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public string Name { get; set; }

		public ICollection<TournamentGame> Games { get; set; }

	}
}
