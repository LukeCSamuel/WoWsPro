using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class Account : IScope
	{
		public Account()
		{
			DiscordAccounts = new HashSet<DiscordUser>();
			WarshipsAccounts = new HashSet<WarshipsPlayer>();
			OwnedTournaments = new HashSet<Tournament>();
			AccountClaims = new HashSet<AccountClaim>();
			TournamentClaims = new HashSet<TournamentClaim>();
			OwnedTeams = new HashSet<TournamentTeam>();
		}

		public long AccountId { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; set; }


		public virtual ICollection<DiscordUser> DiscordAccounts { get; set; }
		public virtual ICollection<WarshipsPlayer> WarshipsAccounts { get; set; }
		public virtual ICollection<Tournament> OwnedTournaments { get; set; }
		public virtual ICollection<TournamentTeam> OwnedTeams { get; set; }

		[JsonIgnore]
		public virtual ICollection<AccountClaim> AccountClaims { get; set; }
		[JsonIgnore]
		public virtual ICollection<TournamentClaim> TournamentClaims { get; set; }
		[JsonIgnore]
		public virtual ICollection<TournamentTeamClaim> TeamClaims { get; set; }

		[NotMapped]
		long IScope.ScopeId => AccountId;

		public IEnumerable<IClaim> GetClaims (Type scope)
		{
			if (scope == typeof(Account))
			{
				return AccountClaims;
			}
			else if (scope == typeof(Tournament))
			{
				return TournamentClaims;
			}
			else if (scope == typeof(TournamentTeam))
			{
				return TeamClaims;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public ICollection<IClaim<T>> GetClaims<T> () where T : IScope
		{
			if (typeof(T) == typeof(Account))
			{
				return (ICollection<IClaim<T>>)AccountClaims;
			}
			else if (typeof(T) == typeof(Tournament))
			{
				return (ICollection<IClaim<T>>)TournamentClaims;
			}
			else if (typeof(T) == typeof(TournamentTeam))
			{
				return (ICollection<IClaim<T>>)TeamClaims;
			}
			else
			{
				throw new NotImplementedException();
			}
		}


		public static implicit operator Shared.Models.Account (Account account) => account.ConvertObject<Account, Shared.Models.Account>();
		public static implicit operator Account (Shared.Models.Account account) => account.ConvertObject<Shared.Models.Account, Account>();

	}
}
