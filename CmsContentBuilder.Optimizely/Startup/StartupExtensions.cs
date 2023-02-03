using CmsContentBuilder.Optimizely.Builders;
using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
            ApplyOptions(app.ApplicationServices, builderOptions);
        }

        //TODO if BuildMode is set to Overwrite then delete all the content prior invoking builder
        //TODO if BuildMode is set to OnlyIfEmpty then check if there are any pages already created prior invoking builder

        var appBuilder = app.ApplicationServices.GetRequiredService<ICmsContentApplicationBuilder>();
        builder.Invoke(appBuilder);
    }

    private static void ApplyOptions(IServiceProvider services, CmsContentApplicationBuilderOptions builderOptions)
    {
        var options = services.GetRequiredService<CmsContentApplicationBuilderOptions>();
        options.DefaultLanguage = builderOptions.DefaultLanguage;

        //TODO Create new language branch
        //Set available languages on root
    }
}
