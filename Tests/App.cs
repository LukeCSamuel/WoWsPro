using System;
using System.Collections.Generic;
using System.IO;
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

		FileOperations Filoio { get; }

		public App (FileOperations fileio) => Filoio = fileio;

		public async Task RunAsync ()
		{
			using var file = new FileStream(@"C:\OneDrive\Tournaments\King of the Sea\KotS 10\Art\KoTS-X_banner-512.png", FileMode.Open);
			using var mem = new MemoryStream();

			file.CopyTo(mem);
			(long id, string name) = Filoio.SaveFile("KoTS-X_banner-512.png", mem.ToArray());
			Console.WriteLine($"api/SampleData/{id}/{name}");
		}
	}
}
