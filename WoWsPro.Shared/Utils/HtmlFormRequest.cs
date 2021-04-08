using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Utils
{
	public class HtmlFormRequest
	{
		public string RequestAddress { get; set; }
		public Dictionary<string, string> Body { get; set; }
	}
}
