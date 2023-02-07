using CmsContentBuilder.Piranha.Extensions;
using CmsContentBuilder.Piranha.Models;
using CmsContentBuilder.Piranha.Startup;
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

namespace CmsContentBuilder.Tests;

[TestClass]
public class PiranhaTests
{
    [TestInitialize]
    public void Initialize()
    {
        var builder = WebHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                .AddConfiguration(context.Configuration)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                .Build();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddCmsContentBuilder(context.Configuration);
                Globals.Services = services.BuildServiceProvider();
            })
            .Configure(builder =>
            {
                builder.UseCmsContentBuilder(typeof(StartPage).Assembly, contentBuilder =>
                {
                    contentBuilder.BuildMode = BuildModeEnum.Overwrite;
                    contentBuilder.DefaultLanguage = "sr-RS";

                    contentBuilder.WithSite<PublicSite>(site =>
                    {
                        site.SiteFooter.Column1Header = PropertyHelpers.AddRandomText();
                        site.SiteFooter.Column2Header = PropertyHelpers.AddRandomText();
                        site.SiteFooter.Column3Header = PropertyHelpers.AddRandomText();
                    });

                    contentBuilder.WithPage<StartPage>(page =>
                    {
                        page.Title = "StartPage";
                        page.PrimaryImage = PropertyHelpers.AddRandomImage(Globals.Services.GetRequiredService<IApi>());
                        page.Blocks
                        .Add<TeaserBlock>(block =>
                        {
                            block.Heading = PropertyHelpers.AddRandomText();
                        })
                        .Add<HtmlBlock>(block =>
                        {
                            block.Body = PropertyHelpers.AddRandomHtml();
                        });
                    }, level1 =>
                    {
                        level1.WithSubPage<ArticlePage>(page =>
                        {
                            page.Title = "Article1_1";
                            page.PageRegion.Heading = PropertyHelpers.AddRandomText();
                        }, level2 =>
                        {
                            level2.WithSubPage<ArticlePage>(page =>
                            {
                                page.Title = "Article2_1";
                                page.PageRegion.Heading = PropertyHelpers.AddRandomText();
                            });
                            level2.WithSubPage<ArticlePage>(page =>
                            {
                                page.Title = "Article2_2";
                                page.PageRegion.Heading = PropertyHelpers.AddRandomText();
                            });
                        });
                        level1.WithSubPages<ArticlePage>(page =>
                        {
                            page.Title = "Article1_2";
                            page.PageRegion.Heading = PropertyHelpers.AddRandomText();
                        }, 100);
                    });

                    contentBuilder.WithPage<ArticlePage>(page =>
                    {
                        page.Title = "Article2";
                        page.PageRegion.Heading = PropertyHelpers.AddRandomText();
                        page.PageRegion.LeadText = PropertyHelpers.AddRandomText(100);
                        page.PageRegion.MainContent = PropertyHelpers.AddRandomHtml();
                    });

                    contentBuilder.WithPages<ArticlePage>(page =>
                    {
                        page.Title = "Article2";
                        page.PageRegion.Heading = PropertyHelpers.AddRandomText();
                        page.PageRegion.LeadText = PropertyHelpers.AddRandomText(150);
                        page.PageRegion.MainContent = PropertyHelpers.AddRandomHtml();
                    }, 10);
                });
            });

        builder.Build().Start();
    }

    [TestCleanup]
    public void Uninitialize()
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
        var api = Globals.Services.GetService<IApi>();

        //Act
        var pages = api.Pages.GetAllAsync().GetAwaiter().GetResult();
        var site = api.Sites.GetDefaultAsync().GetAwaiter().GetResult();
        var defaultLanguage = api.Languages.GetDefaultAsync().GetAwaiter().GetResult();

        //Assert
        Assert.IsNotNull(site);
        Assert.IsTrue(site.LanguageId.Equals(defaultLanguage.Id));
        Assert.IsNotNull(pages);
        Assert.IsTrue(pages.Count() > 0);
    }
}