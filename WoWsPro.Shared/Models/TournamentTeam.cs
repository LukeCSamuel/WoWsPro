using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models
{
	public class TournamentTeam
	{
		public TournamentTeam () { }

		public long TeamId { get; set; }
		public long TournamentId { get; set; }
		public string Name { get; set; }
		public string Tag { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public long OwnerAccountId { get; set; }

		public TeamStatus Status { get; set; }
		public Region Region { get; set; }
		public DateTime Created { get; set; }

		public Tournament Tournament { get; set; }
		public Account Owner { get; set; }

		public ICollection<TournamentParticipant> Participants { get; set; }
	}
}
