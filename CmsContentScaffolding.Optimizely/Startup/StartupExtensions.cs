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
		services.AddScoped<IContentBuilderManager, ContentBuilderManager>();
		services.AddScoped(x => new ContentBuilderOptions());
		services.AddScoped<IContentBuilder, ContentBuilder>();

		return services;
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
