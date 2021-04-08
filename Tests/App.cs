using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WoWsPro.Data.Operations;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Shared.Constants;
using WoWsPro.Shared.Models;
using WoWsPro.Shared.Models.Tournaments;

namespace Tests
{
	public class App
	{
		public async Task RunAsync ()
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri("https://localhost:4200");

			var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
				ReferenceHandler = ReferenceHandler.Preserve
			};

			// var sample = new Sample () {
			// 	Hello = "Hello",
			// 	World = "World"
			// };
			// var list = new List<Sample>() { sample };

			// var serialized = JsonSerializer.Serialize(list, options);
			// Console.WriteLine(serialized);
			// var deserialized = JsonSerializer.Deserialize<List<Sample>>(serialized, options);
			// Console.WriteLine(deserialized[0].Hello);


			
			var result = await client.GetStringAsync("api/tournament");
			Console.WriteLine(result);
			var tournaments = await client.GetFromJsonAsync<List<Tournament>>("api/tournament", options);
			Console.WriteLine(JsonSerializer.Serialize(tournaments[0]));
		}

		class Sample {
			public string Hello { get; set; }
			public string World { get; set; }
		}
	}
}
