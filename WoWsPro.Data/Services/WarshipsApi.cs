using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;

namespace WoWsPro.Data.Services
{
	public interface IWarshipsApi
	{
		Task<IEnumerable<(long, string)>> SearchPlayerAsync (Region region, string search);
		Task<WarshipsPlayer> GetPlayerInfoAsync (Region region, long id);
	}

	public class WarshipsApi : IWarshipsApi, IDisposable
	{
		IConfiguration Configuration { get; }
		string ApiKey { get; }
		Dictionary<Region, HttpClient> Http { get; }

		public WarshipsApi (IConfiguration configuration)
		{
			Configuration = configuration;
			ApiKey = Configuration["ApiKeys:Wargaming"];
			Http = new Dictionary<Region, HttpClient>
			{
				[Region.NA] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.com/wows/") },
				[Region.EU] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.eu/wows/") },
				[Region.CIS] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.ru/wows/") },
				[Region.APAC] = new HttpClient() { BaseAddress = new Uri("https://api.worldofwarships.asia/wows/") }
			};

			foreach (var client in Http.Values)
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}
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
			using var response = await Http[region].GetAsync($"account/list/{param}");
			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadAsAsync<ApiResponse<List<dynamic>>>();
				if (result.Ok)
				{
					return result.data.Select(d => ((long)d.account_id, (string)d.nickname));
				}
				else
				{
					throw new WarshipsApiException(result.error);
				}
			}
			else
			{
				throw new HttpRequestException(response.ReasonPhrase);
			}
		}
		public async Task<WarshipsPlayer> GetPlayerInfoAsync (Region region, long id)
		{
			var param = new ParamList(ApiKey)
				.Add("account_id", id.ToString())
				;

			var player = new WarshipsPlayer()
			{
				PlayerId = id,
				Region = region
			};

			using var responseInfo = await Http[region].GetAsync($"account/info/{param}");
			if (responseInfo.IsSuccessStatusCode)
			{
				var result = await responseInfo.Content.ReadAsAsync<ApiResponse<Dictionary<string, dynamic>>>();
				if (result.Ok)
				{
					var info = result.data.Values.First();
					player.Created = FromEpoch((long?)info.created_at);
					player.Nickname = (string)info.nickname;
				}
				else
				{
					throw new WarshipsApiException(result.error);
				}
			}
			else
			{
				throw new HttpRequestException(responseInfo.ReasonPhrase);
			}

			using var responseClan = await Http[region].GetAsync($"clans/accountinfo/{param}");
			if (responseClan.IsSuccessStatusCode)
			{
				var result = await responseClan.Content.ReadAsAsync<ApiResponse<Dictionary<string, dynamic>>>();
				if (result.Ok)
				{
					var info = result.data.Values.First();
					if (info != null)
					{
						player.ClanId = (long?)info.clan_id;
						player.JoinedClan = FromEpoch((long?)info.joined_at);
						player.ClanRole = (string)info.role;
					}
				}
				else
				{
					throw new WarshipsApiException(result.error);
				}
			}
			else
			{
				throw new HttpRequestException(responseClan.ReasonPhrase);
			}

			return player;
		}
		public void Dispose ()
		{
			foreach (var client in Http.Values)
			{
				client.Dispose();
			}
		}

		public static DateTime? FromEpoch (long? epoch) => epoch is long epoc ? (DateTime?)new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoc) : null;
	}

	public static class WarshipsApiProvider
	{
		public static IServiceCollection AddWarshipsApi (this IServiceCollection services)
			=> services.AddSingleton<IWarshipsApi, WarshipsApi>();
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

	internal class ApiResponse<T>
	{
		public string status { get; set; }
		public Meta meta { get; set; }
		public Error error { get; set; }
		public T data { get; set; }

		public bool Ok => status == "ok";

	}

	internal class Meta
	{
		public int? count { get; set; }
		public int? page_total { get; set; }
		public int? total { get; set; }
		public int? limit { get; set; }
		public int? page { get; set; }
	}

	internal class Error
	{
		public string field { get; set; }
		public string message { get; set; }
		public int code { get; set; }
		public string value { get; set; }
	}

	public class WarshipsApiException : Exception
	{
		public string Field { get; set; }
		public string Info { get; set; }
		public int Code { get; set; }
		public string Value { get; set; }

		internal WarshipsApiException (Error error) : base(
			$"The Warships API returned the following error:\r\n" +
			$"\tmessage: {error.message}\r\n" +
			$"\tcode: {error.code}\r\n" +
			$"\tfield: {error.field}\r\n" +
			$"\tvalue: {error.value}"
			)
		{
			Field = error.field;
			Info = error.message;
			Code = error.code;
			Value = error.value;
		}
	}

}
