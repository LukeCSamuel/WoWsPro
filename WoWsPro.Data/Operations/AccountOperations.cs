using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;
using WoWsPro.Shared.Models.Discord;
using WoWsPro.Shared.Models.Warships;
using WoWsPro.Shared.Permissions;

namespace WoWsPro.Data.Operations
{
	/// <summary>
	/// Defines operations scoped to an account.
	/// </summary>
	public class AccountOperations : Operations
	{
        IWarshipsApi WarshipsApi { get; set; }

        public AccountOperations(Context context, IAuthorizer authorizer, IWarshipsApi warshipsApi) : base(context, authorizer)
        {
            WarshipsApi = warshipsApi;
        }

        public async Task<string> GetNicknameAsync (long accountId) => (await Context.Accounts.SingleOrDefaultAsync(a => a.AccountId == accountId))?.Nickname;

		public Task<Account> GetAccountAsync (long accountId)
		{
			return Context.Accounts
				.Include(a => a.WarshipsAccounts).ThenInclude(p => p.Participations).ThenInclude(pp => pp.Team)
				.Include(a => a.DiscordAccounts)
				.Include(a => a.OwnedTeams).ThenInclude(t => t.Participants).ThenInclude(p => p.Player)
				.Where(a => a.AccountId == accountId)
				.SingleOrDefaultAsync() ?? throw new KeyNotFoundException();
		}

        public Task<IEnumerable<(long, string)>> FindPlayerAsync(Region region, string name)
            => WarshipsApi.SearchPlayerAsync(region, name);

        public async Task<WarshipsPlayer> FetchPlayer (Region region, long playerId)
		{
			var player = WarshipsApi.GetPlayerInfoAsync(region, playerId).GetAwaiter().GetResult();
			var dbPlayer = await Context.WarshipsPlayers
				.AsTracking()
				.Where(p => p.PlayerId == playerId)
				.SingleOrDefaultAsync();

			if (dbPlayer is null)
			{
				// Add and return
				var result = Context.WarshipsPlayers.Add(new WarshipsPlayer()
				{
					AccountId = player.AccountId,
					ClanId = player.ClanId,
					ClanRole = player.ClanRole,
					JoinedClan = player.JoinedClan,
					Created = player.Created,
					IsPrimary = player.IsPrimary,
					Nickname = player.Nickname,
					PlayerId = player.PlayerId,
					Region = player.Region
				}).Entity;

				await Context.SaveChangesAsync();
				return result;
			}
			else
			{
				dbPlayer.Nickname = player.Nickname;
				dbPlayer.ClanId = player.ClanId;
				dbPlayer.ClanRole = player.ClanRole;
				dbPlayer.JoinedClan = player.JoinedClan;

				await Context.SaveChangesAsync();
				return dbPlayer;
			}
		}

		public async Task SetNicknameAsync (long accountId, string nickname)
		{
			var acc = await Context.Accounts
				.AsTracking()
				.Where(a => a.AccountId == accountId)
				.SingleOrDefaultAsync();

			if (acc is Account)
			{
				acc.Nickname = nickname;
				await Context.SaveChangesAsync();
			}
			else
			{
				throw new KeyNotFoundException("Account does not exist.");
			}
		}

