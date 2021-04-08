using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi
{
	internal class Meta
	{
		public int? count { get; set; }
		public int? page_total { get; set; }
		public int? total { get; set; }
		public int? limit { get; set; }
		public int? page { get; set; }
	}
}
