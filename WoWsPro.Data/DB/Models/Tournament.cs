using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class Tournament : IScope
	{
		public Tournament()
		{
			Claims = new HashSet<TournamentClaim>();
			RegistrationRules = new HashSet<TournamentRegistrationRules>();
			Stages = new HashSet<TournamentStage>();
			Teams = new HashSet<TournamentTeam>();
		}

		public long TournamentId { get; set; }
		public long? GuildId { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
		public string Description { get; set; }
		public long OwnerAccountId { get; set; }
		public DateTime Created { get; set; }
		public long? ParticipantRoleId { get; set; }
		public long? TeamOwnerRoleId { get; set; }


		public virtual DiscordGuild Guild { get; set; }
		public virtual Account Owner { get; set; }
		public virtual DiscordRole ParticipantRole { get; set; }
		public virtual DiscordRole TeamOwnerRole { get; set; }

		[JsonIgnore]
		public virtual ICollection<TournamentClaim> Claims { get; set; }
		public virtual ICollection<TournamentRegistrationRules> RegistrationRules { get; set; }
		public virtual ICollection<TournamentStage> Stages { get; set; }
		public virtual ICollection<TournamentTeam> Teams { get; set; }

		[NotMapped]
		long IScope.ScopeId => TournamentId;

		public static implicit operator Shared.Models.Tournament (Tournament tournament) => tournament.ConvertObject<Tournament, Shared.Models.Tournament>();
		public static implicit operator Tournament (Shared.Models.Tournament tournament) => tournament.ConvertObject<Shared.Models.Tournament, Tournament>();
	}
}
