using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Attributes
{
	/// <summary>
	/// Represents that a method is authorized for unauthenticated users
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	internal class PublicAttribute : Attribute
	{
		public PublicAttribute () { }
	}
}
