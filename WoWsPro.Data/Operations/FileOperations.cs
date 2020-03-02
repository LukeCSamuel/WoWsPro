using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsPro.Data.DB;

namespace WoWsPro.Data.Operations
{
	public class FileOperations
	{
		Context Context { get; }

		public FileOperations (Context context) => Context = context;

		// TODO: async-ify & stream-ify this class

		/// <summary>
		/// Retrieves a file from the database
		/// </summary>
		public byte[] GetFile (long id, string title)
		{
			var file = Context.Files.SingleOrDefault(f => f.FileContentId == id);
			if (file is DB.Models.FileContent)
			{
				if (title == file.Title)
				{
					return file.Content;
				}
				else
				{
					// Checking file title with id prevents crawling files
					throw new InvalidOperationException("Incorrect file title was given.");
				}
			}
			else
			{
				throw new KeyNotFoundException("File does not exist.");
			}
		}

		/// <summary>
		/// Adds a new file to the database.
		/// </summary>
		public (long id, string title) SaveFile (string title, byte[] content)
		{
			var file = new DB.Models.FileContent()
			{
				Title = title,
				Content = content
			};
			Context.Files.Add(file);
			Context.SaveChanges();
			return (file.FileContentId, file.Title);
		}
	}

	public static class FileOperationsProvider
	{
		public static IServiceCollection AddFileOperations (this IServiceCollection services)
			=> services.AddScoped<FileOperations>();
	}
}
