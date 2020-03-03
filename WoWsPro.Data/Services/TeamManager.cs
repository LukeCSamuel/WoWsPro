using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;

namespace WoWsPro.Data.Services
{
	public class TeamManager : Manager<TeamOperations>
	{
		internal override Type ScopeType => typeof(DB.Models.TournamentTeam);
		internal override TeamOperations Instance { get; }
		internal override Context Context { get; }
		internal override long? UserId { get; }

		public TeamManager (IAuthenticator authenticator, Context context)
		{
			Context = context;
			UserId = authenticator.AccountId;
			Instance = new TeamOperations(this);
		}
	}

	public static class TeamManagerProvider
	{
		public static IServiceCollection AddTeamManager (this IServiceCollection services)
			=> services.AddAuthorizer<TeamManager, TeamOperations>();
	}
}


namespace WoWsPro.Data.Operations
{
	public partial class TeamOperations : Operations<TeamOperations>
	{
		internal TeamOperations (Manager<TeamOperations> manager) => Manager = manager;
	}
}