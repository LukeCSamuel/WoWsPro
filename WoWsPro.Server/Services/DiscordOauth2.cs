using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using WoWsPro.Server.Extensions;
using WoWsPro.Shared;
using WoWsPro.Shared.Models;

namespace WoWsPro.Server.Services
{
	public interface IDiscordOauth2
	{
		object GetRequestBody (string returnUrl);
		Task<DiscordToken> GetTokenAsync (string returnUrl, string code, string state);
		Task<DiscordUser> GetUserAsync (string token);
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
		ISettings Settings { get; }
		ISession Session { get; }

		public DiscordOauth2 (ISettings settings, IHttpContextAccessor contextAccessor, DiscordHttpClient http)
		{
			Http = http;
			Settings = settings;
			Session = contextAccessor.HttpContext.Session;
		}

		public object GetRequestBody (string returnUrl)
		{
			var nonce = Session.Retrieve<Nonce>();
			if (nonce.State is null)
			{
				nonce = Nonce.NewNonce();
				Session.Store(nonce);
			}
			return new HtmlFormRequest
			{
				RequestAddress = $"{"https:"}//discordapp.com/api/oauth2/authorize",
				Body = new Dictionary<string, string>()
				{
					{ "response_type", "code" },
					{ "client_id", Settings.DiscordClientId },
					{ "scope", _scope },
					{ "state", nonce },
					{ "redirect_uri", returnUrl },
					{ "prompt", "none" }
				}
			};
		}

		public async Task<DiscordToken> GetTokenAsync (string returnUrl, string code, string state)
		{
			var nonce = Session.Retrieve<Nonce>();
			if (nonce.Matches(state))
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

				var response = await Http.SendAsync(request);
				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadAsAsync<dynamic>();
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
					// FIXME: InvalidOperationException for this
					throw new Exception("Failed to obtain token for request.");
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
			message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

			var response = await Http.SendAsync(message);
			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadAsAsync<dynamic>();
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

		class Nonce
		{
			[SessionVar]
			public string State { get; set; }

			public Nonce () { }

			public static Nonce NewNonce () => new Nonce()
			{
				State = TokenGenerator.GenerateToken()
			};

			public bool Matches (string nonce) => State == nonce;

			public override string ToString () => State;

			public static implicit operator string (Nonce nonce) => nonce.State;
		}
	}

	public static class DiscordOuath2Provider
	{
		public static IServiceCollection AddDiscordOauth2 (this IServiceCollection services)
			=> services
			.AddSingleton<DiscordHttpClient, DiscordHttpClient>()
			.AddScoped<IDiscordOauth2, DiscordOauth2>()
			;
	}
}
