using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WoWsPro.Data.DB;
using WoWsPro.Data.Services;
using WoWsPro.Shared.Models;

namespace WoWsPro.Data.Managers
{
	public interface IApplicationSettingManager
	{
		Task<ApplicationSetting> GetAsync (long id);
		Task<ApplicationSetting> AddAsync (ApplicationSetting setting);
		Task<ApplicationSetting> SetValueAsync (long id, string value);
		Task DeleteAsync (long id);
	}

	internal class ApplicationSettingManager : IApplicationSettingManager
	{
		IContextAuthorization Authorization { get; }
		Context Context => Authorization.Context;

		public ApplicationSettingManager (IContextAuthorization auth)
		{
			Authorization = auth;
		}

		public async Task<ApplicationSetting> GetAsync (long id)
		{
			return await Context.ApplicationSettings.SingleOrDefaultAsync(e => e.ApplicationSettingId == id);
		}

		public async Task<ApplicationSetting> AddAsync (ApplicationSetting setting)
		{
			var result = Context.ApplicationSettings.Add(setting);
			await Context.SaveChangesAsync();
			return result.Entity;
		}

		public async Task<ApplicationSetting> SetValueAsync (long id, string value)
		{
			var e = await Context.ApplicationSettings.SingleOrDefaultAsync(e => e.ApplicationSettingId == id);
			e.Value = value;
			await Context.SaveChangesAsync();
			return e;
		}

		public async Task DeleteAsync (long id)
		{
			var e = await Context.ApplicationSettings.SingleOrDefaultAsync(e => e.ApplicationSettingId == id);
			Context.ApplicationSettings.Remove(e);
			await Context.SaveChangesAsync();
		}
	}
}
