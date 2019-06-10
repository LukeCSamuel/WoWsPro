using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WoWsPro.Data.Authorization.Model
{
	internal class AuthorizationCollection : IEnumerable<AuthorizationModel>
	{
		private static Dictionary<Type, AuthorizationModel> Models { get; set; } = new Dictionary<Type, AuthorizationModel>();

		public AuthorizationModel this [Type type]
		{
			get
			{
				try
				{
					return Models[type];
				}
				catch (KeyNotFoundException)
				{
					return Models[type] = new AuthorizationModel(type);
				}
			}
		}

		public IEnumerator<AuthorizationModel> GetEnumerator () => Models.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator () => Models.Values.GetEnumerator();
	}
}
