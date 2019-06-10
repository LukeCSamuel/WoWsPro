using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization
{
	public class UnauthorizedException : Exception
	{
		public object Entity { get; set; }
		public long? UserId { get; set; }
		public Actions Action { get; set; }

		public override string Message => $"User {UserId} is unauthorized to {Action} {Entity.GetType().BaseType?.Name ?? Entity} in the manner requested.";
	}
}
