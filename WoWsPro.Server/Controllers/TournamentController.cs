using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Data.Operations;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Exceptions;
using WoWsPro.Shared.Models;

namespace WoWsPro.Server.Controllers
{
    [Route("api/[controller]")]
    public class TournamentController : WowsProApiController
    {
        TournamentOperations Tops { get; }

        public TournamentController(TournamentOperations tOps) => Tops = tOps;

        [HttpGet("{id:long}")]
        public IActionResult GetTournament(long id)
        {
            try
            {
                var result = Tops.GetTournament(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        [HttpGet]
        public IActionResult ListTournaments()
        {
            try
            {
                var result = Tops.ListTournaments();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        [HttpGet("{tournamentId:long}/teams")]
        public IActionResult ListTeams(long tournamentId)
        {
            try
            {
                var result = Tops.ListTeamsAsync(tournamentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }

        [HttpGet("{tournamentId:long}/{region}/waitlist")]
        public IActionResult GetWaitList(long tournamentId, Region region)
        {
            try
            {
                var result = Tops.GetWaitList(tournamentId, region);
                return Ok(result);
            }
            catch (Exception ex)
			{
                return Error(ex);
            }

        }

	}
}
