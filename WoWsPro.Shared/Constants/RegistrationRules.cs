using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Constants
{
	[Flags]
	public enum RegistrationRules
	{
		None							= 0,
		AllowRosterChanges				= 1 << 0,
		RequireTeamOwnerDiscord			= 1 << 1,
		RequireParticipantsDiscord		= 1 << 2,
		RequireInvitationAccept			= 1 << 3,

		Default = AllowRosterChanges | RequireTeamOwnerDiscord
	}
}
