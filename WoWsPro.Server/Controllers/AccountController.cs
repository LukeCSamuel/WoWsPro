using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WoWsPro.Data.Exceptions;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;
using WoWsPro.Server.Extensions;
using WoWsPro.Server.Services;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;

namespace WoWsPro.Server.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
		private readonly IUserService _user;
		private readonly ISettings _settings;
		private readonly IWGOpenId _wgOpenId;
		private readonly IWarshipsApi _warshipsApi;
		private readonly IDiscordOauth2 _discordOauth2;
		IAuthorizer<AccountOperations> Accounts { get; }

		public AccountController (
			IUserService userService,
			ISettings settings,
			IWGOpenId wgOpenId,
			IDiscordOauth2 discordOauth2,
			IWarshipsApi warshipsApi,
			IAuthorizer<AccountOperations> accounts)
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
			catch
			{
				return StatusCode(500);
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
			catch
			{
				// FIXME: refine exceptions
				return StatusCode(500);
			}
		}

		[HttpGet("[controller]/DiscordId/Login")]
		public IActionResult DiscordIdLogin (
			[FromQuery(Name = "code")] string code,
			[FromQuery(Name = "state")] string state)
		{
			try
			{
				var token = _discordOauth2.GetTokenAsync($"{_settings.BaseUrl}Account/DiscordId/Login", code, state).GetAwaiter().GetResult();

				_user.Login(token);
				return RedirectToReferer();
			}
			catch
			{
				// FIXME: refine exceptions
				return StatusCode(500);
			}
		}

		[HttpGet("api/[controller]/WGOpenId/RequestBody/{region}")]
		public IActionResult GetWGOpenIdRequestBody (string region)
		{
			try
			{
				return Ok(_wgOpenId.GetRequestBody(RegionExtensions.FromString(region), $"{_settings.BaseUrl}Account/WGOpenId/Login/{region}"));
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

		[HttpGet("[controller]/WGOpenId/Login/{region}")]
		public IActionResult WGOpenIdLogin (string region,
			[FromQuery(Name = "openid.assoc_handle")] string assocHandle,
			[FromQuery(Name = "openid.claimed_id")] string claimedId)
		{
			try
			{
				var verification = _wgOpenId.VerifyLogin(RegionExtensions.FromString(region), claimedId, assocHandle, HttpContext.Request.Query);

				// ONEDAY: use C# 8 syntax
				if (verification is null)
				{
					return BadRequest();
				}
				else
				{
					(long id, string nickname) = ((long, string))verification;

					var player = _warshipsApi.GetPlayerInfoAsync(RegionExtensions.FromString(region), id).GetAwaiter().GetResult();
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
		public IActionResult GetAccount (long id)
		{
			Accounts.Manager.ScopeId = id;
			try
			{
				var result = Accounts.Do(a => a.GetAccount());
				return result.Success ? Ok(result.Result) : throw result.Exception;
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (UnauthorizedException)
			{
				return Unauthorized();
			}
			catch
			{
				return StatusCode(500);
			}
		}

		[HttpGet("api/[controller]")]
		public IActionResult GetAccount ()
		{
			Accounts.Manager.ScopeId = _user.User.AccountId;
			try
			{
				var result = Accounts.Do(a => a.GetAccount());
				return result.Success ? Ok(result.Result) : throw result.Exception;
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (UnauthorizedException)
			{
				return Unauthorized();
			}
			catch
			{
				return StatusCode(500);
			}
		}

		[HttpPost("api/[controller]/primary/discord")]
		public IActionResult SetPrimaryDiscordUser ([FromBody] long discordId)
		{
			Accounts.Manager.ScopeId = _user.User.AccountId;			
			try
			{
				var result = Accounts.Do(a => a.SetPrimaryDiscordUser(discordId));
				return result.Success ? Ok() : throw result.Exception;
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (UnauthorizedException)
			{
				return Unauthorized();
			}
			catch
			{
				return StatusCode(500);
			}
		}

		[HttpPost("api/[controller]/primary/warships")]
		public IActionResult SetPrimaryWarshipsPlayer ([FromBody] long playerId)
		{
			Accounts.Manager.ScopeId = _user.User.AccountId;
			try
			{
				var result = Accounts.Do(a => a.SetPrimaryWarshipsPlayer(playerId));
				return result.Success ? Ok() : throw result.Exception;
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (UnauthorizedException)
			{
				return Unauthorized();
			}
			catch
			{
				return StatusCode(500);
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