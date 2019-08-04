using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class TournamentStage 
	{
		public TournamentStage()
		{
			Groups = new HashSet<TournamentGroup>();
		}

		public long StageId { get; set; }
		public long TournamentId { get; set; }
		public string Name { get; set; }
		public int Capacity { get; set; }
		public Format Format { get; set; }
		public int Sequence { get; set; }

		public virtual Tournament Tournament { get; set; }

		public virtual ICollection<TournamentGroup> Groups { get; set; }

	}
}
