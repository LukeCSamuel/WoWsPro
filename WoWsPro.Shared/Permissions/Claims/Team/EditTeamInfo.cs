using System;
using System.Text.Json;
using WoWsPro.Shared.Exceptions;
using WoWsPro.Shared.Models;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Shared.Permissions.Claims.Team {
    public class EditTeamInfo : ClaimResolver<TournamentTeam>
    {
        const string siteScope = "Site";
        const string tournamentScope = "Tournament";
        const string teamScope = "Team";

        public override string ClaimTitle => "EditTeamInfo";

        public EditTeamInfo () { }

        protected override bool IsClaimValid(Claim claim, TournamentTeam context)
        {
            // Owner can always edit team info
            if (context.OwnerAccountId == claim.AccountId)
            {
                return true;
            }

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
                teamScope => JsonSerializer.Deserialize<long>(value) == context.TeamId,
                _ => throw new ScopeNotImplementedException(scope, typeof(EditTeamInfo))
            };
        }

        public Claim ForSite () => CreateScopedClaim(siteScope, true);
        public Claim ForTournament (long tournamentId) => CreateScopedClaim(tournamentScope, tournamentId);
        public Claim ForTeam (long teamId) => CreateScopedClaim(teamScope, teamId);
    }
}