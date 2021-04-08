using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class TournamentStage 
	{
		public TournamentStage() { }

		public long StageId { get; set; }
		public long TournamentId { get; set; }
		public string Name { get; set; }
		public int Capacity { get; set; }
		public Format Format { get; set; }
		public int Sequence { get; set; }

		public Tournament Tournament { get; set; }

		public ICollection<TournamentGroup> Groups { get; set; }

	}
}
