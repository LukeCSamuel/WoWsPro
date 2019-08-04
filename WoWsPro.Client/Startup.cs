using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using WoWsPro.Client.Services;

namespace WoWsPro.Client
{
	public class Startup
	{
		public void ConfigureServices (IServiceCollection services)
		{
			services.AddUserService();
		}

		public void Configure (IComponentsApplicationBuilder app)
		{
			app.AddComponent<App>("app");
		}
	}
}
