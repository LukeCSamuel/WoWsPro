using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Utils;

namespace WoWsPro.Server.Services
{
	public interface IWGOpenId
	{
		/// <summary>
		/// Gets or creates the OpenId association for the specified region.
		/// </summary>
		/// <param name="region">The region on which the association should be created.</param>
		OpenIdAssociation GetAssociation (Region region);

		/// <summary>
		/// Gets or creates the OpenId association for the specified region.
		/// </summary>
		/// <param name="region">The region on which the association should be created.</param>
		Task<OpenIdAssociation> GetAssociationAsync (Region region);

		/// <summary>
		/// Generates the request body required to initialize the OpenId process.
		/// </summary>
		/// <param name="region">The region on which the OpenId process should be performed.</param>
		/// <param name="returnUrl">The url to which the result of the OpenId request should be redirected.</param>
		object GetRequestBody (Region region, string returnUrl);

		/// <summary>
		/// Generates the request body required to initialize the OpenId process.
		/// </summary>
		/// <param name="region">The region on which the OpenId process should be performed.</param>
		/// <param name="returnUrl">The url to which the result of the OpenId request should be redirected.</param>
		Task<object> GetRequestBodyAsync (Region region, string returnUrl);

		/// <summary>
		/// Verififes an OpenID redirect back to the server.
		/// </summary>
		/// <param name="region">Originating region</param>
		/// <param name="claimedId">ID of user</param>
		/// <param name="assoc">Association used</param>
		/// <param name="query">Query parameters of redirect</param>
		(long id, string nickname)? VerifyLogin (Region region, string claimedId, string assoc, IQueryCollection query);
		/// <summary>
		/// Verififes an OpenID redirect back to the server.
		/// </summary>
		/// <param name="region">Originating region</param>
		/// <param name="claimedId">ID of user</param>
		/// <param name="assoc">Association used</param>
		/// <param name="query">Query parameters of redirect</param>
		Task<(long id, string nickname)?> VerifyLoginAsync (Region region, string claimedId, string assoc, IQueryCollection query);
	}

	public class WGOpenId : IWGOpenId
	{
		Dictionary<Region, OpenIdAssociation> Associations { get; } = new Dictionary<Region, OpenIdAssociation>()
		{
			{ Region.NA, null },
			{ Region.EU, null },
			{ Region.CIS, null },
			{ Region.SEA, null }
		};

		private readonly IWGOpenIdHttpService _http;
		private readonly ISettings _settings;

		public WGOpenId (IWGOpenIdHttpService httpProvider, ISettings settings)
		{
			_http = httpProvider;
			_settings = settings;
		}

		/// <summary>
		/// Gets or creates the OpenId association for the specified region.
		/// </summary>
		/// <param name="region">The region on which the association should be created.</param>
		public OpenIdAssociation GetAssociation (Region region) => GetAssociationAsync(region).Result;

		/// <summary>
		/// Gets or creates the OpenId association for the specified region.
		/// </summary>
		/// <param name="region">The region on which the association should be created.</param>
		public async Task<OpenIdAssociation> GetAssociationAsync (Region region)
		{
			if (!Associations[region]?.IsExpired ?? false)
			{
				return Associations[region];
			}

			HttpContent body = new OpenIdBody()
				.AddKey("mode", "associate")
				.AddKey("assoc_type", "HMAC-SHA256")
				.AddKey("session_type", "no-encryption");

			using var response = await _http[region].PostAsync("openid/", body);
			if (response.IsSuccessStatusCode)
			{
				string result = await response.Content.ReadAsStringAsync();
				Associations[region] = new OpenIdAssociation(result);
				return Associations[region];
			}
			else
			{
				throw new Exception($"OpenId association request to {_http[region].BaseAddress} failed.{Environment.NewLine}Reason: {response.ReasonPhrase}");
			}
		}

		/// <summary>
		/// Generates the request body required to initialize the OpenId process.
		/// </summary>
		/// <param name="region">The region on which the OpenId process should be performed.</param>
		/// <param name="returnUrl">The url to which the result of the OpenId request should be redirected.</param>
		public object GetRequestBody (Region region, string returnUrl) => GetRequestBodyAsync(region, returnUrl).GetAwaiter().GetResult();

		/// <summary>
		/// Generates the request body required to initialize the OpenId process.
		/// </summary>
		/// <param name="region">The region on which the OpenId process should be performed.</param>
		/// <param name="returnUrl">The url to which the result of the OpenId request should be redirected.</param>
		public async Task<object> GetRequestBodyAsync (Region region, string returnUrl)
		{
			string handle = (await GetAssociationAsync(region)).Handle;
			return new HtmlFormRequest
			{
				RequestAddress = $"https://{region.Subdomain()}.wargaming.net/id/openid/",
				Body = new Dictionary<string, string>()
				{
					{ "ns", "http://specs.openid.net/auth/2.0" },
					{ "mode", "checkid_setup" },
					{ "return_to", returnUrl },
					{ "realm", _settings.BaseUrl },
					{ "claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" },
					{ "identity", "http://specs.openid.net/auth/2.0/identifier_select" },
					{ "assoc_handle", handle }
				}
			};
		}

