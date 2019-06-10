using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.Authorization.Claim;

namespace WoWsPro.Data.Authorization
{
	public interface IAuthorization
	{
		IClaimResolver ClaimResolver { get; set; }

		void Authorize (EntityEntry entry);
		void Authorize (IEnumerable<EntityEntry> entries)
		{
			foreach (var entry in entries)
			{
				Authorize(entry);
			}
		}
		bool TryAuthorize (EntityEntry entry)
		{
			try
			{
				Authorize(entry);
				return true;
			}
			catch (UnauthorizedException)
			{
				return false;
			}
		}
		bool TryAuthorize (IEnumerable<EntityEntry> entries)
		{
			try
			{
				foreach (var entry in entries)
				{
					Authorize(entry);
				}
				return true;
			}
			catch (UnauthorizedException)
			{
				return false;
			}
		}
	}
}
