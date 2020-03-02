using WoWsPro.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsPro.Data.Operations;

namespace WoWsPro.Server.Controllers
{
	[Route("api/[controller]")]
	public class SampleDataController : Controller
	{
		//[HttpGet("[action]")]
		//public IEnumerable<WeatherForecast> WeatherForecasts ()
		//{
		//	var rng = new Random();
		//	return Enumerable.Range(1, 5).Select(index => new WeatherForecast
		//	{
		//		Date = DateTime.Now.AddDays(index),
		//		TemperatureC = rng.Next(-20, 55),
		//		Summary = Summaries[rng.Next(Summaries.Length)]
		//	});
		//}

		FileOperations FileIO { get; }

		public SampleDataController (FileOperations fileIO) => FileIO = fileIO;

		[HttpGet("{id}/{title}")]
		public IActionResult DownloadFile (long id, string title)
		{
			try
			{
				byte[] content = FileIO.GetFile(id, title);
				string mimeType = "image/png";
				return new FileContentResult(content, mimeType)
				{
					FileDownloadName = title
				};
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (InvalidOperationException)
			{
				return BadRequest();
			}
			catch
			{
				return StatusCode(500);
			}
		}
	}
}
