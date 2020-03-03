using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		public Tournament GetTournament (long id)
		{
			Tops.Manager.ScopeId = id;
			return Tops.Do(t => t.GetTournament(id)).Result;
		}

		[HttpGet]
		public IEnumerable<Tournament> ListTournaments () => Tops.Do(t => t.ListTournaments()).Result;
	}
}
