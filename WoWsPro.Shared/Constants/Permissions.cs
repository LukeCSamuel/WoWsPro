using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Constants
{
	public static class Permissions
	{
		public static IPermission ManageTournament => (Permission)nameof(ManageTournament);
		public static IPermission ManageTournamentClaims => (Permission)nameof(ManageTournamentClaims);
		public static IPermission ManageTeam => (Permission)nameof(ManageTeam);
		public static IPermission ManageTeamClaims => (Permission)nameof(ManageTeamClaims);
		public static IPermission ManageParticipants => (Permission)nameof(ManageParticipants);
		public static IPermission ManageAccount => (Permission)nameof(ManageAccount);


		public static IEnumerable<IPermission> InitialPermissions => new IPermission[] { ManageAccount };

	}

	public interface IPermission
	{
		string Permission { get; }
	}

	public class Permission : IPermission
	{
		string IPermission.Permission => _permission;
		readonly string _permission;

		public Permission (string permission)
		{
			_permission = permission;
		}

		public static implicit operator string (Permission obj) => obj._permission;
		public static implicit operator Permission (string obj) => new Permission(obj);
	}

}
