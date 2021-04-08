using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Models.Discord;

namespace WoWsPro.Shared.Models.Tournaments
{
	public class Tournament
	{
		public Tournament () { }
		public long TournamentId { get; set; }
		public long? GuildId { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Description { get; set; }
		public long OwnerAccountId { get; set; }
		public DateTime Created { get; set; }
		public long? ParticipantRoleId { get; set; }
		public long? TeamRepRoleId { get; set; }
		public string ShortDescription { get; set; }
		public bool Published { get; set; }
		public bool Archived { get; set; }

		public DiscordGuild Guild { get; set; }
		public Account Owner { get; set; }
		public DiscordRole ParticipantRole { get; set; }
		public DiscordRole TeamRepRole { get; set; }

		public ICollection<TournamentRegistrationRules> RegistrationRules { get; set; }
		public ICollection<TournamentStage> Stages { get; set; }
		public ICollection<TournamentTeam> Teams { get; set; }
	}
}
