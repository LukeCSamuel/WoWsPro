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
	}

	public class DiscordOauth2 : IDisposable, IDiscordOauth2
	{
		private readonly string _scope = "identify guilds guilds.join";

		HttpClient Http { get; }
		ISettings Settings { get; }
		ISession Session { get; }

		public DiscordOauth2 (ISettings settings, IHttpContextAccessor contextAccessor)
		{
			Http = new HttpClient()
			{
				BaseAddress = new Uri($"{"https:"}//discordapp.com/api/oauth2/")
			};
			Http.DefaultRequestHeaders.Accept.Clear();
			Settings = settings;
			Session = contextAccessor.HttpContext.Session;
		}

		public object GetRequestBody (string returnUrl)
		{
			var nonce = new Nonce();
			Session.Store(nonce);
			return new HtmlFormRequest
			{
				RequestAddress = $"{"https:"}//discordapp.com/api/oauth2/authorize/",
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
				var response = await Http.PostAsync("token", new
				{
					client_id = Settings.DiscordClientId,
					client_secret = Settings.DiscordClientSecret,
					grant_type = "authorization_code",
					code,
					redirect_uri = returnUrl,
					scope = _scope
				},
				new FormUrlEncodedMediaTypeFormatter());
				if (response.IsSuccessStatusCode)
				{
					// TODO: this isn't going to work, there is some conversion that must be done to write to DiscordToken
					//       need to do an immediate discord API request to get the user id to build/connect to DiscordUser object in database
					return await response.Content.ReadAsAsync<DiscordToken>();
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

		public void Dispose () => ((IDisposable)Http).Dispose();

		class Nonce
		{
			[SessionVar]
			public string State { get; }

			public Nonce () => State = TokenGenerator.GenerateToken();

			public bool Matches (string nonce) => State == nonce;

			public override string ToString () => State;

			public static implicit operator string (Nonce nonce) => nonce.State;
		}
	}

	public static class DiscordOuath2Provider
	{
		public static IServiceCollection AddDiscordOauth2 (this IServiceCollection services)
			=> services
			.AddSingleton<IDiscordOauth2, DiscordOauth2>()
			;
	}
}
