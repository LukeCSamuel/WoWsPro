using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi
{
	public class WarshipsApiException : Exception
	{
		public string Field { get; set; }
		public string Info { get; set; }
		public int Code { get; set; }
		public string Value { get; set; }

		internal WarshipsApiException (Error error) : base(
			$"The Warships API returned the following error:\r\n" +
			$"\tmessage: {error.message}\r\n" +
			$"\tcode: {error.code}\r\n" +
			$"\tfield: {error.field}\r\n" +
			$"\tvalue: {error.value}"
			)
		{
			Field = error.field;
			Info = error.message;
			Code = error.code;
			Value = error.value;
		}
	}
}
