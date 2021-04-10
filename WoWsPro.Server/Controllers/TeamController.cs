using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Data.Operations;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Exceptions;
using WoWsPro.Shared.Models.Tournaments;

namespace WoWsPro.Server.Controllers
{
	[Route("api/[controller]")]
	public class TeamController : WowsProApiController
	{
		TeamOperations Tops { get; }

		public TeamController (TeamOperations tOps) => Tops = tOps;

		[HttpGet("{id:long}")]
		public async Task<IActionResult> GetTeamAsync (long id)
		{
			try
			{
				var result = await Tops.GetTeamByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
		}

		[HttpGet("joined/{tournamentId:long}/{playerId:long}")]
		public async Task<IActionResult> GetTeamAsync (long playerId, long tournamentId)
		{
			try
			{
				var result = await Tops.GetJoinedTeamAsync(playerId, tournamentId);
				return Ok(result);
			}
            catch (Exception ex)
            {
                return Error(ex);
            }
		}

		[HttpGet("me/{tournamentId:long}")]
		public async Task<IActionResult> GetInvolvedTeams (long tournamentId)
		{
			try 
			{
                var joinedTeams = (await Tops.GetUserInvitesAsync(tournamentId))
                    .Where(p => p.Status == ParticipantStatus.Accepted)
                    .Select(p => p.TeamId);
                var managedTeams = (await Tops.GetManagedTeamsAsync(tournamentId))
					.Select(t => t.TeamId);

				var teamIds = joinedTeams.Union(managedTeams);

                var teams = new List<TournamentTeam>();
				foreach (var id in teamIds)
				{
                    teams.Add(await Tops.GetTeamByIdAsync(id));
                }

                return Ok(teams);
            }
			catch (Exception ex)
			{
                return Error(ex);
            }
		}

		[HttpPost("create")]
		public async Task<IActionResult> CreateTeamAsync ([FromBody] TournamentTeam team)
		{
			try
			{
				await Tops.CreateTeamAsync(team);
				return Ok();
			}
            catch (Exception ex)
            {
                return Error(ex);
            }
		}

		[HttpPost("update/info")]
		public async Task<IActionResult> UpdateTeamInfoAsync ([FromBody] TournamentTeam team)
		{
			try
			{
				await Tops.EditTeamInfoAsync(team);
				return Ok();
			}
            catch (Exception ex)
            {
                return Error(ex);
            }
		}

		[HttpPost("update/roster")]
		public async Task<IActionResult> UpdateTeamRosterAsync ([FromBody] TournamentTeam team)
		{
			try
			{
				await Tops.EditTeamRosterAsync(team);
				return Ok();
			}
			catch (Exception ex)
			{
				return Error(ex);
			}
		}
	}
}
