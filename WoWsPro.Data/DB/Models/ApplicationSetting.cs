using System;
using WoWsPro.Data.Authorization;
using WoWsPro.Shared.Constants;
using SM = WoWsPro.Shared.Models;

namespace WoWsPro.Data.DB.Models
{
	[Authorize(Actions.All, Permissions.AdministerApplicationSettings)]
	internal partial class ApplicationSetting
	{
		public ApplicationSetting() { }

		public long ApplicationSettingId { get; set; }
		public string Environment { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }
		public DateTime Timestamp { get; set; }



		public static implicit operator SM.ApplicationSetting (ApplicationSetting model) => new SM.ApplicationSetting()
		{
			ApplicationSettingId = model.ApplicationSettingId,
			Environment = model.Environment,
			Key = model.Key,
			Value = model.Value,
			Timestamp = model.Timestamp
		};

		public static implicit operator ApplicationSetting (SM.ApplicationSetting model) => new ApplicationSetting()
		{
			ApplicationSettingId = model.ApplicationSettingId,
			Environment = model.Environment,
			Key = model.Key,
			Value = model.Value,
			Timestamp = model.Timestamp
		};
	}
}