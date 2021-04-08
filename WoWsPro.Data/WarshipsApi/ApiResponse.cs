using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi
{
	internal class ApiResponse<T>
	{
		public string status { get; set; }
		public Meta meta { get; set; }
		public Error error { get; set; }
		public T data { get; set; }

		public bool Ok => status == "ok";

	}
}
