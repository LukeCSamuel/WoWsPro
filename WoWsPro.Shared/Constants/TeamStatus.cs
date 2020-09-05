using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Constants
{
	public enum TeamStatus
	{
		Unregistered,
		Registered,
		WaitListed,
		Accepted,
		Withdrawn,
		Disqualified
	}

	public static class TeamStatusExtensions
	{
		public static string ToVerb (this TeamStatus status)
		{
			switch (status)
			{
			case TeamStatus.Unregistered:
				return "Unregistered";
			case TeamStatus.Registered:
				return "Registered";
			case TeamStatus.WaitListed:
				return "Wait Listed";
			case TeamStatus.Accepted:
				return "Accepted";
			case TeamStatus.Withdrawn:
				return "Withdrawn";
			case TeamStatus.Disqualified:
				return "Disqualified";
			default:
				throw new NotImplementedException("TeamStatus of " + status + " does not have a ToVerb extension implementation.");
			}
		}
	}
}
