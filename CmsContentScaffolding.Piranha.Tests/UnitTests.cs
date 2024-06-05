using CmsContentScaffolding.Piranha.Extensions;
using CmsContentScaffolding.Piranha.Models;
using CmsContentScaffolding.Piranha.Startup;
using CmsContentScaffolding.Shared.Resources;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Piranha;
using Piranha.Data.EF.SQLite;
using Piranha.Extend.Blocks;
using PiranhaCMS.PublicWeb.Models.Blocks;
using PiranhaCMS.PublicWeb.Models.Pages;
using PiranhaCMS.PublicWeb.Models.Sites;

namespace CmsContentScaffolding.Piranha.Tests;

[TestClass]
public class UnitTests
{
    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        var builder = WebHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                .AddConfiguration(context.Configuration)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.unittest.json", false, true)
                .Build();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddCmsContentScaffolding(context.Configuration);
                Globals.Services = services.BuildServiceProvider();
            })
            .Configure(builder =>
            {
                builder.UseCmsContentScaffolding(typeof(StartPage).Assembly,
                    builderOptions: o =>
                    {
                        o.DefaultLanguage = "sr-RS";
                        o.BuildMode = BuildModeEnum.Overwrite;
                        o.PublishContent = true;
                    },
                    builder: b =>
                    {
                        b.UsePages()
                        .WithSite<PublicSite>(s =>
                        {
                            s.SiteFooter.Column1Header = ResourceHelpers.Faker.Lorem.Paragraphs();
                            s.SiteFooter.Column2Header = ResourceHelpers.Faker.Lorem.Paragraphs();
                            s.SiteFooter.Column3Header = ResourceHelpers.Faker.Lorem.Paragraphs();
                        })
                        .WithPage<StartPage>(p =>
                        {
                            p.Title = "StartPage";
                            p.PrimaryImage = PropertyHelpers.AddRandomImage(Globals.Services.GetRequiredService<IApi>(), "PrimaryImage.png", ResourceHelpers.GetImageStream());
                            p.Blocks
                            .Add<TeaserBlock>(block =>
                            {
                                block.Heading = ResourceHelpers.Faker.Lorem.Slug();
                            })
                            .Add<HtmlBlock>(block =>
                            {
                                block.Body = ResourceHelpers.Faker.Lorem.Paragraphs();
                            });
                        }, l1 =>
                        {
                            l1
                            .WithPage<ArticlePage>(p =>
                            {
                                p.Title = "Article1_1";
                                p.PageRegion.Heading = ResourceHelpers.Faker.Lorem.Slug();
                            }, l2 =>
                            {
                                l2
                                .WithPage<ArticlePage>(p =>
                                {
                                    p.Title = "Article2_1";
                                    p.PageRegion.Heading = ResourceHelpers.Faker.Lorem.Slug();
                                })
                                .WithPage<ArticlePage>(p =>
                                {
                                    p.Title = "Article2_2";
                                    p.PageRegion.Heading = ResourceHelpers.Faker.Lorem.Slug();
                                });
                            })
                            .WithPages<ArticlePage>(p =>
                            {
                                p.Title = "Article1_2";
                                p.PageRegion.Heading = ResourceHelpers.Faker.Lorem.Slug();
                            }, 100);
                        })
                        .WithPage<ArticlePage>(p =>
                        {
                            p.Title = "Article2";
                            p.PageRegion.Heading = ResourceHelpers.Faker.Lorem.Slug();
                            p.PageRegion.LeadText = ResourceHelpers.Faker.Lorem.Paragraphs(1);
                            p.PageRegion.MainContent = ResourceHelpers.Faker.Lorem.Paragraphs();
                        })
                        .WithPages<ArticlePage>(p =>
                        {
                            p.Title = "Article3";
                            p.PageRegion.Heading = ResourceHelpers.Faker.Lorem.Slug();
                            p.PageRegion.LeadText = ResourceHelpers.Faker.Lorem.Paragraphs(1);
                            p.PageRegion.MainContent = ResourceHelpers.Faker.Lorem.Paragraphs();
                        }, 100);
                    });
            });

        builder.Build().Start();
    }

    [ClassCleanup]
    public static void Uninitialize()
    {
        #region DB cleanup

        var dbContext = Globals.Services.GetRequiredService<SQLiteDb>();
        dbContext.Database.EnsureDeleted();

        #endregion

        #region Files cleanup

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        if (Directory.Exists(path))
            Directory.Delete(path, true);

        #endregion
    }

    [TestMethod]
    public void TestMethod1()
    {
        //Arrange
        var api = Globals.Services.GetRequiredService<IApi>();

        //Act
        var pages = api.Pages.GetAllAsync().GetAwaiter().GetResult();
        var site = api.Sites.GetDefaultAsync().GetAwaiter().GetResult();
        var defaultLanguage = api.Languages.GetDefaultAsync().GetAwaiter().GetResult();

        //Assert
        Assert.IsNotNull(site);
        Assert.IsTrue(site.LanguageId.Equals(defaultLanguage.Id));
        Assert.IsNotNull(pages);
        Assert.IsTrue(pages.Count() > 100);
    }
}