		/// <summary>
		/// Verififes an OpenID redirect back to the server.
		/// </summary>
		/// <param name="region">Originating region</param>
		/// <param name="claimedId">ID of user</param>
		/// <param name="assoc">Association used</param>
		/// <param name="query">Query parameters of redirect</param>
		public (long id, string nickname)? VerifyLogin (Region region, string claimedId, string assoc, IQueryCollection query) => VerifyLoginAsync(region, claimedId, assoc, query).GetAwaiter().GetResult();

		/// <summary>
		/// Verififes an OpenID redirect back to the server.
		/// </summary>
		/// <param name="region">Originating region</param>
		/// <param name="claimedId">ID of user</param>
		/// <param name="assoc">Association used</param>
		/// <param name="query">Query parameters of redirect</param>
		public async Task<(long id, string nickname)?> VerifyLoginAsync (Region region, string claimedId, string assoc, IQueryCollection query)
		{
			var match = Regex.Match(claimedId, @"https://(.+)\.wargaming\.net/id/(\d*)-(.*)/");
			var association = await GetAssociationAsync(region);
			if (match.Success && assoc == association.Handle)
			{
				long id = long.Parse(match.Groups[2].Value);
				string nickname = match.Groups[3].Value;

				string signature = query["openid.sig"];

				// Key-Value Form encode the requested parameters with UTF-8
				string form = "";
				foreach (string key in query["openid.signed"].ToString().Split(','))
				{
					form += $"{key}:{query[$"openid.{key}"]}\n";
				}
				byte[] bytes = Encoding.UTF8.GetBytes(form);

				// Use HMAC-SHA256 to generate hash signature of the form
				using (var hmac = new HMACSHA256(Convert.FromBase64String(association.Key)))
				{
					string hash = Convert.ToBase64String(hmac.ComputeHash(bytes));
					if (hash != signature)
					{
						// Unable to verify signature
						return null;
					}
				}

				// Authentication succeeded
				return (id, nickname);
			}
			else
			{
				// Authenticatoin failed
				return null;
			}
		}


		internal class OpenIdBody : IEnumerable<KeyValuePair<string, string>>
		{
			public string this[string key]
			{
				get => Content["openid." + key];
				set => Content["openid." + key] = value;
			}
			Dictionary<string, string> Content { get; set; }

			public OpenIdBody ()
			{
				Content = new Dictionary<string, string>();
				this["ns"] = "http://specs.openid.net/auth/2.0";
			}

			public OpenIdBody AddKey (string key, string value)
			{
				this[key] = value;
				return this;
			}

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator () => ((IEnumerable<KeyValuePair<string, string>>)Content).GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator () => ((IEnumerable<KeyValuePair<string, string>>)Content).GetEnumerator();

			public static implicit operator HttpContent (OpenIdBody obj) => new FormUrlEncodedContent(obj);
		}

	}

	public class OpenIdAssociation
	{
		public string Handle { get; }
		public string Type { get; }
		public DateTime Expires { get; }
		public bool IsExpired => DateTime.Now > Expires;
		public string Key { get; }


		public OpenIdAssociation (string requestResult)
		{
			string[] keyvals = requestResult.Trim().Split('\n');
			var dict = new Dictionary<string, string>();
			foreach (string keyval in keyvals)
			{
				var match = Regex.Match(keyval, @"(.*):(.*)");
				if (match.Success)
				{
					dict[match.Groups[1].Value.Trim()] = match.Groups[2].Value.Trim();
				}
				else
				{
					throw new ArgumentException($"Failed to parse line: `{keyval}`", nameof(requestResult));
				}
			}

			Handle = dict["assoc_handle"];
			Type = dict["assoc_type"];
			Key = dict["mac_key"];
			Expires = DateTime.Now + new TimeSpan(0, 0, int.Parse(dict["expires_in"]));
		}
	}

	public interface IWGOpenIdHttpService
	{
		HttpClient this[Region region] { get; }
	}

	public class WGHttpService : IDisposable, IWGOpenIdHttpService
	{
		public HttpClient this[Region region] => _clients[region];
		private readonly Dictionary<Region, HttpClient> _clients;

		public WGHttpService ()
		{
			_clients = new Dictionary<Region, HttpClient>();
			foreach (var region in (Region[])Enum.GetValues(typeof(Region)))
			{
				var client = new HttpClient()
				{
					BaseAddress = new Uri($"https://{region.Subdomain()}.wargaming.net/id/")
				};
				client.DefaultRequestHeaders.Accept.Clear();
				_clients[region] = client;
			}
		}

		public void Dispose ()
		{
			foreach (var client in _clients.Values)
			{
				((IDisposable)client).Dispose();
			}
		}
	}

	public static class WGOpenIdProvider
	{
		public static IServiceCollection AddWGOpenId (this IServiceCollection services)
			=> services
			.AddSingleton<IWGOpenIdHttpService, WGHttpService>()
			.AddSingleton<IWGOpenId, WGOpenId>()
			;
	}
}
