using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared
{
	public class WGOpenIdRequest
	{
		public string RequestAddress { get; set; }
		public Dictionary<string, string> Body { get; set; }
	}
}
