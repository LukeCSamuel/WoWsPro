using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Shared.Models
{
	public class TournamentTeam
	{
		public TournamentTeam () { }

		public long TeamId { get; set; }
		public long TournamentId { get; set; }

		[Required]
		[MinLength(5, ErrorMessage = "Team name must be at least 5 characters.")]
		[MaxLength(255, ErrorMessage = "Team name must be no more than 255 characters.")]
		public string Name { get; set; }

		[Required]
		[MinLength(2, ErrorMessage = "Tag must be at least 2 characters.")]
		[MaxLength(5, ErrorMessage = "Tag must be no more than 5 characters.")]
		public string Tag { get; set; }

		[MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
		public string Description { get; set; }

		[MaxLength(255, ErrorMessage = "Logo URL cannot exceed 255 characters.")]
		public string Icon { get; set; }
		public long OwnerAccountId { get; set; }

		public TeamStatus Status { get; set; }
		public Region Region { get; set; }
		public DateTime Created { get; set; }

		public Tournament Tournament { get; set; }
		public Account Owner { get; set; }

		public ICollection<TournamentParticipant> Participants { get; set; }
	}
}
