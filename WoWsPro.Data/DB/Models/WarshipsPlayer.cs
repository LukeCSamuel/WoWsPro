using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class WarshipsPlayer
	{
		public WarshipsPlayer ()
		{
			Participations = new HashSet<TournamentParticipant>();
		}

		public long PlayerId { get; set; }
		public long? AccountId { get; set; }
		public Region Region { get; set; }
		public string Nickname { get; set; }
		public DateTime? Created { get; set; }
		public long? ClanId { get; set; }
		public string ClanRole { get; set; }
		public DateTime? JoinedClan { get; set; }
		public bool IsPrimary { get; set; }

		public virtual Account Account { get; set; }
		public virtual WarshipsClan Clan { get; set; }

		public virtual ICollection<TournamentParticipant> Participations { get; set; }
	}
}
