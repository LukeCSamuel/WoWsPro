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
	public class TeamController : Controller
	{
		IAuthorizer<TeamOperations> Tops { get; }

		public TeamController (IAuthorizer<TeamOperations> tOps) => Tops = tOps;

		[HttpGet("{id:long}")]
		public IActionResult GetTeam (long id)
		{
			try
			{
				var result = Tops.Do(t => t.GetTeamById(id));
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

		[HttpGet("joined/{tournamentId:long}/{playerId:long}")]
		public IActionResult GetTeam (long playerId, long tournamentId)
		{
			try
			{
				var result = Tops.Do(t => t.GetJoinedTeam(playerId, tournamentId));
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

		[HttpPost("create")]
		public IActionResult CreateTeam ([FromBody] TournamentTeam team)
		{
			try
			{
				var result = Tops.Do(t => t.CreateNewTeam(team));
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
			catch (NotSupportedException)
			{
				return BadRequest();
			}
			catch
			{
				return StatusCode(500);
			}
		}

		[HttpPost("update")]
		public IActionResult UpdateTeam ([FromBody] TournamentTeam team)
		{
			try
			{
				Tops.Manager.ScopeId = team.TeamId;
				var detailResult = Tops.Do(t => t.UpdateTeam(team));
				var memberResult = Tops.Do(t => t.UpdateTeamMembers(team));
				return detailResult.Success && memberResult.Success 
					? Ok(detailResult.Result && memberResult.Result) 
					: throw new AggregateException(detailResult.Exception, memberResult.Exception); 
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (UnauthorizedException)
			{
				return Unauthorized();
			}
			catch (NotSupportedException ex)
			{
				Console.WriteLine(ex);
				return BadRequest();
			}
			catch
			{
				return StatusCode(500);
			}
		}
	}
}
