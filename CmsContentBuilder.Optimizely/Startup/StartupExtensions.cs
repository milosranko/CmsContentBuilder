using CmsContentBuilder.Optimizely.Builders;
using CmsContentBuilder.Optimizely.Interfaces;
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
        var users = app.ApplicationServices.GetRequiredService<IUsersService>();
        users.CreateUser("admin@test.com", "admin@test.com", "9hDR=1h|i]K#,o8l", new[] { "WebAdmins" }).GetAwaiter().GetResult();

        if (builderOptions != null)
        {
            switch (builderOptions.BuildMode)
            {
                case Models.BuildModeEnum.Append:
                    break;
                case Models.BuildModeEnum.Overwrite:
                    //TODO Delete all previously created content
                    break;
                case Models.BuildModeEnum.OnlyIfEmpty:
                    //TODO Check if there are any pages already created, if yes then return
                    break;
                default:
                    break;
            }

            ApplyOptions(app.ApplicationServices, builderOptions);
        }

        var appBuilder = app.ApplicationServices.GetRequiredService<ICmsContentApplicationBuilder>();
        builder.Invoke(appBuilder);
    }

    private static void ApplyOptions(IServiceProvider services, CmsContentApplicationBuilderOptions builderOptions)
    {
        var options = services.GetRequiredService<CmsContentApplicationBuilderOptions>();
        options.DefaultLanguage = builderOptions.DefaultLanguage;

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

        var rootPage = contentLoader.Get<PageData>(ContentReference.RootPage);
        if (!rootPage.ExistingLanguages.Any(x => x.Name.Equals(options.DefaultLanguage, StringComparison.InvariantCultureIgnoreCase)))
        {
            var rootPageClone = rootPage.CreateWritableClone();
            rootPageClone.ExistingLanguages.Append(new CultureInfo(options.DefaultLanguage));
            contentRepository.Save(rootPageClone, SaveAction.Default, AccessLevel.NoAccess);
        }
    }
}
