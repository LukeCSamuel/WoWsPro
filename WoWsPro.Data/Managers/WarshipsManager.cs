using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.Services;
using WoWsPro.Shared.Models;

namespace WoWsPro.Data.Managers
{
	public interface IWarshipsManager
	{

	}

	internal class WarshipsManager : IWarshipsManager
	{
		IContextAuthorization Authorization { get; }
		Context Context => Authorization.Context;

		public WarshipsManager (IContextAuthorization authorization)
		{
			Authorization = authorization;
		}

		internal async Task<DB.Models.WarshipsPlayer> GetPlayerAsync (long id) => (await Context.WarshipsPlayers.SingleOrDefaultAsync(e => e.PlayerId == id)) ?? throw new KeyNotFoundException();
		internal async Task<DB.Models.WarshipsPlayer> AddOrUpdateAsync (DB.Models.WarshipsPlayer player)
		{
			var result = await Context.WarshipsPlayers.AddOrUpdateAsync(player, (a, b) => a.PlayerId == b.PlayerId);
			await Context.SaveChangesAsync();
			return result.Entity;
		}

	}
}
