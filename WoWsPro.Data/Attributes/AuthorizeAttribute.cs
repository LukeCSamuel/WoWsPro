using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Attributes
{
	/// <summary>
	/// Represents the permissions required to authorize the execution of a method
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	internal class AuthorizeAttribute : Attribute
	{
		public string Permission { get; }

		public AuthorizeAttribute (string permission) => Permission = permission;
	}

}
