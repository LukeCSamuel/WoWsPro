using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi
{
	internal class Error
	{
		public string field { get; set; }
		public string message { get; set; }
		public int code { get; set; }
		public string value { get; set; }
	}
}
