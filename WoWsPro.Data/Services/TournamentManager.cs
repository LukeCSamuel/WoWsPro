using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;

namespace WoWsPro.Data.Services
{
	public class TournamentManager : Manager<TournamentOperations>
	{
		internal override Type ScopeType => typeof(DB.Models.Tournament);
		internal override TournamentOperations Instance { get; }
		internal override Context Context { get; }
		internal override long? UserId { get; }

		public TournamentManager (IAuthenticator authenticator, Context context)
		{
			Context = context;
			UserId = authenticator.AccountId;
			Instance = new TournamentOperations(this);
		}
	}

	public static class TournamentManagerProvider
	{
		public static IServiceCollection AddTournamentManager (this IServiceCollection services)
			=> services.AddAuthorizer<TournamentManager, TournamentOperations>();
	}
}

namespace WoWsPro.Data.Operations
{
	public partial class TournamentOperations : Operations<TournamentOperations>
	{
		internal TournamentOperations (Manager<TournamentOperations> manager) => Manager = manager;
	}
}
