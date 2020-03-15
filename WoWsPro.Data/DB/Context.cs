using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using WoWsPro.Data.DB.Models;

namespace WoWsPro.Data.DB
{

	public partial class Context : DbContext
	{
		public Context (DbContextOptions<Context> options) : base(options) { }

		#region Tables
		internal virtual DbSet<ApplicationSetting> ApplicationSettings { get; set; }
		internal virtual DbSet<FileContent> Files { get; set; }

		internal virtual DbSet<Account> Accounts { get; set; }
		internal virtual DbSet<Claim> Claims { get; set; }
		internal virtual DbSet<AccountClaim> AccountClaims { get; set; }

		internal virtual DbSet<DiscordUser> DiscordUsers { get; set; }
		internal virtual DbSet<DiscordGuild> DiscordGuilds { get; set; }
		internal virtual DbSet<DiscordRole> DiscordRoles { get; set; }
		internal virtual DbSet<DiscordToken> DiscordTokens { get; set; }

		internal virtual DbSet<WarshipsClan> WarshipsClans { get; set; }
		internal virtual DbSet<WarshipsPlayer> WarshipsPlayers { get; set; }
		internal virtual DbSet<WarshipsMap> WarshipsMaps { get; set; }

		internal virtual DbSet<Tournament> Tournaments { get; set; }
		internal virtual DbSet<TournamentRegistrationRules> TournamentRegistrationRules { get; set; }
		internal virtual DbSet<TournamentClaim> TournamentClaims { get; set; }
		internal virtual DbSet<TournamentStage> TournamentStages { get; set; }
		internal virtual DbSet<TournamentGroup> TournamentGroups { get; set; }
		internal virtual DbSet<TournamentTeam> TournamentTeams { get; set; }
		internal virtual DbSet<TournamentTeamClaim> TournamentTeamClaims { get; set; }
		internal virtual DbSet<TournamentSeed> TournamentSeads { get; set; }
		internal virtual DbSet<TournamentMatch> TournamentMatches { get; set; }
		internal virtual DbSet<TournamentGame> TournamentGames { get; set; }
		internal virtual DbSet<TournamentParticipant> TournamentParticipants { get; set; }
		#endregion

		#region Table Configuration
		protected override void OnModelCreating (ModelBuilder modelBuilder) => modelBuilder

