using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;

namespace Tests
{
	public class App
	{
		IAuthorizer<AccountOperations> Auth { get; }

		public App (IAuthorizer<AccountOperations> auth) => Auth = auth;

		public async Task RunAsync ()
		{
			Auth.Manager.ScopeId = 3;
			Console.WriteLine(Auth.Do(o => o.GetNickname()).Result);
			Console.WriteLine(Auth.Do(o => o.SetNickname("OG Test Account")).Success);

			Auth.Manager.ScopeId = 4;
			Console.WriteLine(Auth.Do(o => o.GetNickname()).Result);
			Console.WriteLine(Auth.Do(o => o.SetNickname("uh oh spaghettio")).Success);
		}
	}
}
