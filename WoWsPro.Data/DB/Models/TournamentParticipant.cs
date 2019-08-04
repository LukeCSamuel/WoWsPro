using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class TournamentParticipant
	{
		public TournamentParticipant () { }

		public long ParticipantId { get; set; }
		public long TeamId { get; set; }
		public long PlayerId { get; set; }
		public ParticipantStatus Status { get; set; }

		public virtual TournamentTeam Team { get; set; }
		public virtual WarshipsPlayer Player { get; set; }
	}
}
