using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.DB.Models
{
	internal partial class WarshipsPlayer
	{
		public WarshipsPlayer ()
		{
			Participations = new HashSet<TournamentParticipant>();
		}

		public long PlayerId { get; set; }
		public long? AccountId { get; set; }
		public Region Region { get; set; }
		public string Nickname { get; set; }
		public DateTime? Created { get; set; }
		public long? ClanId { get; set; }
		public string ClanRole { get; set; }
		public DateTime? JoinedClan { get; set; }
		public bool IsPrimary { get; set; }

		public virtual Account Account { get; set; }
		public virtual WarshipsClan Clan { get; set; }

		public virtual ICollection<TournamentParticipant> Participations { get; set; }


		public static implicit operator Shared.Models.WarshipsPlayer (WarshipsPlayer player) => new Shared.Models.WarshipsPlayer()
		{
			PlayerId = player.PlayerId,
			AccountId = player.AccountId,
			Region = player.Region,
			Nickname = player.Nickname,
			Created = player.Created,
			ClanId = player.ClanId,
			ClanRole = player.ClanRole,
			JoinedClan = player.JoinedClan,
			IsPrimary = player.IsPrimary,
			Account = player.Account
		};

		public static implicit operator WarshipsPlayer (Shared.Models.WarshipsPlayer player) => new WarshipsPlayer()
		{
			PlayerId = player.PlayerId,
			AccountId = player.AccountId,
			Region = player.Region,
			Nickname = player.Nickname,
			Created = player.Created,
			ClanId = player.ClanId,
			ClanRole = player.ClanRole,
			JoinedClan = player.JoinedClan,
			IsPrimary = player.IsPrimary,
			Account = player.Account
		};

	}
}
