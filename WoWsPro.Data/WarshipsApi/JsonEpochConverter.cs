using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WoWsPro.Data.WarshipsApi
{
	internal class JsonEpochConverter : JsonConverter<DateTime?>
	{
		public override DateTime? Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var success = reader.TryGetInt64(out long seconds);
			if (!success)
			{
				return null;
			}

			var result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);
			return result;
		}

		public override void Write (Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
		{
			if (value is DateTime val)
			{
				long seconds = (long)(val - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
				writer.WriteNumberValue(seconds);
			}
			else
			{
				writer.WriteNullValue();
			}

		}
	}
}
