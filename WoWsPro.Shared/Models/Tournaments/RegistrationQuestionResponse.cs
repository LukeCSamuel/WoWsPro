using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class RegistrationQuestionResponse
	{
		public RegistrationQuestionResponse () { }

		public long RegistrationQuestionResponseId { get; set; }
		public long RegistrationQuestionId { get; set; }
		public long TeamId { get; set; }
		public string Response { get; set; }

		public TournamentTeam Team { get; set; }
	}
}
