using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WoWsPro.Shared.Models
{
	public class Account
	{
		public Account () { }

		public long AccountId { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; set; }


		public ICollection<DiscordUser> DiscordAccounts { get; set; }
		public ICollection<WarshipsPlayer> WarshipsAccounts { get; set; }
		public ICollection<Tournament> OwnedTournaments { get; set; }
		public ICollection<TournamentTeam> OwnedTeams { get; set; }

		// TODO: Navigation properties

		public DiscordUser GetPrimaryDiscordAccount () => DiscordAccounts.SingleOrDefault(e => e.IsPrimary);
		public WarshipsPlayer GetPrimaryWarshipsAccount () => WarshipsAccounts.SingleOrDefault(e => e.IsPrimary);
	}
}
