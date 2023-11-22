using CmsContentScaffolding.Optimizely.Builders;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Managers;
using CmsContentScaffolding.Optimizely.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CmsContentScaffolding.Optimizely.Startup;

public static class StartupExtensions
{
	public static IServiceCollection AddCmsContentScaffolding(this IServiceCollection services)
	{
		return services
			.AddScoped<IContentBuilderManager, ContentBuilderManager>()
			.AddScoped(x => new ContentBuilderOptions())
			.AddTransient<IContentBuilder, ContentBuilder>();
	}

	public static IApplicationBuilder UseCmsContentScaffolding(
		this IApplicationBuilder app,
		Action<IContentBuilder> builder,
		Action<ContentBuilderOptions>? builderOptions = null)
	{
		var options = app.ApplicationServices.GetRequiredService<ContentBuilderOptions>();
		builderOptions?.Invoke(options);

		using var contentBuilder = app.ApplicationServices.GetRequiredService<IContentBuilder>();
		builder.Invoke(contentBuilder);

		return app;
	}
}