		public async Task<DiscordToken> GetDiscordTokenAsync (long discordId)
		{
			var user = await Context.DiscordUsers
				.Include(u => u.DiscordTokens)
				.Where(u => u.DiscordId == discordId)
				.SingleOrDefaultAsync();

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

		public async Task SetPrimaryDiscordUserAsync (long accountId, long discordId)
		{
			var acc = await Context.Accounts
				.Where(a => a.AccountId == accountId)
				.SingleOrDefaultAsync();
			var discord = await Context.DiscordUsers
				.AsTracking()
				.Where(u => u.AccountId == accountId && u.DiscordId == discordId)
				.SingleOrDefaultAsync();
			
			if (acc is Account && discord is DiscordUser)
			{
				var olds = await Context.DiscordUsers
					.AsTracking()
					.Where(u => u.AccountId == acc.AccountId && u.IsPrimary)
					.ToListAsync();
				foreach (var old in olds)
				{
					old.IsPrimary = false;
				}
				discord.IsPrimary = true;
				await Context.SaveChangesAsync();
			}
			else
			{
				throw new KeyNotFoundException("Either the scoped account or the requested discord user does not exist.");
			}
		}

		public async Task SetPrimaryWarshipsPlayerAsync (long accountId, long playerId)
		{
			var acc = await Context.Accounts
				.Where(a => a.AccountId == accountId)
				.SingleOrDefaultAsync();
			var player = await Context.WarshipsPlayers
				.AsTracking()
				.Where(p => p.AccountId == accountId && p.PlayerId == playerId)
				.SingleOrDefaultAsync();

			if (acc is Account && player is WarshipsPlayer)
			{
				var olds = await Context.WarshipsPlayers
					.AsTracking()
					.Where(p => p.AccountId == acc.AccountId && p.IsPrimary)
					.ToListAsync();
				foreach (var old in olds)
				{
					old.IsPrimary = false;
				}
				player.IsPrimary = true;
				await Context.SaveChangesAsync();
			}
			else
			{
				throw new KeyNotFoundException("Either the scoped account or the requested WoWs account does not exist.");
			}
		}

		public async Task AcceptTeamInviteAsync (long accountId, long teamId)
		{
			var acc = await Context.Accounts
				.AsTracking()
				.Include(a => a.WarshipsAccounts).ThenInclude(p => p.Participations).ThenInclude(p => p.Team)
				.Where(a => a.AccountId == accountId)
				.SingleOrDefaultAsync();
			var team = await Context.TournamentTeams
				.AsTracking()
				.Where(t => t.TeamId == teamId)
				.SingleOrDefaultAsync();

			if (acc is null || team is null)
			{
				throw new KeyNotFoundException();
			}
			else
			{
				foreach (var player in acc.WarshipsAccounts)
				{
					foreach (var participant in player.Participations)
					{
						if (participant.Team.TournamentId == team.TournamentId)
						{
							if (participant.TeamId == team.TeamId)
							{
								participant.Status = ParticipantStatus.Accepted;
							}
							else
							{
								participant.Status = ParticipantStatus.Declined;
							}
						}
					}
				}
				await Context.SaveChangesAsync();
			}
		}
	}

	public class AdminAccountOperations
	{
		public static readonly TimeSpan TokenExpiration = TimeSpan.FromDays(7);
		Context Context { get; }

		public AdminAccountOperations (Context context) => Context = context;

		public bool VerifyToken (long accountId, Guid token)
		{
			var tokens = Context.AccountTokens
				.AsTracking()
				.Where(t => t.AccountId == accountId && t.Token == token)
				.AsEnumerable();
			var now = DateTime.UtcNow;
			bool isValid = false;
			foreach (var t in tokens)
			{
				if (!isValid && now < t.Created + TokenExpiration)
				{
					// Extend this token
					t.Created = now + TokenExpiration;
					isValid = true;
				}
				else
				{
					Context.AccountTokens.Remove(t);
				}
			}
			Context.SaveChanges();
			return isValid;
		}

		public void InsertToken (long accountId, Guid token)
		{
			Context.AccountTokens.Add(new AccountToken()
			{
				AccountId = accountId,
				Token = token,
				Created = DateTime.UtcNow
			});
			Context.SaveChanges();
		}

		/// <summary>
		/// Gets the account associated with a particular user
		/// </summary>
		public Account GetAccount (long accountId)
		{
			var account = Context.Accounts
				.Include(a => a.Claims)
				.Include(a => a.DiscordAccounts)
				.Include(a => a.WarshipsAccounts)
				.Include(a => a.OwnedTeams)
				.Where(a => a.AccountId == accountId)
				.Single();
			return account;
		}

		// TODO: Implement a proper merge operation so accounts don't go headless!


