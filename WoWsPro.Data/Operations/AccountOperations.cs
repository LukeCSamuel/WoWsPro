﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWsPro.Data.Attributes;
using WoWsPro.Data.DB;
using WoWsPro.Data.DB.Models;
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
		public Shared.Models.Account GetAccount ()
		{
			return Context.Accounts
				.Include(a => a.WarshipsAccounts).ThenInclude(p => p.Participations).ThenInclude(pp => pp.Team)
				.Include(a => a.DiscordAccounts)
				.Include(a => a.OwnedTeams).ThenInclude(t => t.Participants).ThenInclude(p => p.Player)
				.Where(a => a.AccountId == ScopeId)
				.Select(a => new Shared.Models.Account()
				{
					AccountId = a.AccountId,
					Nickname = a.Nickname,
					Created = a.Created,
					WarshipsAccounts = a.WarshipsAccounts.Select(p => new Shared.Models.WarshipsPlayer()
					{
						AccountId = p.AccountId,
						PlayerId = p.PlayerId, 
						Region = p.Region,
						Nickname = p.Nickname,
						Created = p.Created,
						ClanId = p.ClanId,
						ClanRole = p.ClanRole,
						JoinedClan = p.JoinedClan,
						IsPrimary = p.IsPrimary,
						Participations = p.Participations.Select(pp => new Shared.Models.TournamentParticipant()
						{
							ParticipantId = pp.ParticipantId,
							TeamId = pp.TeamId,
							PlayerId = pp.PlayerId,
							Status = pp.Status,
							Team = new Shared.Models.TournamentTeam()
							{
								TeamId = pp.TeamId,
								TournamentId = pp.Team.TournamentId,
								Name = pp.Team.Name,
								Tag = pp.Team.Tag,
								Description = pp.Team.Description,
								Icon = pp.Team.Icon,
								OwnerAccountId = pp.Team.OwnerAccountId,
								Status = pp.Team.Status,
								Region = pp.Team.Region,
								Created = pp.Team.Created
							}
						}).ToList()
					}).ToList(),
					DiscordAccounts = a.DiscordAccounts.Select(u => new Shared.Models.DiscordUser()
					{
						DiscordId = u.DiscordId,
						AccountId = u.AccountId,
						Username = u.Username,
						Discriminator = u.Discriminator,
						Avatar = u.Avatar,
						IsPrimary = u.IsPrimary
					}).ToList(),
					OwnedTeams = a.OwnedTeams.Select(t => new Shared.Models.TournamentTeam()
					{
						TeamId = t.TeamId,
						TournamentId = t.TournamentId,
						Name = t.Name,
						Tag = t.Tag,
						Description = t.Description,
						Icon = t.Icon,
						OwnerAccountId = t.OwnerAccountId,
						Status = t.Status,
						Region = t.Region,
						Created = t.Created,
						Participants = t.Participants.Select(p => new Shared.Models.TournamentParticipant()
						{
							ParticipantId = p.ParticipantId,
							TeamId = p.TeamId,
							PlayerId = p.PlayerId,
							Status = p.Status,
							Player = new Shared.Models.WarshipsPlayer()
							{
								AccountId = p.Player.AccountId,
								PlayerId = p.Player.PlayerId,
								Region = p.Player.Region,
								Nickname = p.Player.Nickname,
								Created = p.Player.Created,
								ClanId = p.Player.ClanId,
								ClanRole = p.Player.ClanRole,
								JoinedClan = p.Player.JoinedClan,
								IsPrimary = p.Player.IsPrimary
							}
						}).ToList()
					}).ToList()
				})
				.SingleOrDefault() ?? throw new KeyNotFoundException();
		}

		[Public]
		public IEnumerable<(long, string)> FindPlayer (Region region, string name) => WarshipsApi.SearchPlayerAsync(region, name).GetAwaiter().GetResult();

		[Public]
		public Shared.Models.WarshipsPlayer FetchPlayer (Region region, long playerId)
		{
			var player = WarshipsApi.GetPlayerInfoAsync(region, playerId).GetAwaiter().GetResult();
			var dbPlayer = Context.WarshipsPlayers.SingleOrDefault(p => p.PlayerId == playerId);
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
				Context.SaveChanges();
				return new Shared.Models.WarshipsPlayer()
				{
					AccountId = result.AccountId,
					ClanId = result.ClanId,
					ClanRole = result.ClanRole,
					JoinedClan = result.JoinedClan,
					Created = result.Created,
					IsPrimary = result.IsPrimary,
					Nickname = result.Nickname,
					PlayerId = result.PlayerId,
					Region = result.Region
				};;
			}
			else
			{
				dbPlayer.Nickname = player.Nickname;
				dbPlayer.ClanId = player.ClanId;
				dbPlayer.ClanRole = player.ClanRole;
				dbPlayer.JoinedClan = player.JoinedClan;
				Context.SaveChanges();
				return new Shared.Models.WarshipsPlayer()
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
			}
		}

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
					return new Shared.Models.DiscordToken()
					{
						AccessToken = bestToken.AccessToken,
						DiscordId = bestToken.DiscordId,
						DiscordTokenId = bestToken.DiscordTokenId,
						DiscordUser = new Shared.Models.DiscordUser()
						{
							DiscordId = bestToken.DiscordUser.DiscordId,
							AccountId = bestToken.DiscordUser.AccountId,
							Avatar = bestToken.DiscordUser.Avatar,
							Discriminator = bestToken.DiscordUser.Discriminator,
							Username = bestToken.DiscordUser.Username,
							IsPrimary = bestToken.DiscordUser.IsPrimary
						},
						Expires = bestToken.Expires,
						RefreshToken = bestToken.RefreshToken,
						Scope = bestToken.Scope,
						TokenType = bestToken.TokenType
					};
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

		[Authorize(nameof(Permissions.ManageAccount))]
		public void AcceptTeamInvite (long teamId)
		{
			var acc = Context.Accounts.SingleOrDefault(a => a.AccountId == ScopeId);
			var team = Context.TournamentTeams.SingleOrDefault(t => t.TeamId == teamId);

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
			return new Shared.Models.Account()
			{
				AccountId = account.AccountId,
				Created = account.Created,
				Nickname = account.Nickname,
				DiscordAccounts = account.DiscordAccounts.Select(du => new Shared.Models.DiscordUser()
				{
					AccountId = du.AccountId,
					Avatar = du.Avatar,
					DiscordId = du.DiscordId,
					Discriminator = du.Discriminator,
					Username = du.Username,
					IsPrimary = du.IsPrimary
				}).ToList(),
				WarshipsAccounts = account.WarshipsAccounts.Select(wp => new Shared.Models.WarshipsPlayer()
				{
					AccountId = wp.AccountId,
					ClanId = wp.ClanId,
					ClanRole = wp.ClanRole,
					Created = wp.Created,
					IsPrimary = wp.IsPrimary,
					JoinedClan = wp.JoinedClan,
					Nickname = wp.Nickname,
					PlayerId = wp.PlayerId,
					Region = wp.Region
				}).ToList(),
				OwnedTeams = account.OwnedTeams.Select(tt => new Shared.Models.TournamentTeam()
				{
					TeamId = tt.TeamId,
					TournamentId = tt.TournamentId,
					OwnerAccountId = tt.OwnerAccountId,
					Created = tt.Created,
					Description = tt.Description,
					Icon = tt.Icon,
					Name = tt.Name,
					Region = tt.Region,
					Status = tt.Status,
					Tag = tt.Tag
				}).ToList()
			};
		}

		// TODO: Implement a proper merge operation so accounts don't go headless!


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
					Context.Accounts.Add(account);
					Context.SaveChanges();

					AddDefaultClaims(account);
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
				Context.Accounts.Add(account);
				Context.SaveChanges();

				AddDefaultClaims(account);
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
		public void AddOrReplaceDiscordToken (Shared.Models.DiscordToken token)
		{
			var existing = Context.DiscordTokens
				.Where(t => t.DiscordId == token.DiscordId)
				.OrderByDescending(t => t.Expires)
				.FirstOrDefault();
			if (existing is null)
			{
				token.DiscordUser = null;
				Context.DiscordTokens.Add(new DiscordToken()
				{
					AccessToken = token.AccessToken,
					DiscordId = token.DiscordId,
					DiscordTokenId = token.DiscordTokenId,
					DiscordUser = new DiscordUser()
					{
						DiscordId = token.DiscordUser.DiscordId,
						AccountId = token.DiscordUser.AccountId,
						Avatar = token.DiscordUser.Avatar,
						Discriminator = token.DiscordUser.Discriminator,
						Username = token.DiscordUser.Username,
						IsPrimary = token.DiscordUser.IsPrimary
					},
					Expires = token.Expires,
					RefreshToken = token.RefreshToken,
					Scope = token.Scope,
					TokenType = token.TokenType
				});
				Context.SaveChanges();
			}
			else if (existing.Expires < token.Expires)
			{
				Context.DiscordTokens.RemoveRange(Context.DiscordTokens.Where(t => t.DiscordId == token.DiscordId));
				token.DiscordUser = null;
				Context.DiscordTokens.Add(new DiscordToken()
				{
					AccessToken = token.AccessToken,
					DiscordId = token.DiscordId,
					DiscordTokenId = token.DiscordTokenId,
					DiscordUser = new DiscordUser()
					{
						DiscordId = token.DiscordUser.DiscordId,
						AccountId = token.DiscordUser.AccountId,
						Avatar = token.DiscordUser.Avatar,
						Discriminator = token.DiscordUser.Discriminator,
						Username = token.DiscordUser.Username,
						IsPrimary = token.DiscordUser.IsPrimary
					},
					Expires = token.Expires,
					RefreshToken = token.RefreshToken,
					Scope = token.Scope,
					TokenType = token.TokenType
				});
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
					Context.Accounts.Add(account);
					Context.SaveChanges();

					AddDefaultClaims(account);
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
				Context.Accounts.Add(account);
				Context.SaveChanges();

				AddDefaultClaims(account);
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
