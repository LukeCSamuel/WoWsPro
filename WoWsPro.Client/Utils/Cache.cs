using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace WoWsPro.Client.Utils
{
	public class Cache<T>
	{
		Func<Task<T>> Update { get; }
		T CachedValue { get; set; }
		public event EventHandler<T> Updated;

		public Task Load => CurrentUpdateTask ?? UpdateAsync();
		Task<T> CurrentUpdateTask { get; set; }

		public Cache (Func<Task<T>> update) => Update = update;

		public System.Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter () => GetValueAsync().GetAwaiter();

		public async Task<T> GetValueAsync ()
		{
			if (CurrentUpdateTask is null)
			{
				await UpdateAsync();
				return CachedValue;
			}
			else
			{
				return CurrentUpdateTask.IsCompleted ? CachedValue : await CurrentUpdateTask;
			}
		}

		public async Task UpdateAsync ()
		{
			CurrentUpdateTask = Update();
			CachedValue = await CurrentUpdateTask;
			Updated?.Invoke(this, CachedValue);
		}
	}

}
