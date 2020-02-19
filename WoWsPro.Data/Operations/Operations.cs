using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Data.Services;

namespace WoWsPro.Data.Operations
{
	public abstract class Operations<T>
	{
		protected internal Context Context => Manager.Context;
		protected internal long? ScopeId => Manager.ScopeId;
		protected internal long? UserId => Manager.UserId;
		protected internal Manager<T> Manager { get; set; }
	}
}
