using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.DB.Models;
using WoWsPro.Shared.Constants;

namespace WoWsPro.Data.Managers
{
	public class ClaimManager
	{
		IContextManager ContextManager { get; set; }
		Context Context => ContextManager.Context;

		public ClaimManager (IContextManager manager)
		{
			ContextManager = manager;
		}

		public async Task AddAllClaimsAsync ()
		{
			var claims = Context.Claims.ToList();
			foreach (var field in typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.IsLiteral && field.FieldType == typeof(string))
				{
					string value = (string)field.GetValue(null);
					if (!claims.Any(e => e.Permission == value))
					{
						await Context.Claims.AddAsync(new Claim() { Permission = value });
					}
				}
			}
			await Context.SaveChangesAsync();
		}
	}
}