			.Entity<ApplicationSetting>(entity =>
			{
				entity.ToTable(nameof(ApplicationSetting))
					.HasKey(e => e.ApplicationSettingId)
					;

				entity.Property(e => e.Key)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Environment)
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Value)
					.HasMaxLength(255)
					.IsUnicode(false);
			})

			.Entity<FileContent>(entity =>
			{
				entity.ToTable(nameof(FileContent))
					.HasKey(e => e.FileContentId);

				entity.Property(e => e.Title)
					.HasMaxLength(255)
					.IsUnicode(false);
			})

			.Entity<Account>(entity =>
			{
				entity.ToTable(nameof(Account))
					.HasKey(e => e.AccountId);

				entity.Property(e => e.Nickname)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode();
			})

			.Entity<Claim>(entity =>
			{
				entity.ToTable(nameof(Claim))
					.HasKey(e => e.ClaimId);

				entity.Property(e => e.Permission)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);
			})

			.Entity<AccountClaim>(entity =>
			{
				entity.ToTable(nameof(AccountClaim))
					.HasKey(e => new { e.AccountId, e.ClaimId });

				entity.HasOne(d => d.Account)
					.WithMany(p => p.AccountClaims)
					.HasForeignKey(d => d.AccountId);

				entity.HasOne(d => d.Claim)
					.WithMany(p => p.AccountClaims)
					.HasForeignKey(d => d.ClaimId);
			})


			.Entity<DiscordUser>(entity =>
			{
				entity.ToTable(nameof(DiscordUser))
					.HasKey(e => e.DiscordId);

				entity.Property(e => e.DiscordId).ValueGeneratedNever();

				entity.Property(e => e.Username)
					.IsRequired()
					.HasMaxLength(64)
					.IsUnicode();

				entity.Property(e => e.Discriminator)
					.IsRequired()
					.HasMaxLength(4)
					.IsUnicode(false);

				entity.Property(e => e.Avatar)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.HasOne(d => d.Account)
					.WithMany(p => p.DiscordAccounts)
					.HasForeignKey(d => d.AccountId);
			})

			.Entity<DiscordToken>(entity =>
			{
				entity.ToTable(nameof(DiscordToken))
					.HasKey(e => e.DiscordTokenId);

				entity.Property(e => e.AccessToken)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.TokenType)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.RefreshToken)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Scope)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.HasOne(d => d.DiscordUser)
					.WithMany(p => p.DiscordTokens)
					.HasForeignKey(d => d.DiscordId);
			})

			.Entity<DiscordGuild>(entity =>
			{
				entity.ToTable(nameof(DiscordGuild))
					.HasKey(e => e.GuildId);

				entity.Property(e => e.GuildId).ValueGeneratedNever();

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(200)
					.IsUnicode();

				entity.Property(e => e.Icon)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Invite)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode(false);
			})

			.Entity<DiscordRole>(entity =>
			{
				entity.ToTable(nameof(DiscordRole))
					.HasKey(e => e.RoleKeyId);

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode();

				entity.HasOne(d => d.Guild)
					.WithMany(p => p.Roles)
					.HasForeignKey(d => d.GuildId);
			})

			.Entity<WarshipsClan>(entity =>
			{
				entity.ToTable(nameof(WarshipsClan))
					.HasKey(e => e.ClanId);

				entity.Property(e => e.ClanId).ValueGeneratedNever();

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode();

				entity.Property(e => e.Tag)
					.IsRequired()
					.HasMaxLength(5)
					.IsUnicode(false);
			})

			.Entity<WarshipsPlayer>(entity =>
			{
				entity.ToTable(nameof(WarshipsPlayer))
					.HasKey(e => e.PlayerId);

				entity.Property(e => e.PlayerId).ValueGeneratedNever();

				entity.Property(e => e.Nickname)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.ClanRole)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.HasOne(d => d.Account)
					.WithMany(p => p.WarshipsAccounts)
					.HasForeignKey(d => d.AccountId);

				entity.HasOne(d => d.Clan)
					.WithMany(p => p.Players)
					.HasForeignKey(d => d.ClanId);
			})

			.Entity<WarshipsMap>(entity =>
			{
				entity.ToTable(nameof(WarshipsMap))
					.HasKey(e => e.MapId);

				entity.Property(e => e.MapId).ValueGeneratedNever();

				entity.Property(e => e.Description)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Icon)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode(false);
			})


			.Entity<Tournament>(entity =>
			{
				entity.ToTable(nameof(Tournament))
					.HasKey(e => e.TournamentId);

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode();

				entity.Property(e => e.Icon)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.Property(e => e.Description)
					.IsRequired()
					.HasMaxLength(2000)
					.IsUnicode();

				entity.Property(e => e.OwnerAccountId)
					.HasColumnName("Owner");

				entity.Property(e => e.ParticipantRoleId)
					.HasColumnName("ParticipantRole");

				entity.Property(e => e.TeamOwnerRoleId)
					.HasColumnName("TeamOwnerRole");

				entity.HasOne(d => d.Guild)
					.WithMany(p => p.Tournaments)
					.HasForeignKey(d => d.GuildId);

				entity.HasOne(d => d.Owner)
					.WithMany(p => p.OwnedTournaments)
					.HasForeignKey(d => d.OwnerAccountId);

				entity.HasOne(d => d.ParticipantRole)
					.WithMany()
					.HasForeignKey(d => d.ParticipantRoleId);

				entity.HasOne(d => d.TeamOwnerRole)
					.WithMany()
					.HasForeignKey(d => d.TeamOwnerRoleId);
			})

			.Entity<TournamentRegistrationRules>(entity =>
			{
				entity.ToTable(nameof(TournamentRegistrationRules))
					.HasKey(e => e.TournamentRegistrationRulesId);

				entity.Property(e => e.RegionParticipantRoleId)
					.HasColumnName("RegionParticipantRole");

				entity.Property(e => e.RegionTeamOwnerRoleId)
					.HasColumnName("RegionTeamOwnerRole");

				entity.HasOne(d => d.Tournament)
					.WithMany(p => p.RegistrationRules)
					.HasForeignKey(d => d.TournamentId);

				entity.HasOne(d => d.RegionParticipantRole)
					.WithMany()
					.HasForeignKey(d => d.RegionParticipantRoleId);

				entity.HasOne(d => d.RegionTeamOwnerRole)
					.WithMany()
					.HasForeignKey(d => d.RegionTeamOwnerRoleId);
			})

			.Entity<TournamentClaim>(entity =>
			{
				entity.ToTable(nameof(TournamentClaim))
					.HasKey(e => new { e.TournamentId, e.AccountId, e.ClaimId });

				entity.HasOne(d => d.Tournament)
					.WithMany(p => p.Claims)
					.HasForeignKey(d => d.TournamentId);

				entity.HasOne(d => d.Account)
					.WithMany(p => p.TournamentClaims)
					.HasForeignKey(d => d.AccountId);

				entity.HasOne(d => d.Claim)
					.WithMany(p => p.TournamentClaims)
					.HasForeignKey(d => d.ClaimId);
			})

			.Entity<TournamentStage>(entity =>
			{
				entity.ToTable(nameof(TournamentStage))
					.HasKey(e => e.StageId);

				entity.Property(e => e.Name)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode();

				entity.HasOne(d => d.Tournament)
					.WithMany(p => p.Stages)
					.HasForeignKey(d => d.TournamentId);
			})

			.Entity<TournamentGroup>(entity =>
			{
				entity.ToTable(nameof(TournamentGroup))
					.HasKey(e => e.GroupId);

				entity.Property(e => e.Name)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode();

				entity.HasOne(d => d.Stage)
					.WithMany(p => p.Groups)
					.HasForeignKey(d => d.TournamentStageId);
			})

			.Entity<TournamentTeam>(entity =>
			{
				entity.ToTable(nameof(TournamentTeam))
					.HasKey(e => e.TeamId);

				entity.Property(e => e.OwnerAccountId)
					.HasColumnName("Owner");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(255)
					.IsUnicode();

				entity.Property(e => e.Tag)
					.IsRequired()
					.HasMaxLength(5)
					.IsUnicode(false);

				entity.Property(e => e.Description)
					.IsRequired(false)
					.HasMaxLength(1000)
					.IsUnicode();

				entity.Property(e => e.Icon)
					.IsRequired(false)
					.HasMaxLength(255)
					.IsUnicode(false);

				entity.HasOne(d => d.Tournament)
					.WithMany(p => p.Teams)
					.HasForeignKey(d => d.TournamentId);

				entity.HasOne(d => d.Owner)
					.WithMany(p => p.OwnedTeams)
					.HasForeignKey(d => d.OwnerAccountId);
			})

			.Entity<TournamentTeamClaim>(entity =>
			{
				entity.ToTable(nameof(TournamentTeamClaim))
					.HasKey(e => new { e.TeamId, e.AccountId, e.ClaimId });

				entity.HasOne(d => d.Team)
					.WithMany(p => p.Claims)
					.HasForeignKey(d => d.TeamId);

				entity.HasOne(d => d.Account)
					.WithMany(p => p.TeamClaims)
					.HasForeignKey(d => d.AccountId);

				entity.HasOne(d => d.Claim)
					.WithMany(p => p.TeamClaims)
					.HasForeignKey(d => d.ClaimId);
			})

			.Entity<TournamentSeed>(entity =>
			{
				entity.ToTable(nameof(TournamentSeed))
					.HasKey(e => e.GroupSeedId);

				entity.HasAlternateKey(e => new { e.GroupId, e.Seed });

				entity.HasOne(d => d.Group)
					.WithMany(p => p.Seeds)
					.HasForeignKey(d => d.GroupId);

				entity.HasOne(d => d.Team)
					.WithMany(p => p.Seeds)
					.HasForeignKey(d => d.TeamId);
			})

			.Entity<TournamentMatch>(entity =>
			{
				entity.ToTable(nameof(TournamentMatch))
					.HasKey(e => e.MatchId);

				entity.HasOne(d => d.Group)
					.WithMany(p => p.Matches)
					.HasForeignKey(d => d.GroupId);

				entity.HasOne(d => d.AlphaTeam)
					.WithMany(p => p.AlphaMatches)
					.HasForeignKey(d => d.AlphaTeamId);

				entity.HasOne(d => d.BravoTeam)
					.WithMany(p => p.BravoMatches)
					.HasForeignKey(d => d.BravoTeamId);
			})

			.Entity<TournamentGame>(entity =>
			{
				entity.ToTable(nameof(TournamentGame))
					.HasKey(e => e.GameId);

				entity.HasOne(d => d.Match)
					.WithMany(p => p.Games)
					.HasForeignKey(d => d.MatchId);

				entity.HasOne(d => d.WinningTeam)
					.WithMany(p => p.GamesWon)
					.HasForeignKey(d => d.WinnerTeamId);

				entity.HasOne(d => d.Map)
					.WithMany(p => p.Games)
					.HasForeignKey(d => d.MapId);
			})

			.Entity<TournamentParticipant>(entity =>
			{
				entity.ToTable(nameof(TournamentParticipant))
					.HasKey(e => e.ParticipantId);

				entity.HasOne(d => d.Team)
					.WithMany(p => p.Participants)
					.HasForeignKey(d => d.TeamId);

				entity.HasOne(d => d.Player)
					.WithMany(p => p.Participations)
					.HasForeignKey(d => d.PlayerId);
			})

			;

		#endregion
	}

	public static class ContextExtensions
	{
		internal static async Task<EntityEntry<T>> AddOrUpdateAsync<T> (this DbSet<T> set, T entity, Expression<Func<T, bool>> comparer) where T: class
		{
			return (await set.AsNoTracking().AnyAsync(comparer))
				? set.Update(entity)
				: set.Add(entity);
		}

		internal static R ConvertObject<T, R> (this T obj) where T : class where R : class
		{
			return obj is null
				? null
				: JsonConvert.DeserializeObject<R>(JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
		}

		public static IServiceCollection AddDataContextPool (this IServiceCollection services, int poolSize)
		{
			return services.AddDbContextPool<Context>((serviceProvider, options) =>
			{
				if (!options.IsConfigured)
				{
					var config = serviceProvider.GetRequiredService<IConfiguration>();
					options
						.UseLazyLoadingProxies()
						.UseSqlServer(config["ConnectionStrings:WoWsPro"])
						.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning));
				}
			}, poolSize);
		}
	}
}
