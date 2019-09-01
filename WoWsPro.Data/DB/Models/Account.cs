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

		public virtual ICollection<AccountClaim> AccountClaims { get; set; }
		public virtual ICollection<TournamentClaim> TournamentClaims { get; set; }
		public virtual ICollection<TournamentTeamClaim> TeamClaims { get; set; }

		[NotMapped]
		long IScope.ScopedId => AccountId;


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



		public static implicit operator Shared.Models.Account (Account account)
		{
			return new Shared.Models.Account()
			{
				AccountId = account.AccountId,
				Created = account.Created,
				Nickname = account.Nickname,
				DiscordAccounts = account.DiscordAccounts.Select<DiscordUser, Shared.Models.DiscordUser>(e => e).ToHashSet(),
				WarshipsAccounts = account.WarshipsAccounts.Select<WarshipsPlayer, Shared.Models.WarshipsPlayer>(e => e).ToHashSet()
			};
		}

		public static implicit operator Account (Shared.Models.Account account)
		{
			return new Account()
			{
				AccountId = account.AccountId,
				Created = account.Created,
				Nickname = account.Nickname,
				DiscordAccounts = account.DiscordAccounts.Select<Shared.Models.DiscordUser, DiscordUser>(e => e).ToHashSet(),
				WarshipsAccounts = account.WarshipsAccounts.Select<Shared.Models.WarshipsPlayer, WarshipsPlayer>(e => e).ToHashSet()
			};
		}

	}
}
