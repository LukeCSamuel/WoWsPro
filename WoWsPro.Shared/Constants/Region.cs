using System;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Shared.Constants
{
	public enum Region
	{
		NA,
		EU,
		CIS,
		APAC
	}

	public static class RegionExtensions
	{
		public static string TopLevelDomain (this Region region)
		{
			switch (region)
			{
			case Region.NA:
				return "com";
			case Region.EU:
				return "eu";
			case Region.CIS:
				return "ru";
			case Region.APAC:
				return "asia";
			default:
				throw new NotImplementedException();
			}
		}

		public static string Subdomain (this Region region)
		{
			switch (region)
			{
			case Region.NA:
				return "na";
			case Region.EU:
				return "eu";
			case Region.CIS:
				return "ru";
			case Region.APAC:
				return "asia";
			default:
				throw new NotImplementedException();
			}
		}

		public static Region FromString (string region)
		{
			switch (region.ToLower().Trim())
			{
			case "na":
				return Region.NA;
			case "eu":
				return Region.EU;
			case "cis":
			case "ru":
				return Region.CIS;
			case "sea":
			case "asia":
			case "apac":
				return Region.APAC;
			default:
				throw new ArgumentException($"\"{region}\" could not be matched to a known region.");
			}
		}
	}
}
