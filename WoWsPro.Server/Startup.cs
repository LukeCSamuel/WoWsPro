using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WoWsPro.Data.DB;
using WoWsPro.Data.Operations;
using WoWsPro.Data.WarshipsApi;
using WoWsPro.Server.Formatters;
using WoWsPro.Server.Services;
using WoWsPro.Shared.Permissions;
using WoWsPro.Shared.Services;

namespace WoWsPro.Server
{
	public class Startup
	{
		static object locker = new object();

		private readonly IConfiguration _config;

		public Startup (IConfiguration config)
		{
			_config = config;
		}
		
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices (IServiceCollection services)
		{
			// Map controllers and pages
			services.AddControllersWithViews(options =>
			{
				options.InputFormatters.Insert(0, new TextFormatter());
			})
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
			});
			services.AddRazorPages();

			// Provide services for  Blazor Components
			services.AddScoped(s => 
			{
				var navigationManager = s.GetRequiredService<NavigationManager>();
				return new HttpClient
				{
					BaseAddress = new System.Uri(navigationManager.BaseUri)
				};
			});
			Client.Program.ConfigureServices(services);
			
			services.AddSettings();

			services.AddResponseCompression(opts =>
			{
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});

			services.AddDistributedSqlServerCache(options =>
			{
				options.ConnectionString = _config["ConnectionStrings:WoWsPro"];
				options.SchemaName = "dbo";
				options.TableName = "SessionCache";
			});

			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromDays(7);
				options.Cookie.Name = ".WoWsPro.Session";
				options.Cookie.IsEssential = true;
			});
			services.AddHttpContextAccessor();
			services.AddWGOpenId();
			services.AddDiscordOauth2();

			services.AddUserService();
			services.AddUserProvider<UserService>();
            services.AddAuthorizer();

            services.AddDataContextPool(poolSize: 1);
            services.AddOperations();

			services.AddServerPreRenderSource();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
				app.Use(LogRequest);
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			

			app.UseSession();

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToPage("/_Host");
			});
		}

		private async Task LogRequest (HttpContext context, Func<Task> next) {
			var start = DateTime.UtcNow;

			await next();

			var duration = DateTime.UtcNow - start;

			lock (locker) {
				var bg = Console.BackgroundColor;
				var fg = Console.ForegroundColor;

				Console.BackgroundColor = context.Response.StatusCode < 400 ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
				Console.Write($" {context.Response.StatusCode} ");
				Console.BackgroundColor = bg;

				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write($" [{start:hh:mm:ss.fff} + {duration.TotalMilliseconds,8:0.00} ms]");
				Console.ForegroundColor = fg;

				Console.WriteLine($" {context.Request.Method} {context.Request.Path}");
			}
		}
	}
}
