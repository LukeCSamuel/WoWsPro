using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class TournamentTeam : IScope
	{
		public TournamentTeam ()
		{
			Seeds = new HashSet<TournamentSeed>();
			AlphaMatches = new HashSet<TournamentMatch>();
			GamesWon = new HashSet<TournamentGame>();
			Participants = new HashSet<TournamentParticipant>();
		}

		public long TeamId { get; set; }
		public long TournamentId { get; set; }
		public string Name { get; set; }
		public string Tag { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public long OwnerAccountId { get; set; }

		public TeamStatus Status { get; set; }
		public Region Region { get; set; }
		public DateTime Created { get; set; }

		public virtual Tournament Tournament { get; set; }
		public virtual Account Owner { get; set; }
		
		public virtual ICollection<TournamentSeed> Seeds { get; set; }
		[JsonIgnore]
		public virtual ICollection<TournamentMatch> AlphaMatches { get; set; }
		[JsonIgnore]
		public virtual ICollection<TournamentMatch> BravoMatches { get; set; }
		public virtual ICollection<TournamentGame> GamesWon { get; set; }
		public virtual ICollection<TournamentParticipant> Participants { get; set; }
		[JsonIgnore]
		public virtual ICollection<TournamentTeamClaim> Claims { get; set; }

		[NotMapped]
		public ICollection<TournamentMatch> Matches => AlphaMatches.Union(BravoMatches).ToHashSet();
		[NotMapped]
		long IScope.ScopeId => TeamId;


		public static implicit operator Shared.Models.TournamentTeam (TournamentTeam team) => team.ConvertObject<TournamentTeam, Shared.Models.TournamentTeam>();
		public static implicit operator TournamentTeam (Shared.Models.TournamentTeam team) => team.ConvertObject<Shared.Models.TournamentTeam, TournamentTeam>();
	}
}
