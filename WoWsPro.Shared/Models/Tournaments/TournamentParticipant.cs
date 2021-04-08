using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Warships;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class TournamentParticipant
	{
		public TournamentParticipant () { }

		public long ParticipantId { get; set; }
		public long TeamId { get; set; }
		public long PlayerId { get; set; }
		public bool TeamRep { get; set; }
		public ParticipantStatus Status { get; set; }

		public TournamentTeam Team { get; set; }
		public WarshipsPlayer Player { get; set; }
	}
}