		/// <summary>
		/// Addes a Discord account to a WoWsPro account
		/// </summary>
		public void AddOrMergeAccount (long accountId, DiscordToken token)
		{
			var account = Context.Accounts
				.AsTracking()
				.Where(a => a.AccountId == accountId)
				.Single();
			bool hasPrimary = Context.DiscordUsers
				.AsTracking()
				.Where(u => u.AccountId == account.AccountId && u.IsPrimary)
				.SingleOrDefault() is DiscordUser;

			if (Context.DiscordUsers.Any(u => u.DiscordId == token.DiscordId))
			{
				// User is already in database
				var existing = Context.DiscordUsers
					.AsTracking()
					.Include(u => u.Account)
					.Where(u => u.DiscordId == token.DiscordId)
					.Single();
				if (existing.Account is null)
				{
					// Can safely add it to this account
					existing.Account = account;
					existing.IsPrimary = existing.IsPrimary || !hasPrimary;
				}
				else if (existing.Account != account)
				{
					// Already has an account associated, move it to this account
					// ONEDAY: clean up unused accounts
					existing.Account = account;
					existing.IsPrimary = !hasPrimary;
				}
				existing.Avatar = token.DiscordUser.Avatar;
				existing.Discriminator = token.DiscordUser.Discriminator;
				existing.Username = token.DiscordUser.Username;
				Context.SaveChanges();
			}
			else
			{
				var dbUser = new DiscordUser()
				{
					AccountId = accountId,
					Avatar = token.DiscordUser.Avatar,
					DiscordId = token.DiscordUser.DiscordId,
					Discriminator = token.DiscordUser.Discriminator,
					IsPrimary = token.DiscordUser.IsPrimary,
					Username = token.DiscordUser.Username
				};
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
		public (string Nickname, long AccountId) GetOrCreateAccount (DiscordToken token)
		{
			var existing = Context.DiscordUsers
				.AsTracking()
				.Include(u => u.Account)
				.Where(u => u.DiscordId == token.DiscordId)
				.SingleOrDefault();
			if (existing is DiscordUser)
			{
				if (existing.Account is Shared.Models.Account)
				{
					// Account already exists
					bool hasPrimary = Context.DiscordUsers
						.Where(u => u.AccountId == existing.AccountId && u.IsPrimary)
						.SingleOrDefault() is DiscordUser;
					existing.IsPrimary = existing.IsPrimary || !hasPrimary;

					AddOrReplaceDiscordToken(token);
					return (existing.Account.Nickname, existing.Account.AccountId);
				}
				else
				{
					// Create a new account for this user
					var account = new Shared.Models.Account()
					{
						Nickname = token.DiscordUser.Username,
						Created = DateTime.UtcNow
					};
					Context.Accounts.Add(account);
					Context.SaveChanges();

					// AddDefaultClaims(account);
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
				var account = new Shared.Models.Account()
				{
					Nickname = token.DiscordUser.Username,
					Created = DateTime.UtcNow
				};
				Context.Accounts.Add(account);
				Context.SaveChanges();

				// AddDefaultClaims(account);
				var dbUser = new DiscordUser()
				{
					AccountId = token.DiscordUser.AccountId,
					Avatar = token.DiscordUser.Avatar,
					DiscordId = token.DiscordUser.DiscordId,
					Discriminator = token.DiscordUser.Discriminator,
					IsPrimary = token.DiscordUser.IsPrimary,
					Username = token.DiscordUser.Username
				};
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
		public void AddOrReplaceDiscordToken (DiscordToken token)
		{
			var existing = Context.DiscordTokens
				.Where(t => t.DiscordId == token.DiscordId)
				.OrderByDescending(t => t.Expires)
				.FirstOrDefault();
			if (existing is null)
			{
				// Null the discord user because it may or may not already be tracked
				token.DiscordUser = null;
				Context.DiscordTokens.Add(token);
				Context.SaveChanges();
			}
			else if (existing.Expires < token.Expires)
			{
				// Null the discord user because it may or may not already be tracked
				token.DiscordUser = null;
				Context.DiscordTokens.RemoveRange(Context.DiscordTokens.Where(t => t.DiscordId == token.DiscordId));
				Context.DiscordTokens.Add(token);
				Context.SaveChanges();
			}
		}

		/// <summary>
		/// Adds a World of Warships player account to a WoWsPro account
		/// </summary>
		public void AddOrMergeAccount (long accountId, WarshipsPlayer player)
		{
			var account = Context.Accounts
				.AsTracking()
				.Where(a => a.AccountId == accountId)
				.Single();
			bool hasPrimary = Context.WarshipsPlayers
				.Where(p => p.AccountId == account.AccountId && p.IsPrimary)
				.SingleOrDefault() is WarshipsPlayer;

			if (Context.WarshipsPlayers.Any(p => p.PlayerId == player.PlayerId))
			{
				// Player is already in the database
				var existing = Context.WarshipsPlayers
					.AsTracking()
					.Include(p => p.Account)
					.Where(p => p.PlayerId == player.PlayerId)
					.Single();
				if (existing.Account is null)
				{
					// Can safely add it to this account
					existing.Account = account;
					existing.IsPrimary = existing.IsPrimary || !hasPrimary;
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
				var dbPlayer = new WarshipsPlayer()
				{
					AccountId = player.AccountId,
					ClanId = player.ClanId,
					ClanRole = player.ClanRole,
					JoinedClan = player.JoinedClan,
					Created = player.Created,
					IsPrimary = player.IsPrimary,
					Nickname = player.Nickname,
					PlayerId = player.PlayerId,
					Region = player.Region
				};
				dbPlayer.Account = account;
				dbPlayer.IsPrimary = !hasPrimary;
				Context.WarshipsPlayers.Add(dbPlayer);
				Context.SaveChanges();
			}
		}
		
		/// <summary>
		/// Gets or creates the WoWsPro account associated with the specified World of Warships player account
		/// </summary>
		public (string Nickname, long AccountId) GetOrCreateAccount (WarshipsPlayer player)
		{
			var existing = Context.WarshipsPlayers
				.Include(p => p.Account)
				.AsTracking()
				.Where(p => p.PlayerId == player.PlayerId)
				.SingleOrDefault();
			if (existing is WarshipsPlayer)
			{
				if (existing.Account is Account)
				{
					bool hasPrimary = Context.WarshipsPlayers
						.Where(p => p.AccountId == existing.AccountId && p.IsPrimary)
						.SingleOrDefault() is WarshipsPlayer;
					existing.IsPrimary = existing.IsPrimary || !hasPrimary;
					Context.SaveChanges();

					// Account already exists
					return (existing.Account.Nickname, existing.Account.AccountId);
				}
				else
				{
					// Create a new account for this player
					var account = new Account()
					{
						Nickname = player.Nickname,
						Created = DateTime.UtcNow
					};
					Context.Accounts.Add(account);
					Context.SaveChanges();

					// AddDefaultClaims(account);
					existing.IsPrimary = true;
					existing.Account = account;
					Context.SaveChanges();

					return (account.Nickname, account.AccountId);
				}
			}
			else
			{
				// Create new account for new player
				var account = new Account()
				{
					Nickname = player.Nickname,
					Created = DateTime.UtcNow
				};
				Context.Accounts.Add(account);
				Context.SaveChanges();

				// AddDefaultClaims(account);
				var dbPlayer = new WarshipsPlayer()
				{
					AccountId = player.AccountId,
					ClanId = player.ClanId,
					ClanRole = player.ClanRole,
					JoinedClan = player.JoinedClan,
					Created = player.Created,
					IsPrimary = player.IsPrimary,
					Nickname = player.Nickname,
					PlayerId = player.PlayerId,
					Region = player.Region
				}; ;
				dbPlayer.IsPrimary = true;
				dbPlayer.Account = account;
				Context.WarshipsPlayers.Add(dbPlayer);
				Context.SaveChanges();

				return (account.Nickname, account.AccountId);
			}
		}

		/// <summary>
		/// Adds the default claims to the account
		// /// </summary>
		// private void AddDefaultClaims (Account account)
		// {
		// 	foreach (var permission in Permissions.InitialPermissions)
		// 	{
		// 		long claimId = Context.Claims.Single(c => c.Permission == permission.Permission).ClaimId;
		// 		account.AccountClaims.Add(new DB.Models.AccountClaim()
		// 		{
		// 			AccountId = account.AccountId,
		// 			ClaimId = claimId
		// 		});
		// 	}
		// }
	}
}
