using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Data.DB;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Shared.Permissions;

namespace WoWsPro.Data.Operations
{
	public abstract class Operations
	{
		protected internal Context Context { get; private set; }
		protected internal IAuthorizer Authorizer { get; private set; }

        public Operations (Context context, IAuthorizer authorizer)
		{
            Context = context;
            Authorizer = authorizer;
        }
	}
}
