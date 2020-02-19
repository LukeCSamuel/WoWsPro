using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Exceptions
{
	/// <summary>
	/// Indicates that the user is unauthenticated
	/// </summary>
	public class UnauthenticatedException : Exception
	{
		public UnauthenticatedException () : base("An unauthenticated user attempted to access a non-public resource.") { }
	}
}
