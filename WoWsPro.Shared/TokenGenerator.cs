using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WoWsPro.Shared
{
	public static class TokenGenerator
	{
		static readonly char[] _characters = {
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
			'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
			'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
			'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'
		};

		public static string GenerateToken (int length = 11)
		{
			char[] chars = new char[length];
			byte[] rand = new byte[length];

			using var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(rand);

			for (int i = 0; i < chars.Length; i++)
			{
				chars[i] = _characters[rand[i] % _characters.Length];
			}

			return new string(chars);
		}
	}
}
