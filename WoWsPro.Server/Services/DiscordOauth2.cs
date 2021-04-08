using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WoWsPro.Shared.Models.Discord;
using WoWsPro.Shared.Utils;

namespace WoWsPro.Server.Services
{
	public interface IDiscordOauth2
	{
		object GetRequestBody (string returnUrl);
		Task<DiscordToken> GetTokenAsync (string returnUrl, string code, string state);
		Task<DiscordUser> GetUserAsync (string token);
	}

	public class Oauth2State
	{
		public Nonce State => Nonce.NonceBatch(Key)[0];

		private string Key { get; }

		public Oauth2State (ISettings settings) {
			Key = settings.OauthSalt;
		}

		public bool Matches (string state)
		{
			return Nonce.NonceBatch(Key, 2).Any(n => n.Matches(state));
		}

		public class Nonce
		{
			public string State { get; set; }

			public Nonce () { }

			public static Nonce[] NonceBatch (string salt, int count = 1)
			{
				using var sha = SHA256.Create();
				var now = DateTime.UtcNow;
				var nonces = new Nonce[count];
				for (int i = 0; i < nonces.Length; i++)
				{
					string key = $"{salt}{(now - TimeSpan.FromMinutes(5 * i)).Ticks / TimeSpan.FromMinutes(5).Ticks}";
					var bytes = Encoding.UTF8.GetBytes(key);
					var hash = sha.ComputeHash(bytes);
					string state = Convert.ToBase64String(hash);
					nonces[i] = new Nonce()
					{
						State = state
					};
				}
				return nonces;
			}

			public bool Matches (string nonce) => State == nonce;

			public override string ToString () => State;

			public static implicit operator string (Nonce nonce) => nonce.State;
		}
	}

	public class DiscordHttpClient : HttpClient
	{
		public DiscordHttpClient ()
		{
			BaseAddress = new Uri($"{"https:"}//discordapp.com/api/");
			DefaultRequestHeaders.Accept.Clear();
		}
	}

	public class DiscordOauth2 : IDiscordOauth2
	{
		private readonly string _scope = "identify guilds guilds.join";

		DiscordHttpClient Http { get; }
		Oauth2State Oauth2State { get; }
		ISettings Settings { get; }

		public DiscordOauth2 (ISettings settings, DiscordHttpClient http, Oauth2State state)
		{
			Http = http;
			Settings = settings;
			Oauth2State = state;
		}

		public object GetRequestBody (string returnUrl)
		{
			return new HtmlFormRequest
			{
				RequestAddress = $"{"https:"}//discordapp.com/api/oauth2/authorize",
				Body = new Dictionary<string, string>()
				{
					{ "response_type", "code" },
					{ "client_id", Settings.DiscordClientId },
					{ "scope", _scope },
					{ "state", Oauth2State.State },
					{ "redirect_uri", returnUrl },
					{ "prompt", "none" }
				}
			};
		}

		public async Task<DiscordToken> GetTokenAsync (string returnUrl, string code, string state)
		{
			if (Oauth2State.Matches(state))
			{
				var request = new HttpRequestMessage()
				{
					RequestUri = new Uri($"{Http.BaseAddress}oauth2/token"),
					Method = HttpMethod.Post
				};
				request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
				{
					{ "client_id", Settings.DiscordClientId },
					{ "client_secret", Settings.DiscordClientSecret },
					{ "grant_type", "authorization_code" },
					{ "code", code },
					{ "redirect_uri", returnUrl },
					{ "scope", _scope }
				});
				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				var response = await Http.SendAsync(request);
				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
					try
					{
						double seconds = result.expires_in;
						var token = new DiscordToken()
						{
							AccessToken = result.access_token,
							TokenType = result.token_type,
							Expires = DateTime.UtcNow.AddSeconds(seconds),
							RefreshToken = result.refresh_token,
							Scope = result.scope
						};

						// Get the Discord Id associated with this request
						var user = await GetUserAsync(token.AccessToken);
						token.DiscordId = user.DiscordId;
						token.DiscordUser = user;

						// FIXME: probably move more of the Discord API work to different library
						return token;
					}
					catch (Exception)
					{
						// FIXME: more specific exception
						throw new Exception("Failed to obtain token for request.");
					}
				}
				else
				{
					var error = await request.Content.ReadAsStringAsync();
					// FIXME: InvalidOperationException for this
					throw new Exception("Failed to obtain token for request. " + error);
				}
			}
			else
			{
				// FIXME: custom exception for this
				throw new InvalidOperationException("OAuth request was not initiated by this server.");
			}
		}

		public async Task<DiscordUser> GetUserAsync (string token)
		{
			var message = new HttpRequestMessage()
			{
				RequestUri = new Uri($"{Http.BaseAddress}users/@me"),
				Method = HttpMethod.Get
			};
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var response = await Http.SendAsync(message);
			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<UserResponse>();
				try
				{
					return new DiscordUser()
					{
						DiscordId = result.id,
						Username = result.username,
						Discriminator = result.discriminator,
						Avatar = result.avatar
					};
				}
				catch (Exception)
				{
					// FIXME: more specific exception
					throw new Exception("Failed to obtain user with token.");
				}
			}
			else
			{
				// FIXME: more specific exception
				throw new Exception("Failed to obtain user with token.");
			}
		}

		public class UserResponse {
			public long id { get; set; }
			public string username { get; set; }
			public string discriminator { get; set; }
			public string avatar { get; set; }
		}

		public class TokenResponse {
			public string access_token { get; set; }
			public string token_type { get; set; }
			public string refresh_token { get; set; }
			public string scope { get; set; }
			public double expires_in { get; set; }
		}
	}

	public static class DiscordOuath2Provider
	{
		public static IServiceCollection AddDiscordOauth2 (this IServiceCollection services)
			=> services
			.AddSingleton<Oauth2State>()
			.AddSingleton<DiscordHttpClient, DiscordHttpClient>()
			.AddScoped<IDiscordOauth2, DiscordOauth2>()
			;
	}
}
