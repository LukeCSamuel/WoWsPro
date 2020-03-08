using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Data.Exceptions;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;
using WoWsPro.Shared.Models;

namespace WoWsPro.Server.Controllers
{
	[Route("api/[controller]")]
	public class TournamentController : Controller
	{
		IAuthorizer<TournamentOperations> Tops { get; }

		public TournamentController (IAuthorizer<TournamentOperations> tOps) => Tops = tOps;

		[HttpGet("{id:long}")]
		public IActionResult GetTournament (long id)
		{
			Tops.Manager.ScopeId = id;
			try
			{
				var result = Tops.Do(t => t.GetTournament(id));
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

		[HttpGet]
		public IActionResult ListTournaments ()
		{
			try
			{
				var result = Tops.Do(t => t.PreviewListTournaments());
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

		[HttpGet("{tournamentId:long}/teams")]
		public IActionResult ListTeams (long tournamentId)
		{
			Tops.Manager.ScopeId = tournamentId;
			try
			{
				var result = Tops.Do(t => t.ListTeams());
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

	}
}
