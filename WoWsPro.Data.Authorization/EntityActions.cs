using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization
{
	[Flags]
	public enum Actions
	{
		None = 0,
		Read = 1,
		Modify = 2,
		Create = 4,
		Delete = 8,

		ReadModify = Read | Modify,
		ReadCreate = Read | Create,
		ReadDelete = Read | Delete,
		Immutable = Read | Create | Delete,
		Permanent = Read | Modify | Create,
		All = Read | Modify | Create | Delete
	}
}
