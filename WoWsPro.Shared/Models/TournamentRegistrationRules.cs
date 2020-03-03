﻿using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models
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
		public long? RegionParticipantRoleId { get; set; }
		public long? RegionTeamOwnerRoleId { get; set; }


		public Tournament Tournament { get; set; }
		public DiscordRole RegionParticipantRole { get; set; }
		public DiscordRole RegionTeamOwnerRole { get; set; }
	}
}
