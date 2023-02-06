using CmsContentBuilder.Optimizely.Builders;
using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Optimizely.Services;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
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
        CmsContentApplicationBuilderOptions? builderOptions = null)
    {
        if (ApplyOptions(app.ApplicationServices, builderOptions))
        {
            var appBuilder = app.ApplicationServices.GetRequiredService<ICmsContentApplicationBuilder>();
            builder.Invoke(appBuilder);
        }
    }

    private static bool ApplyOptions(IServiceProvider services, CmsContentApplicationBuilderOptions? builderOptions)
    {
        var proceedBuildingContent = false;
        var options = services.GetRequiredService<CmsContentApplicationBuilderOptions>();

        if (builderOptions != null)
        {
            options.DefaultLanguage = builderOptions.DefaultLanguage;
            options.BlocksDefaultLocation = builderOptions.BlocksDefaultLocation;
            options.RootPage = builderOptions.RootPage;
            options.BuildMode = builderOptions.BuildMode;
        }

        switch (options.BuildMode)
        {
            case BuildModeEnum.Append:
                proceedBuildingContent = true;
                break;
            case BuildModeEnum.Overwrite:
                proceedBuildingContent = true;
                break;
            case BuildModeEnum.OnlyIfEmptyInDefaultLanguage:
                proceedBuildingContent = IsInstallationEmpty(services, options);
                break;
            case BuildModeEnum.OnlyIfEmptyRegardlessOfLanguage:
                proceedBuildingContent = IsInstallationEmpty(services, options);
                break;
            default:
                break;
        }

        if (!proceedBuildingContent)
            return false;

        //ApplyDefaultUser(services, options);
        ApplyDefaultLanguage(services, options);

        return true;
    }

    //private static void ApplyDefaultUser(IServiceProvider services, CmsContentApplicationBuilderOptions options)
    //{
    //    var users = services.GetRequiredService<IUsersService>();

    //    if (options.CreateNewUser && string.IsNullOrEmpty(options.UserSettings.UserName))
    //    {
    //        users.CreateUser(
    //            options.UserSettings.UserName,
    //            options.UserSettings.UserEmail,
    //            options.UserSettings.Password,
    //            new[] { options.UserSettings.UserRole })
    //            .GetAwaiter()
    //            .GetResult();
    //    }
    //    else if (options.CreateNewUser &&
    //        !string.IsNullOrEmpty(options.UserSettings.UserName) &&
    //        !string.IsNullOrEmpty(options.UserSettings.UserEmail) &&
    //        !string.IsNullOrEmpty(options.UserSettings.Password) &&
    //        !string.IsNullOrEmpty(options.UserSettings.UserRole))
    //    {
    //        users.CreateUser(
    //            options.UserSettings.UserName,
    //            options.UserSettings.UserEmail,
    //            options.UserSettings.Password,
    //            new[] { options.UserSettings.UserRole })
    //            .GetAwaiter()
    //            .GetResult();
    //    }
    //}

    private static bool IsInstallationEmpty(IServiceProvider services, CmsContentApplicationBuilderOptions options)
    {
        var contentLoader = services.GetRequiredService<IContentLoader>();
        var languageBranchRepository = services.GetRequiredService<ILanguageBranchRepository>();

        if (options.BuildMode.Equals(BuildModeEnum.OnlyIfEmptyInDefaultLanguage))
        {
            if (languageBranchRepository.ListAll().Any(x => x.LanguageID.Equals(options.DefaultLanguage, StringComparison.InvariantCultureIgnoreCase)))
            {
                var pages = contentLoader.GetChildren<IContentData>(options.RootPage, new CultureInfo(options.DefaultLanguage));
                return pages is null || !pages.Any();
            }
            else
            {
                return true;
            }
        }
        else if (options.BuildMode.Equals(BuildModeEnum.OnlyIfEmptyRegardlessOfLanguage))
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
