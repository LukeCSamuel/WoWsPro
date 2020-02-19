using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;
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
		IAuthorizer<AccountOperations> Accounts { get; }

		public AccountController (
			IUserService userService,
			ISettings settings,
			IWGOpenId wgOpenId,
			IWarshipsApi warshipsApi,
			IAuthorizer<AccountOperations> accounts)
		{
			_user = userService;
			_settings = settings;
			_wgOpenId = wgOpenId;
			_warshipsApi = warshipsApi;
			Accounts = accounts;
		}

		[HttpGet("api/[controller]/User")]
		public IActionResult GetUser () => Ok(_user.User);

		[HttpGet("[controller]/Logout")]
		public IActionResult Logout ()
		{
			_user.Logout();
			return Redirect("/"); // TODO: Replace with intelligent redirect
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
			catch (Exception)
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
					return Redirect("/"); // TODO: Replace with memory redirect
				}
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { Reason = ex.Message });
			}
			catch (Exception)
			{
				return StatusCode(500);
			}
		}

		[HttpGet("api/[controller]/{id:long}")]
		public Account GetAccount (long id)
		{
			Accounts.Manager.ScopeId = id;
			return Accounts.Do(a => a.GetAccount()).Result;
		}

    }
}