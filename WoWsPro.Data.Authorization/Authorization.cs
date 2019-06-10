using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using WoWsPro.Data.Authorization.Claim;
using WoWsPro.Data.Authorization.Model;

namespace WoWsPro.Data.Authorization
{

	// TODO: Invalid reads should be filtered, instead of unauthorized.  Property-specific read filtering should be implemented.

	public class Authorization : IAuthorization
	{
		public IClaimResolver ClaimResolver { get; set; }

		internal AuthorizationCollection Authorizations { get; } = new AuthorizationCollection();

		public void Authorize (EntityEntry entry) => Authorizations[entry.Entity.GetType()].Authorize(entry, ClaimResolver.GetClaims());
	}

	public static class AuthorizationInjection
	{
		public static IServiceCollection AddAuthorization (this IServiceCollection services) => services.AddTransient<IAuthorization, Authorization>();
	}
}
