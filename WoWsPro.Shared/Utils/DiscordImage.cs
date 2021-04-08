using System;
using System.Collections.Generic;
using System.Text;
using WoWsPro.Shared.Models.Discord;

namespace WoWsPro.Shared.Utils
{
	public static class DiscordImage
	{
		const string _cdnUrl = @"https://cdn.discordapp.com";

		public static string GetUserAvatar (DiscordUser user, Size size = Size.Medium)
		{
			if (!string.IsNullOrEmpty(user.Avatar))
			{
				string baseUrl = $"{_cdnUrl}/avatars/{user.DiscordId}";
				if (user.Avatar.StartsWith("a_"))
				{
					return $"{baseUrl}/{user.Avatar}.gif?size={(int)size}";
				}
				else
				{
					return $"{baseUrl}/{user.Avatar}.png?size={(int)size}";
				}
			}
			else
			{
				return $"{_cdnUrl}/embed/avatars/{int.Parse(user.Discriminator) % 5}.png?size={(int)size}";
			}
		}

		public enum Size
		{
			x16 = 1 << 4,
			x32 = 1 << 5,
			x64 = 1 << 6,
			x128 = 1 << 7,
			x256 = 1 << 8,
			x512 = 1 << 9,
			x1024 = 1 << 10,
			x2048 = 1 << 11,

			Small = x32,
			Medium = x128,
			Large = x512,
			ExtraLarge = x2048
		}
	}
}
