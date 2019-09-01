using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Constants
{
	public static class Permissions
	{
		public static IAdminPermission AdministerApplicationSettings => (AdminPermission)nameof(AdministerApplicationSettings);
		public static IAdminPermission AdministerAccounts => (AdminPermission)nameof(AdministerAccounts);
		public static IAdminPermission AdministerClaims => (AdminPermission)nameof(AdministerClaims);
		public static IAdminPermission AdministerTournaments => (AdminPermission)nameof(AdministerTournaments);
		public static IAdminPermission AdministerParticipants => (AdminPermission)nameof(AdministerParticipants);
		public static IAdminPermission AdministerWGData => (AdminPermission)nameof(AdministerWGData);


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

	public interface IAdminPermission : IPermission
	{ }

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

	public class AdminPermission : IAdminPermission
	{
		string IPermission.Permission => _permission;
		readonly string _permission;

		public AdminPermission (string permission)
		{
			_permission = permission;
		}

		public AdminPermission (IPermission permission)
		{
			_permission = permission.Permission;
		}

		public static implicit operator string (AdminPermission obj) => obj._permission;
		public static explicit operator AdminPermission (string obj) => new AdminPermission(obj);
	}
}
