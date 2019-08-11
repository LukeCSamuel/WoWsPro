using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Models
{
	public class ApplicationSetting
	{
		public ApplicationSetting () { }

		public long ApplicationSettingId { get; set; }
		public string Environment { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }
		public DateTime Timestamp { get; set; }

		public override string ToString () => $"[{Key}]{(Environment is string ? "$" + Environment : "")}@{Timestamp} = {Value}";
	}
}
