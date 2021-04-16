using Ganss.XSS;
using Markdig;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.EmphasisExtras;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoWsPro.Client.Services
{
	public interface IMarkdownConverter
	{
		MarkupString ToHtml (string markdown);
	}

	public class MarkdownConverter : IMarkdownConverter
	{
		MarkdownPipeline MarkdownPipeline { get; }
		HtmlSanitizer Sanitizer { get; }

		public MarkdownConverter ()
		{
			var pipelineBuilder = new MarkdownPipelineBuilder()
				.UseAutoIdentifiers()
				.UseReferralLinks("noopener", "noreferrer", "nofollow")
				.UseAutoLinks(
					new AutoLinkOptions()
					{
						OpenInNewWindow = true,
						UseHttpsForWWWLinks = true
					}
				)
				//.UseMediaLinks()
				.UseListExtras()
				.UseEmphasisExtras(
					EmphasisExtraOptions.Strikethrough
					& EmphasisExtraOptions.Subscript
					& EmphasisExtraOptions.Superscript
				)
				.UseFootnotes()
				;

			MarkdownPipeline = pipelineBuilder.Build();
			// TODO: more work is needed for media links to actually be enabled, because the sanitizer strips iframes by default
			//       we want to allow iframes, but probably only from a whitelist of sites
			Sanitizer = new HtmlSanitizer();
		}

		public MarkupString ToHtml (string markdown)
		{
			var unsanitized = Markdown.ToHtml(markdown, MarkdownPipeline);
			var sanitized = Sanitizer.Sanitize(unsanitized);
			return (MarkupString)sanitized;
		}
	}

	public static class MarkdownConverterProvider
	{
		public static IServiceCollection AddMarkdownConverter (this IServiceCollection services)
			=> services.AddSingleton<IMarkdownConverter, MarkdownConverter>();
	}
}
