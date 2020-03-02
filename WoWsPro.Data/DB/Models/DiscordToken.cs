using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.DB.Models
{
	internal partial class DiscordToken
	{
		public DiscordToken () { }

		public long DiscordTokenId { get; set; }
		public long DiscordId { get; set; }
		public string AccessToken { get; set; }
		public string TokenType { get; set; }
		public DateTime Expires { get; set; }
		public string RefreshToken { get; set; }
		public string Scope { get; set; }

		public virtual DiscordUser DiscordUser { get; set; }

		public static implicit operator Shared.Models.DiscordToken (DiscordToken token) => token.ConvertObject<DiscordToken, Shared.Models.DiscordToken>();
		public static implicit operator DiscordToken (Shared.Models.DiscordToken token) => token.ConvertObject<Shared.Models.DiscordToken, DiscordToken>();
		
	}
}
