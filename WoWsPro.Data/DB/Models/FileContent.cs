using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.DB.Models
{
	internal partial class FileContent
	{
		public FileContent () { }

		public long FileContentId { get; set; }
		public string Title { get; set; }
		public byte[] Content { get; set; }
	}
}
