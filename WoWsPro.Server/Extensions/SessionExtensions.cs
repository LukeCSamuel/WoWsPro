using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoWsPro.Server.Extensions
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SessionVarAttribute : Attribute { }

	public static class SessionExtensions
	{
		public static T Retrieve<T> (this ISession session) where T : new()
		{
			var result = new T();
			var type = typeof(T);
			foreach (var property in type.GetProperties()
				.Where(p => p.CustomAttributes
				.Any(a => a.AttributeType == typeof(SessionVarAttribute))
				))
			{
				string val = session.GetString($"{type.FullName}.{property.Name}");
				if (val is string value)
				{
					property.SetValue(result, JsonConvert.DeserializeObject(value, property.PropertyType));
				}
			}
			return result;
		}

		public static void Store<T> (this ISession session, T value) where T : new()
		{
			var type = typeof(T);
			foreach (var property in type.GetProperties()
				.Where(p => p.CustomAttributes
				.Any(a => a.AttributeType == typeof(SessionVarAttribute))
				))
			{
				string val = JsonConvert.SerializeObject(property.GetValue(value));
				session.SetString($"{type.FullName}.{property.Name}", val);
			}
		}
	}
}
