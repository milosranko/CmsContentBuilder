using CmsContentBuilder.Optimizely.Builders;
using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Optimizely.Services;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace CmsContentBuilder.Optimizely.Startup;

public static class StartupExtensions
{
    public static IServiceCollection AddCmsContentBuilder(
        this IServiceCollection services)
    {
        services.AddTransient<ICmsContentApplicationBuilder, CmsContentApplicationBuilder>();
        services.AddTransient<IUsersService, UsersService>();
        services.AddScoped(x => new CmsContentApplicationBuilderOptions());

        return services;
    }

    public static void UseCmsContentBuilder(
        this IApplicationBuilder app,
        Action<ICmsContentApplicationBuilder> builder,
        Action<CmsContentApplicationBuilderOptions>? builderOptions = null)
    {
        var options = app.ApplicationServices.GetRequiredService<CmsContentApplicationBuilderOptions>();
        builderOptions?.Invoke(options);

        if (ApplyOptions(app.ApplicationServices, options))
        {
            var appBuilder = app.ApplicationServices.GetRequiredService<ICmsContentApplicationBuilder>();
            builder.Invoke(appBuilder);
        }
    }

    private static bool ApplyOptions(IServiceProvider services, CmsContentApplicationBuilderOptions options)
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
                proceedBuildingContent = IsInstallationEmpty(services, options);
                break;
            case BuildMode.OnlyIfEmptyRegardlessOfLanguage:
                proceedBuildingContent = IsInstallationEmpty(services, options);
                break;
            default:
                break;
        }

        if (!proceedBuildingContent)
            return false;

        ApplyDefaultLanguage(services, options);

        return true;
    }

    private static bool IsInstallationEmpty(IServiceProvider services, CmsContentApplicationBuilderOptions options)
    {
        var contentLoader = services.GetRequiredService<IContentLoader>();
        var languageBranchRepository = services.GetRequiredService<ILanguageBranchRepository>();

        if (options.BuildMode.Equals(BuildMode.OnlyIfEmptyInDefaultLanguage))
        {
            if (languageBranchRepository.ListAll().Any(x => x.LanguageID.Equals(options.DefaultLanguage, StringComparison.InvariantCultureIgnoreCase)))
            {
                var siteDefinitionRepository = services.GetRequiredService<ISiteDefinitionRepository>();
                var siteDefinition = siteDefinitionRepository
                    .List()
                    .Where(x => x.GetHosts(new CultureInfo(options.DefaultLanguage), false).Any())
                    .SingleOrDefault();
                var pages = contentLoader.GetChildren<IContentData>(options.RootPage, new CultureInfo(options.DefaultLanguage));

                return siteDefinition is null || pages is null || pages.Count() < 3;
            }

            return true;
        }
        else if (options.BuildMode.Equals(BuildMode.OnlyIfEmptyRegardlessOfLanguage))
        {
            var pages = contentLoader.GetChildren<IContentData>(options.RootPage);

            return pages is null || !pages.Any();
        }

        return false;
    }

    private static void ApplyDefaultLanguage(IServiceProvider services, CmsContentApplicationBuilderOptions options)
    {
        var languageBranchRepository = services.GetRequiredService<ILanguageBranchRepository>();
        var contentLoader = services.GetRequiredService<IContentLoader>();
        var contentRepository = services.GetRequiredService<IContentRepository>();
        var availableLanguages = languageBranchRepository.ListAll();

        if (!availableLanguages.Any(x => x.LanguageID.Equals(options.DefaultLanguage, StringComparison.InvariantCultureIgnoreCase)))
        {
            var newLanguageBranch = new LanguageBranch(options.DefaultLanguage);

            languageBranchRepository.Save(newLanguageBranch);
            languageBranchRepository.Enable(newLanguageBranch.Culture);
        }
        else
        {
            var existingLanguage = availableLanguages.Single(x => x.LanguageID.Equals(options.DefaultLanguage, StringComparison.InvariantCultureIgnoreCase));
            if (!existingLanguage.Enabled)
            {
                languageBranchRepository.Enable(existingLanguage.Culture);
            }
        }

        var rootPage = contentLoader.Get<PageData>(options.RootPage);
        if (!rootPage.ExistingLanguages.Any(x => x.Name.Equals(options.DefaultLanguage, StringComparison.InvariantCultureIgnoreCase)))
        {
            var rootPageClone = rootPage.CreateWritableClone();
            rootPageClone.ExistingLanguages.Append(new CultureInfo(options.DefaultLanguage));
            contentRepository.Save(rootPageClone, SaveAction.Default, AccessLevel.NoAccess);
        }
    }
}
