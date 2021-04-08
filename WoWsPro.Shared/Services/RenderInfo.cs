using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsPro.Shared.Services
{
	public enum RenderSource
	{
		ServerPreRender,
		WebAssembly
	}

	public interface IRenderInfo
	{
		RenderSource Source { get; }
	}

	public class ServerPreRender : IRenderInfo
	{
		public RenderSource Source => RenderSource.ServerPreRender;
	}

	public class WebAssemblyRender : IRenderInfo
	{
		public RenderSource Source => RenderSource.WebAssembly;
	}

	public static class RenderSourceProvider
	{
		public static IServiceCollection AddServerPreRenderSource (this IServiceCollection services)
			=> services.AddSingleton<IRenderInfo, ServerPreRender>();

		public static IServiceCollection AddWebAssemblyRenderSource (this IServiceCollection services)
			=> services.AddSingleton<IRenderInfo, WebAssemblyRender>();
	}
}
