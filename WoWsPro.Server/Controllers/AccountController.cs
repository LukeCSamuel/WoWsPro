using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WoWsPro.Data.Operations;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Server.Extensions;
using WoWsPro.Server.Services;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Exceptions;
using WoWsPro.Shared.Models;

namespace WoWsPro.Server.Controllers
{
    [ApiController]
    public class AccountController : WowsProApiController
    {
		private readonly IUserService _user;
		private readonly ISettings _settings;
		private readonly IWGOpenId _wgOpenId;
		private readonly IWarshipsApi _warshipsApi;
		private readonly IDiscordOauth2 _discordOauth2;
		AccountOperations Accounts { get; }

		public AccountController (
			IUserService userService,
			ISettings settings,
			IWGOpenId wgOpenId,
			IDiscordOauth2 discordOauth2,
			IWarshipsApi warshipsApi,
			AccountOperations accounts)
		{
			_user = userService;
			_settings = settings;
			_wgOpenId = wgOpenId;
			_warshipsApi = warshipsApi;
			_discordOauth2 = discordOauth2;
			Accounts = accounts;
		}

		[HttpGet("api/[controller]/User")]
		public IActionResult GetUser () => Ok(_user.User);

		[HttpPost("api/[controller]/Referer")]
		public IActionResult SetReferer (
			[FromBody] string referer)
		{
			try
			{
				_user.Session.Store(new Referer(referer));
				return Ok();
			}
			catch (Exception ex)
			{
                return Error(ex);
            }
		}

		[HttpGet("[controller]/Logout")]
		public IActionResult Logout ()
		{
			_user.Logout();
			return RedirectToReferer();
		}

		[HttpGet("api/[controller]/DiscordId/RequestBody")]
		public IActionResult GetDiscordOAuth2RequestBody ()
		{
			try
			{
				return Ok(_discordOauth2.GetRequestBody($"{_settings.BaseUrl}Account/DiscordId/Login"));
			}
			catch (Exception ex)
			{
                return Error(ex);
			}
		}

		[HttpGet("[controller]/DiscordId/Login")]
		public async Task<IActionResult> DiscordIdLoginAsync (
			[FromQuery(Name = "code")] string code,
			[FromQuery(Name = "state")] string state)
		{
			try
			{
				var token = await _discordOauth2.GetTokenAsync($"{_settings.BaseUrl}Account/DiscordId/Login", code, state);

				_user.Login(token);
				return RedirectToReferer();
			}
			catch (Exception ex)
			{
                return Error(ex);
			}
		}

		[HttpGet("api/[controller]/WGOpenId/RequestBody/{region}")]
		public IActionResult GetWGOpenIdRequestBody (string region)
		{
			try
			{
				return Ok(_wgOpenId.GetRequestBody(RegionExtensions.FromString(region), $"{_settings.BaseUrl}Account/WGOpenId/Login/{region}"));
			}
			catch (Exception ex)
			{
                return Error(ex);
			}
		}

		[HttpGet("[controller]/WGOpenId/Login/{region}")]
		public async Task<IActionResult> WGOpenIdLoginAsync (string region,
			[FromQuery(Name = "openid.assoc_handle")] string assocHandle,
			[FromQuery(Name = "openid.claimed_id")] string claimedId)
		{
			try
			{
				var verification = _wgOpenId.VerifyLogin(RegionExtensions.FromString(region), claimedId, assocHandle, HttpContext.Request.Query);
				if (verification is null)
				{
					return BadRequest();
				}
				else
				{
					(long id, string nickname) = ((long, string))verification;

					var player = await _warshipsApi.GetPlayerInfoAsync(RegionExtensions.FromString(region), id);
					_user.Login(player);
					return RedirectToReferer();
				}
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { Reason = ex.Message });
			}
			catch
			{
				return StatusCode(500);
			}
		}

		[HttpGet("api/[controller]/{id:long}")]
		public async Task<IActionResult> GetAccountAsync (long id)
		{
			try
			{
				var result = await Accounts.GetAccountAsync(id);
                return Ok(result);
            }
			catch (Exception ex)
			{
                return Error(ex);
			}
		}

		[HttpGet("api/[controller]")]
		public async Task<IActionResult> GetAccountAsync ()
		{
			if (_user.User.AccountId is null)
			{
                return BadRequest();
            }

			try
			{
				var result = await Accounts.GetAccountAsync((long)_user.User.AccountId);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return Error(ex);
			}
        }

		[HttpPost("api/[controller]/primary/discord")]
		public async Task<IActionResult> SetPrimaryDiscordUserAsync ([FromBody] long discordId)
		{
            if (_user.User.AccountId is null)
            {
                return BadRequest();
            }

			try
			{
				await Accounts.SetPrimaryDiscordUserAsync((long)_user.User.AccountId, discordId);
                return Ok();
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
		}

		[HttpPost("api/[controller]/primary/warships")]
		public async Task<IActionResult> SetPrimaryWarshipsPlayerAsync ([FromBody] long playerId)
		{
            if (_user.User.AccountId is null)
            {
                return BadRequest();
            }

			try
			{
				await Accounts.SetPrimaryWarshipsPlayerAsync((long)_user.User.AccountId, playerId);
                return Ok();
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
		}

		IActionResult RedirectToReferer ()
		{
			var referer = _user.Session.Retrieve<Referer>();
			_user.Session.Store(new Referer());
			return Redirect(referer.Uri);
		}


		class Referer
		{
			[SessionVar]
			public string Uri { get; set; } = "/";

			public Referer () { }
			public Referer (string uri) => Uri = uri;
		}
    }
}