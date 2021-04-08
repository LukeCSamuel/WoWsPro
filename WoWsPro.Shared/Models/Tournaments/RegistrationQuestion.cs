using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class RegistrationQuestion
	{
		public RegistrationQuestion () { }

		public long RegistrationQuestionId { get; set; }
		public long TournamentRegistrationRulesId { get; set; }
		public string Prompt { get; set; }
		public RegistrationQuestionType QuestionType { get; set; }
		public bool IsRequired { get; set; }
		public string Options { get; set; }

		public TournamentRegistrationRules TournamentRegistrationRules { get; set; }
	}
}
