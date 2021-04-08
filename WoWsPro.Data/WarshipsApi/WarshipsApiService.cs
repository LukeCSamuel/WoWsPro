using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.WarshipsApi.Models;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Warships;

namespace WoWsPro.Data.WarshipsApi
{
	public interface IWarshipsApi
	{
		Task<IEnumerable<(long Id, string Nickname)>> SearchPlayerAsync (Region region, string search);
		Task<WarshipsPlayer> GetPlayerInfoAsync (Region region, long id);
	}

	public class WarshipsApi : IWarshipsApi
	{
		static Dictionary<Region, HttpClient> Http { get; }
		static WarshipsApi ()
		{
			Http = new Dictionary<Region, HttpClient>
			{
				[Region.NA] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.com/wows/") },
				[Region.EU] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.eu/wows/") },
				[Region.CIS] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.ru/wows/") },
				[Region.SEA] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.asia/wows/") }
			};

			foreach (var client in Http.Values)
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}

			AppDomain.CurrentDomain.ProcessExit += (_, e) =>
			{
				foreach (var client in Http.Values)
				{
					client.Dispose();
				}
			};
		}

		IConfiguration Configuration { get; }
		string ApiKey { get; }
		Context DbContext { get; }

		public WarshipsApi (IConfiguration configuration, Context context)
		{
			DbContext = context;
			Configuration = configuration;
			ApiKey = Configuration["ApiKeys:Wargaming"];
		}

		public async Task<IEnumerable<(long, string)>> SearchPlayerAsync (Region region, string search)
		{
			if (search is null || search.Length < 3)
			{
				throw new ArgumentException($"Invalid search string '{search}'; search string must be at least 3 characters.", nameof(search));
			}

			var param = new ParamList(ApiKey)
				.Add("search", search)
				;

			var result = await Http[region].GetFromJsonAsync<ApiResponse<List<AccountSearch>>>($"account/list/{param}");
			if (result.Ok)
			{
				return result.data.Select(d => (d.AccountId, d.Nickname));
			}
			else
			{
				throw new WarshipsApiException(result.error);
			}
		}

		/// <summary>
		/// Gets information for the given player identity.  If the player is found, it is added to the database.
		/// </summary>
		public async Task<WarshipsPlayer> GetPlayerInfoAsync (Region region, long id)
		{
			var param = new ParamList(ApiKey)
				.Add("account_id", id.ToString())
				;

			var player = DbContext.WarshipsPlayers.SingleOrDefault(p => p.PlayerId == id)
				?? new WarshipsPlayer()
				{
					PlayerId = id,
					Region = region
				};

			var resultPlayer = await Http[region].GetFromJsonAsync<ApiResponse<Dictionary<string, AccountInfo>>>($"account/info/{param}");
			if (resultPlayer.Ok)
			{
				var info = resultPlayer.data.Values.First();
				player.Created = info.Created;
				player.Nickname = info.Nickname;
			}
			else
			{
				throw new WarshipsApiException(resultPlayer.error);
			}

			var resultClan = await Http[region].GetFromJsonAsync<ApiResponse<Dictionary<string, ClanAccountInfo>>>($"clans/accountinfo/{param}");
			if (resultClan.Ok)
			{
				var info = resultClan.data.Values.First();
				if (info != null)
				{
					// Clan information, update clan in database > if clan isn't currently in database!
					if (info.ClanId is long clanId && !await DbContext.WarshipsClans.AnyAsync(c => c.ClanId == clanId))
					{
						await UpdateClanInfoAsync(region, clanId);
					}

					player.ClanId = info.ClanId;
					player.JoinedClan = info.JoinedAt;
					player.ClanRole = info.Role;
				}
			}
			else
			{
				throw new WarshipsApiException(resultClan.error);
			}

			// Add player to database
			var existing = await DbContext.WarshipsPlayers.AsTracking().SingleOrDefaultAsync(p => p.PlayerId == player.PlayerId);
			if (existing is not null)
			{
				existing.ClanId = player.ClanId;
				existing.ClanRole = player.ClanRole;
				existing.Created = player.Created;
				existing.Nickname = player.Nickname;
				existing.Region = player.Region;
				await DbContext.SaveChangesAsync();
				return existing;
			}
			else
			{
				var entry = await DbContext.WarshipsPlayers.AddOrUpdateAsync(player, p => p.PlayerId == player.PlayerId);
				await DbContext.SaveChangesAsync();
				return entry.Entity;
			}
		}

		private async Task UpdateClanInfoAsync (Region region, long id)
		{
			var param = new ParamList(ApiKey)
				.Add("clan_id", id.ToString())
				;

			var clan = await DbContext.WarshipsClans.SingleOrDefaultAsync(c => c.ClanId == id) 
				?? new WarshipsClan()
				{
					ClanId = id,
					Region = region
				};

			var result = await Http[region].GetFromJsonAsync<ApiResponse<Dictionary<string, ClanInfo>>>($"clans/info/{param}");
			if (result.Ok)
			{
				var info = result.data.Values.First();
				if (info != null)
				{
					clan.Created = info.CreatedAt ?? new DateTime(1970, 1, 1, 0, 0, 0);
					clan.MemberCount = info.MemberCount;
					clan.Name = info.Name;
					clan.Tag = info.Tag;
				}
			}
			else
			{
				throw new WarshipsApiException(result.error);
			}

			await DbContext.WarshipsClans.AddOrUpdateAsync(clan, c => c.ClanId == clan.ClanId);
			await DbContext.SaveChangesAsync();
		}

		public static DateTime? FromEpoch (long? epoch) => epoch is long epoc ? (DateTime?)new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoc) : null;
	}

	public static class WarshipsApiProvider
	{
		public static IServiceCollection AddWarshipsApi (this IServiceCollection services)
			=> services.AddScoped<IWarshipsApi, WarshipsApi>();
	}

	internal class ParamList
	{
		public List<(string Key, string Value)> Params { get; }

		public ParamList (string apiKey)
		{
			Params = new List<(string, string)>();
			Params.Add(("application_id", apiKey));
		}

		public ParamList Add (string key, string value)
		{
			Params.Add((key, value));
			return this;
		}

		public static implicit operator string (ParamList p) => p.ToString();
		public override string ToString () => $"?{string.Join("&", Params.Select(p => $"{p.Key}={p.Value}"))}";
	}
}
