using CmsContentBuilder.Piranha.Interfaces;
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

        return services;
    }

    public static void UseCmsContentBuilder(
        this IApplicationBuilder app,
        Assembly modelsAssembly,
        Action<ICmsContentApplicationBuilder> options)
    {
        var api = app.ApplicationServices.GetRequiredService<IApi>();
        App.Init(api);

        new ContentTypeBuilder(api)
            .AddAssembly(modelsAssembly)
            .Build()
            .DeleteOrphans();

        var builder = app.ApplicationServices.GetRequiredService<ICmsContentApplicationBuilder>();
        options.Invoke(builder);
    }
}
