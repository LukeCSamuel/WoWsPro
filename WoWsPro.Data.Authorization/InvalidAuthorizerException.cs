using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization
{
	public class InvalidAuthorizerException : Exception
	{
		public InvalidAuthorizerException (Type type) : base($"The type {type.Name} does not implement the interface {typeof(IAuthorizer).Name}.") { }
	}
}
