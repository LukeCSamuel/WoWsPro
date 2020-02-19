using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Data.Attributes;
using WoWsPro.Data.DB;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.Operations
{
	/// <summary>
	/// Defines operations scoped to an account.
	/// </summary>
	public partial class AccountOperations
	{
		[Public]
		public string GetNickname () => Context.Accounts.SingleOrDefault(a => a.AccountId == ScopeId)?.Nickname;

		[Public]
		public Shared.Models.Account GetAccount () => Context.Accounts.SingleOrDefault(a => a.AccountId == ScopeId);

		[Authorize(nameof(Permissions.ManageAccount))]
		public void SetNickname (string nickname)
		{
			var acc = Context.Accounts.SingleOrDefault(a => a.AccountId == ScopeId);
			acc.Nickname = nickname;
			Context.SaveChanges();
		}
	}

	public class AdminAccountOperations
	{
		Context Context { get; }

		public AdminAccountOperations (Context context)
		{
			Context = context;
		}

		/// <summary>
		/// Gets the account associated with a particular user
		/// </summary>
		public Shared.Models.Account GetAccount (long accountId)
		{
			var account = Context.Accounts.Single(a => a.AccountId == accountId);
			return account;
		}

		/// <summary>
		/// Adds a World of Warships player account to a WoWsPro account
		/// </summary>
		public void AddOrMergeWarshipsAccount (long accountId, Shared.Models.WarshipsPlayer player)
		{
			var account = Context.Accounts.Single(a => a.AccountId == accountId);
			if (Context.WarshipsPlayers.Any(p => p.PlayerId == player.PlayerId))
			{
				// Player is already in the database
				var existing = Context.WarshipsPlayers.Single(p => p.PlayerId == player.PlayerId);
				if (existing.Account is null)
				{
					// Can safely add it to this account
					existing.Account = account;
					Context.SaveChanges();
				}
				else
				{
					// Already has an account associated, move it to this account
					// ONEDAY: clean up unused accounts
					existing.Account = account;
					Context.SaveChanges();
				}
			}
			else
			{
				// Player is not in database, can be added
				var dbPlayer = (DB.Models.WarshipsPlayer)player;
				dbPlayer.Account = account;
				Context.WarshipsPlayers.Add(dbPlayer);
				Context.SaveChanges();
			}
		}
		
		/// <summary>
		/// Gets or creates the WoWsPro account associated with the specified World of Warships player account
		/// </summary>
		public (string Nickname, long AccountId) GetOrCreateAccount (Shared.Models.WarshipsPlayer player)
		{
			var existing = Context.WarshipsPlayers.SingleOrDefault(p => p.PlayerId == player.PlayerId);
			if (existing is DB.Models.WarshipsPlayer)
			{
				if (existing.Account is DB.Models.Account)
				{
					// Account already exists
					return (existing.Account.Nickname, existing.Account.AccountId);
				}
				else
				{
					// Create a new account for this player
					var account = new DB.Models.Account()
					{
						Nickname = player.Nickname,
						Created = DateTime.UtcNow
					};
					AddDefaultClaims(account);
					Context.Accounts.Add(account);

					existing.IsPrimary = true;
					existing.Account = account;

					Context.SaveChanges();
					return (account.Nickname, account.AccountId);
				}
			}
			else
			{
				// Create new account for new player
				var account = new DB.Models.Account()
				{
					Nickname = player.Nickname,
					Created = DateTime.UtcNow
				};
				AddDefaultClaims(account);
				Context.Accounts.Add(account);

				var dbPlayer = (DB.Models.WarshipsPlayer)player;
				dbPlayer.IsPrimary = true;
				dbPlayer.Account = account;
				Context.WarshipsPlayers.Add(dbPlayer);				

				Context.SaveChanges();
				return (account.Nickname, account.AccountId);
			}
		}

		/// <summary>
		/// Addes the default claims to the account
		/// </summary>
		private void AddDefaultClaims (DB.Models.Account account)
		{
			foreach (var permission in Permissions.InitialPermissions)
			{
				long claimId = Context.Claims.Single(c => c.Permission == permission.Permission).ClaimId;
				account.AccountClaims.Add(new DB.Models.AccountClaim()
				{
					AccountId = account.AccountId,
					ClaimId = claimId
				});
			}
		}
	}

	public static class AdminAccountOperationsProvider
	{
		public static IServiceCollection AddAdminAccountOperations (this IServiceCollection services)
			=> services.AddScoped<AdminAccountOperations>();
	}
}
