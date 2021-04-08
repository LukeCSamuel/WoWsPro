using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WoWsPro.Shared.Exceptions;
using WoWsPro.Shared.Models;

namespace WoWsPro.Shared.Permissions.Claims.Tournament
{
	public class EditTournamentPermissions : ClaimResolver<Models.Tournaments.Tournament>
	{
		const string siteScope = "Site";
		const string tournamentScope = "Tournament";

		public override string ClaimTitle => "EditTournamentPermissions";

		protected override bool IsClaimValid (Claim claim, Models.Tournaments.Tournament context)
		{
			string scope, value;
			try
			{
				(scope, value) = GetScopedValue(claim);
			}
			catch
			{
				return false;
			}

			return scope switch
			{
				siteScope => JsonSerializer.Deserialize<bool>(value),
				tournamentScope => JsonSerializer.Deserialize<long>(value) == context.TournamentId,
				_ => throw new ScopeNotImplementedException(scope, typeof(EditTournamentPermissions))
			};
		}

		public Claim ForSite () => CreateScopedClaim(siteScope, true);
		public Claim ForTournament (long tournamentId) => CreateScopedClaim(tournamentScope, tournamentId);
	}
}
