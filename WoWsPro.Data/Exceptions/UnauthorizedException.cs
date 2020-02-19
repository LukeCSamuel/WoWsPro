using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Exceptions
{
	/// <summary>
	/// Details summary of failed authorization
	/// </summary>
	public class UnauthorizedException : Exception
	{
		public string Action { get; }
		public long UserId { get; }
		public long ScopeId { get; }
		public Type ScopeType { get; }

		public UnauthorizedException (string action) => Action = action;

		public UnauthorizedException (UnauthorizedException ex, long? userId, long scopeId, Type scopeType)
			: base($"User: {userId}; Action: {ex.Action}; Type: {scopeType.Name}; Scope: {scopeId}")
		{
			UserId = userId ?? throw new UnauthenticatedException();
			ScopeId = scopeId;
			ScopeType = scopeType;
			Action = ex.Action;
		}
	}
}
