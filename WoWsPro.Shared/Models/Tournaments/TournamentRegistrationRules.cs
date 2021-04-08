using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Discord;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class TournamentRegistrationRules
	{
		public TournamentRegistrationRules () { }

		public long TournamentRegistrationRulesId { get; set; }
		public long TournamentId { get; set; }
		public Region Region { get; set; }
		public DateTime Open { get; set; }
		public DateTime Close { get; set; }
		public int Capacity { get; set; }
		public int MinTeamSize { get; set; }
		public int MaxTeamSize { get; set; }
		public RegistrationRules Rules { get; set; }
		public int? MinReps { get; set; }
		public int? MaxReps { get; set; }
		public long? RegionParticipantRoleId { get; set; }
		public long? RegionRepRoleId { get; set; }


		public Tournament Tournament { get; set; }
		public DiscordRole RegionParticipantRole { get; set; }
		public DiscordRole RegionRepRole { get; set; }
		
		public ICollection<RegistrationQuestion> RegistrationQuestions { get; set; }

		public override string ToString ()
		{
			var builder = new StringBuilder();
			builder.Append($"\n- Minimum number of players: **{MinTeamSize}**");
			builder.Append($"\n- Maximum number of players: **{MaxTeamSize}**");
			if (MinReps is int) builder.Append($"\n- Minimum number of Team Reps: **{MinReps}**");
			if (MaxReps is int) builder.Append($"\n- Maximum number of Team Reps: **{MaxReps}**");
			if (Rules.HasFlag(RegistrationRules.RequireParticipantsDiscord)) builder.Append($"\n- **All players** must have a **linked Discord** account.");
			else if (Rules.HasFlag(RegistrationRules.RequireTeamRepsDiscord)) builder.Append($"\n- **Team Reps** must have a **linked Discord** account.");
			if (Rules.HasFlag(RegistrationRules.AllowRosterChanges)) builder.Append($"\n- Roster changes are allowed after registration closes.");

			return builder.ToString();
		}
	}
}
