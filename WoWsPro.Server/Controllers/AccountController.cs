using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WoWsPro.Server.Services;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Server.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
		private readonly IUserService _user;
		private readonly ISettings _settings;
		private readonly IWGOpenId _wgOpenId;

		public AccountController (IUserService userService, ISettings settings, IWGOpenId wgOpenId)
		{
			_user = userService;
			_settings = settings;
			_wgOpenId = wgOpenId;
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
					_user.TestLogin(id, nickname); // TODO: Replace with data-driven login
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

    }
}