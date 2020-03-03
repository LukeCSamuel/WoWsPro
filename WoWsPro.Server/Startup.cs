using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;
using WoWsPro.Data.DB;
using WoWsPro.Data.Operations;
using WoWsPro.Data.Services;
using WoWsPro.Server.Formatters;
using WoWsPro.Server.Services;

namespace WoWsPro.Server
{
	public class Startup
	{

		private readonly IConfiguration _config;

		public Startup (IConfiguration config)
		{
			_config = config;
		}
		
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices (IServiceCollection services)
		{
			services.AddSettings();

			services.AddMvc(options =>
			{
				options.InputFormatters.Insert(0, new TextFormatter());
			});
				//.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				//.AddNewtonsoftJson(options =>
				//{
				//	options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
				//});

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

			// ONEDAY: don't use session for cookies because it doesn't persist beyond browser lifetime
			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromDays(7);
				options.Cookie.Name = ".WoWsPro.Session";
				options.Cookie.IsEssential = true;
			});
			services.AddHttpContextAccessor();
			services.AddWGOpenId();
			services.AddDiscordOauth2();
			services.AddWarshipsApi();

			services.AddUserService();
			services.AddAuthenticator<UserService>();

			services.AddDataContextPool();
			services.AddAccountManager();
			services.AddTournamentManager();

			services.AddAdminAccountOperations();
			services.AddFileOperations();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBlazorDebugging();
			}
			else
			{
				app.UseHsts();
			}

			app.UseSession();

			app.UseClientSideBlazorFiles<Client.Program>();
			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
				endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html");
			});
		}
	}
}
