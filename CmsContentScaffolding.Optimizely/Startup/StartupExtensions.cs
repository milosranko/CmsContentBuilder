using CmsContentScaffolding.Optimizely.Builders;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Managers;
using CmsContentScaffolding.Optimizely.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CmsContentScaffolding.Optimizely.Startup;

public static class StartupExtensions
{
    public static IServiceCollection AddCmsContentScaffolding(
        this IServiceCollection services)
    {
        services.AddTransient<IContentBuilderManager, ContentBuilderManager>();
        services.AddTransient<IContentBuilder, ContentBuilder>();
        services.AddScoped(x => new ContentBuilderOptions());

        return services;
    }

    public static void UseCmsContentScaffolding(
        this IApplicationBuilder app,
        Action<IContentBuilder> builder,
        Action<ContentBuilderOptions>? builderOptions = null)
    {
        var options = app.ApplicationServices.GetRequiredService<ContentBuilderOptions>();
        var contentBuilderManager = app.ApplicationServices.GetService<IContentBuilderManager>();

        builderOptions?.Invoke(options);

        if (ApplyOptions(contentBuilderManager, options))
        {
            var appBuilder = app.ApplicationServices.GetRequiredService<IContentBuilder>();
            builder.Invoke(appBuilder);

            contentBuilderManager.DeleteTempFolder();
        }
    }

    private static bool ApplyOptions(IContentBuilderManager contentBuilderManager, ContentBuilderOptions options)
    {
        var proceedBuildingContent = false;

        switch (options.BuildMode)
        {
            case BuildMode.Append:
                proceedBuildingContent = true;
                break;
            case BuildMode.Overwrite:
                proceedBuildingContent = true;
                break;
            case BuildMode.OnlyIfEmptyInDefaultLanguage:
                proceedBuildingContent = contentBuilderManager.IsInstallationEmpty();
                break;
            case BuildMode.OnlyIfEmptyRegardlessOfLanguage:
                proceedBuildingContent = contentBuilderManager.IsInstallationEmpty();
                break;
            default:
                break;
        }

        if (!proceedBuildingContent)
            return false;

        contentBuilderManager.ApplyDefaultLanguage();
        contentBuilderManager.CreateRoles(options.Roles);
        contentBuilderManager.CreateUsers(options.Users);

        return true;
    }
}
