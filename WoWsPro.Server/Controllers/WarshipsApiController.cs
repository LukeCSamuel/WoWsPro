using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models.Warships;

namespace WoWsPro.Server.Controllers
{
	[Route("api/[controller]")]
	public class WarshipsApiController : Controller
	{
		IWarshipsApi Api { get; set; }

		public WarshipsApiController (IWarshipsApi api) => Api = api;

		[HttpGet("{region}/player/{name}")]
		public async Task<IActionResult> SearchPlayers (string region, string name)
		{
			try
			{
				var players = await Api.SearchPlayerAsync(RegionExtensions.FromString(region), name);
				if (players.SingleOrDefault(p => p.Nickname == name) is (long id, string nickname))
				{
					return Ok(new List<WarshipsPlayer>() { new WarshipsPlayer() { PlayerId = id, Nickname = nickname } });
				}
				else
				{
					return Ok(players.Select(p => new WarshipsPlayer() { PlayerId = p.Id, Nickname = p.Nickname }));
				}
			}
			catch
			{
				return StatusCode(500);
			}
		}
	}
}
