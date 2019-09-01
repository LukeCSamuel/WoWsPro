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
	public interface IDiscordManager
	{

	}

	internal class DiscordManager : IDiscordManager
	{
		IContextAuthorization Authorization { get; }
		Context Context => Authorization.Context;

		public DiscordManager(IContextAuthorization authorization)
		{
			Authorization = authorization;
		}


		internal async Task<DB.Models.DiscordGuild> GetGuildAsync (long id) => (await Context.DiscordGuilds.SingleOrDefaultAsync(e => e.GuildId == id)) ?? throw new KeyNotFoundException();
		internal async Task<DB.Models.DiscordGuild> AddOrUpdateGuildAsync (DB.Models.DiscordGuild guild)
		{
			var result = await Context.DiscordGuilds.AddOrUpdateAsync(guild, (a, b) => a.GuildId == b.GuildId);
			await Context.SaveChangesAsync();
			return result.Entity;
		}
	}
}
