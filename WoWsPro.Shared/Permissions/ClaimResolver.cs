using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using WoWsPro.Shared.Models;

namespace WoWsPro.Shared.Permissions {
    public interface IClaimResolver<in T> {
        bool HasValidClaim (Account account, T context);
    }

    public abstract class ClaimResolver<T> : IClaimResolver<T>
    {
        public abstract string ClaimTitle { get; }

        /// <summary>
		/// Determines if the given claim passes validation.  The title of the claim will have already been verified.
		/// </summary>
		/// <param name="claim">The claim which should be validated.</param>
        /// <param name="context">The context for which the claim should be validated.</param>
        protected abstract bool IsClaimValid (Claim claim, T context);

        protected (string scope, string value) GetScopedValue (Claim claim)
        {
            var match = Regex.Match(claim.Value, @"([\d\w]*)/(.*)");
            if (match.Success)
            {
                return (match.Groups[1].ToString(), match.Groups[2].ToString());
            }
            else
            {
                throw new InvalidOperationException($"Claim value '{claim.Value}' was not scoped.");
            }
        }

        protected Claim CreateScopedClaim<U> (string scope, U value)
        {
            return new Claim()
            {
                Title = ClaimTitle,
                Value = $"{scope}/{JsonSerializer.Serialize(value)}"
            };
        }

        public bool HasValidClaim (Account account, T context)
        {
            var implicitClaim = new Claim()
            {
                Account = account,
                AccountId = account.AccountId,
                Title = ClaimTitle,
                Value = null
            };
            return IsClaimValid(implicitClaim, context)
                || (account?.Claims?.Any(c => c.Title == ClaimTitle && IsClaimValid(c, context)) ?? false);
        }
    }
}