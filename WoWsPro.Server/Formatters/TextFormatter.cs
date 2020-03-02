using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace WoWsPro.Server.Formatters
{
	public class TextFormatter : TextInputFormatter
	{
		public TextFormatter ()
		{
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
			SupportedEncodings.Add(Encoding.UTF8);
			SupportedEncodings.Add(Encoding.Unicode);
		}

		protected override bool CanReadType (Type type)
		{
			if (typeof(string).IsAssignableFrom(type))
			{
				return base.CanReadType(type);
			}
			return false;
		}

		public override async Task<InputFormatterResult> ReadRequestBodyAsync (InputFormatterContext context, Encoding encoding)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (encoding is null)
			{
				throw new ArgumentNullException(nameof(encoding));
			}

			var request = context.HttpContext.Request;
			using var reader = new StreamReader(request.Body, encoding);

			try
			{
				return await InputFormatterResult.SuccessAsync(await reader.ReadToEndAsync());
			}
			catch
			{
				return await InputFormatterResult.FailureAsync();
			}
		}

	}
}
