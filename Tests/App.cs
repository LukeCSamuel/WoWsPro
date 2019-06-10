using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.Authorization;
using WoWsPro.Data.Managers;
using WoWsPro.Shared.Models;

namespace Tests
{
	public class App
	{
		ClaimManager Manager { get; }

		public App (ClaimManager manager)
		{
			Manager = manager;
		}

		public async Task RunAsync ()
		{
			try
			{
				await Manager.AddAllClaimsAsync();
				Console.WriteLine("Operation succeeded.");
			}
			catch (UnauthorizedException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
