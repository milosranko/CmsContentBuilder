using CmsContentBuilder.Piranha.Builders;
using CmsContentBuilder.Piranha.Interfaces;
using CmsContentBuilder.Piranha.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.SQLite;
using System.Reflection;

namespace CmsContentBuilder.Piranha.Startup;

public static class StartupExtensions
{
    public static IServiceCollection AddCmsContentBuilder(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPiranha(options =>
        {
            options.UseCms();
            options.UseFileStorage();
            options.UseEF<SQLiteDb>(db =>
                db.UseSqlite(configuration.GetConnectionString("piranha-unitests")));
            options.UseIdentityWithSeed<IdentitySQLiteDb>(db =>
                db.UseSqlite(configuration.GetConnectionString("piranha-unitests")));
        });

        services.AddScoped<ICmsContentApplicationBuilder, CmsContentApplicationBuilder>();
        services.AddScoped(x => new CmsContentApplicationBuilderOptions());

        return services;
    }

    public static void UseCmsContentBuilder(
        this IApplicationBuilder app,
        Assembly modelsAssembly,
        Action<ICmsContentApplicationBuilder> builder,
        Action<CmsContentApplicationBuilderOptions>? builderOptions = null)
    {
        var options = app.ApplicationServices.GetRequiredService<CmsContentApplicationBuilderOptions>();
        builderOptions?.Invoke(options);

        var api = app.ApplicationServices.GetRequiredService<IApi>();
        App.Init(api);

        new ContentTypeBuilder(api)
            .AddAssembly(modelsAssembly)
            .Build()
            .DeleteOrphans();

        var appBuilder = app.ApplicationServices.GetRequiredService<ICmsContentApplicationBuilder>();
        builder.Invoke(appBuilder);
    }
}
