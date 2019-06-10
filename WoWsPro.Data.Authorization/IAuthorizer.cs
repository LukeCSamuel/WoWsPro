using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization
{
	public interface IAuthorizer
	{
		bool? IsAuthorized (object value);
	}
}
