using System;

namespace WoWsPro.Data.Authentication
{
	public interface IAuthentication
	{
		public long? AccountId { get; }
		public bool IsAuthenticated { get; }
	}
}
