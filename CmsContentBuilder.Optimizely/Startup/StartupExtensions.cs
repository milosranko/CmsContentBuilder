using CmsContentBuilder.Optimizely.Builders;
using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Optimizely.Services;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Shell.Security;
using EPiServer.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CmsContentBuilder.Optimizely.Startup;

public static class StartupExtensions
{
    public static IServiceCollection AddCmsContentBuilder(
        this IServiceCollection services)
    {
        services.AddTransient<IContentBuilder, ContentBuilder>();
        services.AddTransient<IUsersService, UsersService>();
        services.AddScoped(x => new ContentBuilderOptions());

        return services;
    }

    public static void UseCmsContentBuilder(
        this IApplicationBuilder app,
        Action<IContentBuilder> builder,
        Action<ContentBuilderOptions>? builderOptions = null)
    {
        var options = app.ApplicationServices.GetRequiredService<ContentBuilderOptions>();
        builderOptions?.Invoke(options);

        if (ApplyOptions(app.ApplicationServices, options))
        {
            var appBuilder = app.ApplicationServices.GetRequiredService<IContentBuilder>();
            builder.Invoke(appBuilder);
        }
    }

    private static bool ApplyOptions(IServiceProvider services, ContentBuilderOptions options)
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
        CreateRoles(services, options.Roles);
        CreateUsers(services, options.Users);

        return true;
    }

    private static bool IsInstallationEmpty(IServiceProvider services, ContentBuilderOptions options)
    {
        var contentLoader = services.GetRequiredService<IContentLoader>();
        var languageBranchRepository = services.GetRequiredService<ILanguageBranchRepository>();

        if (options.BuildMode.Equals(BuildMode.OnlyIfEmptyInDefaultLanguage))
        {
            if (languageBranchRepository.ListAll().Any(x => x.Culture.Equals(options.DefaultLanguage)))
            {
                var siteDefinitionRepository = services.GetRequiredService<ISiteDefinitionRepository>();
                var siteDefinition = siteDefinitionRepository
                    .List()
                    .Where(x => x.GetHosts(options.DefaultLanguage, false).Any())
                    .SingleOrDefault();
                var pages = contentLoader.GetChildren<IContentData>(options.RootPage, options.DefaultLanguage);

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

    private static void ApplyDefaultLanguage(IServiceProvider services, ContentBuilderOptions options)
    {
        var languageBranchRepository = services.GetRequiredService<ILanguageBranchRepository>();
        var contentLoader = services.GetRequiredService<IContentLoader>();
        var contentRepository = services.GetRequiredService<IContentRepository>();
        var availableLanguages = languageBranchRepository.ListAll();

        foreach (var lang in options.EnabledLanguages)
        {
            if (availableLanguages.Any(x => x.Culture.Equals(lang)))
            {
                var existingLanguage = availableLanguages.Single(x => x.Culture.Equals(lang));

                if (!existingLanguage.Enabled)
                    languageBranchRepository.Enable(existingLanguage.Culture);
            }
            else
            {
                var newLanguageBranch = new LanguageBranch(lang);
                languageBranchRepository.Save(newLanguageBranch);
                languageBranchRepository.Enable(newLanguageBranch.Culture);
            }
        }

        var rootPage = contentLoader.Get<PageData>(options.RootPage);
        if (!rootPage.ExistingLanguages.Any(x => x.Equals(options.DefaultLanguage)))
        {
            var rootPageClone = rootPage.CreateWritableClone();
            rootPageClone.ExistingLanguages.Append(options.DefaultLanguage);
            contentRepository.Save(rootPageClone, SaveAction.Default, AccessLevel.NoAccess);
        }
    }

    private static void CreateRoles(IServiceProvider services, IEnumerable<string> roles)
    {
        if (!roles.Any()) return;

        var roleProvider = services.GetService<UIRoleProvider>();

        foreach (var role in roles)
        {
            if (!roleProvider.RoleExistsAsync(role).GetAwaiter().GetResult())
                roleProvider.CreateRoleAsync(role).GetAwaiter().GetResult();
        }
    }

    private static void CreateUsers(IServiceProvider services, IEnumerable<UserModel> users)
    {
        if (!users.Any()) return;

        var userProvider = services.GetService<UIUserProvider>();
        var roleProvider = services.GetService<UIRoleProvider>();
        IUIUser? uiUser = default;

        foreach (var user in users)
        {
            uiUser = userProvider.GetUserAsync(user.UserName).GetAwaiter().GetResult();

            if (uiUser != null)
                continue;

            userProvider.CreateUserAsync(user.UserName, user.Password, user.Email, null, null, true).GetAwaiter().GetResult();

            if (user.Roles.Any())
            {
                roleProvider.AddUserToRolesAsync(user.UserName, user.Roles).GetAwaiter().GetResult();
            }
        }
    }
}
