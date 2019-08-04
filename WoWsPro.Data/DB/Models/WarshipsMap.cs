using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class WarshipsMap
	{
		public WarshipsMap ()
		{
			Games = new HashSet<TournamentGame>();
		}

		public long MapId { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public string Name { get; set; }

		public virtual ICollection<TournamentGame> Games { get; set; }

	}
}
