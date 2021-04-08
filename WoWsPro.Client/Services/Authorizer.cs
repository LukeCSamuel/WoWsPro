using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Shared.Exceptions;
using WoWsPro.Shared.Permissions;

namespace WoWsPro.Client.Services
{
    public class Authorizer : IAuthorizer
    {

        IAccountService UserProvider { get; }

        public Authorizer (IAccountService userProvider)
        {
            UserProvider = userProvider;
        }

        /// <summary>
        /// Determines if the current User has the claim specified by the given resolver with respect to the given context.
        /// </summary>
        /// <typeparam name="T">The context type for the claim.</typeparam>
        /// <param name="resolver">The resolver to use to verify the claim.</param>
        /// <param name="context">The context for the claim resolver to use.</param>
        /// <returns>`true` if the Claim is resolved with respect to the context.</returns>
        /// <exception cref="UnauthenticatedException">Thrown if the user provider cannot provide an authenticated user.</exception>
        public bool HasClaim<T> (IClaimResolver<T> resolver, T context)
        {
            return UserProvider.IsLoggedIn ? resolver.HasValidClaim(UserProvider.Account, context) : throw new UnauthenticatedException();
        }
    }

    public static class AuthorizerProvider
    {
        public static IServiceCollection AddAuthorizer (this IServiceCollection services)
            => services.AddScoped<IAuthorizer, Authorizer>();
    }
}
