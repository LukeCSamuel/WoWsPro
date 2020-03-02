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

		[Authorize(nameof(Permissions.ManageAccount))]
		public Shared.Models.DiscordToken GetDiscordToken (long discordId)
		{
			var user = Context.DiscordUsers.SingleOrDefault(u => u.DiscordId == discordId);
			if (user is null)
			{
				throw new KeyNotFoundException("User not found.");
			}
			else
			{
				var bestToken = user.DiscordTokens.OrderByDescending(t => t.Expires).FirstOrDefault();
				if (bestToken is null || bestToken.Expires < DateTime.UtcNow)
				{
					throw new KeyNotFoundException("No token available.");
				}
				else
				{
					return bestToken;
				}
			}
		}

		[Authorize(nameof(Permissions.ManageAccount))]
		public void SetPrimaryDiscordUser (long discordId)
		{
			var acc = Context.Accounts.SingleOrDefault(a => a.AccountId == ScopeId);
			var discord = Context.DiscordUsers.SingleOrDefault(u => u.AccountId == ScopeId && u.DiscordId == discordId);
			
			if (acc is DB.Models.Account && discord is DB.Models.DiscordUser)
			{
				var olds = Context.DiscordUsers.Where(u => u.AccountId == acc.AccountId && u.IsPrimary);
				foreach (var old in olds)
				{
					old.IsPrimary = false;
				}
				discord.IsPrimary = true;
				Context.SaveChanges();
			}
			else
			{
				throw new KeyNotFoundException("Either the scoped account or the requested discord user does not exist.");
			}
		}

		[Authorize(nameof(Permissions.ManageAccount))]
		public void SetPrimaryWarshipsPlayer (long playerId)
		{
			var acc = Context.Accounts.SingleOrDefault(a => a.AccountId == ScopeId);
			var player = Context.WarshipsPlayers.SingleOrDefault(p => p.AccountId == ScopeId && p.PlayerId == playerId);

			if (acc is DB.Models.Account && player is DB.Models.WarshipsPlayer)
			{
				var olds = Context.WarshipsPlayers.Where(p => p.AccountId == acc.AccountId && p.IsPrimary);
				foreach (var old in olds)
				{
					old.IsPrimary = false;
				}
				player.IsPrimary = true;
				Context.SaveChanges();
			}
		}
	}

	public class AdminAccountOperations
	{
		Context Context { get; }

		public AdminAccountOperations (Context context) => Context = context;

		/// <summary>
		/// Gets the account associated with a particular user
		/// </summary>
		public Shared.Models.Account GetAccount (long accountId)
		{
			var account = Context.Accounts.Single(a => a.AccountId == accountId);
			return account;
		}


		/// <summary>
		/// Addes a Discord account to a WoWsPro account
		/// </summary>
		public void AddOrMergeAccount (long accountId, Shared.Models.DiscordToken token)
		{
			var account = Context.Accounts.Single(a => a.AccountId == accountId);
			bool hasPrimary = Context.DiscordUsers.SingleOrDefault(u => u.AccountId == account.AccountId && u.IsPrimary) is DB.Models.DiscordUser;
			if (Context.DiscordUsers.Any(u => u.DiscordId == token.DiscordId))
			{
				// User is already in database
				var existing = Context.DiscordUsers.Single(u => u.DiscordId == token.DiscordId);
				if (existing.Account is null)
				{
					// Can safely add it to this account
					existing.Account = account;
					existing.IsPrimary = !hasPrimary;
					Context.SaveChanges();
				}
				else
				{
					// Already has an account associated, move it to this account
					// ONEDAY: clean up unused accounts
					existing.Account = account;
					existing.IsPrimary = !hasPrimary;
					Context.SaveChanges();
				}
			}
			else
			{
				var dbUser = (DB.Models.DiscordUser)token.DiscordUser;
				dbUser.Account = account;
				dbUser.IsPrimary = !hasPrimary;
				Context.DiscordUsers.Add(dbUser);
				Context.SaveChanges();
			}

			AddOrReplaceDiscordToken(token);
		}

		/// <summary>
		/// Gets or creates the WoWsPro account associated with the specified Discord account
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public (string Nickname, long AccountId) GetOrCreateAccount (Shared.Models.DiscordToken token)
		{
			var existing = Context.DiscordUsers.SingleOrDefault(u => u.DiscordId == token.DiscordId);
			if (existing is DB.Models.DiscordUser)
			{
				if (existing.Account is DB.Models.Account)
				{
					// Account already exists
					bool hasPrimary = Context.DiscordUsers.SingleOrDefault(u => u.AccountId == existing.AccountId && u.IsPrimary) is DB.Models.DiscordUser;
					existing.IsPrimary = existing.IsPrimary || !hasPrimary;

					AddOrReplaceDiscordToken(token);
					return (existing.Account.Nickname, existing.Account.AccountId);
				}
				else
				{
					// Create a new account for this user
					var account = new DB.Models.Account()
					{
						Nickname = token.DiscordUser.Username,
						Created = DateTime.UtcNow
					};
					AddDefaultClaims(account);
					Context.Accounts.Add(account);

					existing.IsPrimary = true;
					existing.Account = account;

					Context.SaveChanges();
					AddOrReplaceDiscordToken(token);
					return (account.Nickname, account.AccountId);
				}
			}
			else
			{
				// Create new account for new user
				var account = new DB.Models.Account()
				{
					Nickname = token.DiscordUser.Username,
					Created = DateTime.UtcNow
				};
				AddDefaultClaims(account);
				Context.Accounts.Add(account);

				var dbUser = (DB.Models.DiscordUser)token.DiscordUser;
				dbUser.IsPrimary = true;
				dbUser.Account = account;
				Context.DiscordUsers.Add(dbUser);

				Context.SaveChanges();
				AddOrReplaceDiscordToken(token);
				return (account.Nickname, account.AccountId);
			}
		}


		/// <summary>
		/// Adds a Discord token to the database
		/// </summary>
		public void AddOrReplaceDiscordToken (Shared.Models.DiscordToken token)
		{
			var existing = Context.DiscordTokens
				.Where(t => t.DiscordId == token.DiscordId)
				.OrderByDescending(t => t.Expires)
				.FirstOrDefault();
			if (existing is null)
			{
				token.DiscordUser = null;
				Context.DiscordTokens.Add(token);
				Context.SaveChanges();
			}
			else if (existing.Expires < token.Expires)
			{
				Context.DiscordTokens.RemoveRange(Context.DiscordTokens.Where(t => t.DiscordId == token.DiscordId));
				token.DiscordUser = null;
				Context.DiscordTokens.Add(token);
				Context.SaveChanges();
			}
		}

		/// <summary>
		/// Adds a World of Warships player account to a WoWsPro account
		/// </summary>
		public void AddOrMergeAccount (long accountId, Shared.Models.WarshipsPlayer player)
		{
			var account = Context.Accounts.Single(a => a.AccountId == accountId);
			bool hasPrimary = Context.WarshipsPlayers.SingleOrDefault(p => p.AccountId == account.AccountId && p.IsPrimary) is DB.Models.WarshipsPlayer;
			if (Context.WarshipsPlayers.Any(p => p.PlayerId == player.PlayerId))
			{
				// Player is already in the database
				var existing = Context.WarshipsPlayers.Single(p => p.PlayerId == player.PlayerId);
				if (existing.Account is null)
				{
					// Can safely add it to this account
					existing.Account = account;
					existing.IsPrimary = !hasPrimary;
					Context.SaveChanges();
				}
				else
				{
					// Already has an account associated, move it to this account
					// ONEDAY: clean up unused accounts
					existing.Account = account;
					existing.IsPrimary = !hasPrimary;
					Context.SaveChanges();
				}
			}
			else
			{
				// Player is not in database, can be added
				var dbPlayer = (DB.Models.WarshipsPlayer)player;
				dbPlayer.Account = account;
				dbPlayer.IsPrimary = !hasPrimary;
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
					bool hasPrimary = Context.WarshipsPlayers.SingleOrDefault(p => p.AccountId == existing.AccountId && p.IsPrimary) is DB.Models.WarshipsPlayer;
					existing.IsPrimary = existing.IsPrimary || !hasPrimary;

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
		/// Adds the default claims to the account
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
