using CmsContentBuilder.Optimizely.Extensions;
using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Optimizely.Startup;
using EPiServer;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Optimizely.Demo.PublicWeb.Models.Blocks;
using Optimizely.Demo.PublicWeb.Models.Pages;

namespace CmsContentBuilder.Tests;

[TestClass]
public class OptimizelyTests
{
    [TestInitialize]
    public void Initialize()
    {
        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureCmsDefaults()
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
                services.AddSingleton<IHttpContextFactory, DefaultHttpContextFactory>();
                services
                    .AddCmsAspNetIdentity<ApplicationUser>()
                    .AddCms()
                    .AddCmsContentBuilder();
                Globals.Services = services.BuildServiceProvider();

                var dbContext = Globals.Services.GetRequiredService<ApplicationDbContext<ApplicationUser>>();
                dbContext.Database.EnsureCreated();
            })
            .ConfigureWebHostDefaults(config =>
            {
                config.Configure(builder =>
                {
                    builder.UseCmsContentBuilder(
                        builderOptions: new CmsContentApplicationBuilderOptions
                        {
                            DefaultLanguage = "sr-RS",
                            BuildMode = BuildModeEnum.OnlyIfEmptyInDefaultLanguage,
                            RootPage = ContentReference.RootPage,
                            BlocksDefaultLocation = BlocksDefaultLocationEnum.CurrentPage
                        },
                        builder: contentBuilder =>
                        {
                            contentBuilder.WithPage<StartPage>(page =>
                            {
                                page.Name = "StartPage";
                                page.MainContentArea
                                .AddBlock<TeaserBlock>()
                                .AddBlock<TeaserBlock>(block =>
                                {
                                    block.Heading = PropertyHelpers.AddRandomText();
                                    block.LeadText = PropertyHelpers.AddRandomText(150);
                                });
                            }, level1 =>
                            {
                                level1.WithSubPage<ArticlePage>(page =>
                                {
                                    page.Name = "Article1_1";
                                    page.MainContent = PropertyHelpers.AddRandomHtml();
                                }, level2 =>
                                {
                                    level2.WithSubPage<ArticlePage>(page =>
                                    {
                                        page.Name = "Article2_1";
                                    });
                                    level2.WithSubPage<ArticlePage>(options: level3 =>
                                    {
                                        level3.WithSubPages<ArticlePage>(totalPages: 20);
                                    });
                                });
                                level1.WithSubPages<ArticlePage>(totalPages: 100);
                            });

                            contentBuilder.WithPage<ArticlePage>();

                            contentBuilder.WithPages<ArticlePage>(page =>
                            {
                                page.Name = "Article2";
                            }, 10);
                        });
                });
            });

        var app = builder.Build();
        app.Start();
    }

    [TestCleanup]
    public void Uninitialize()
    {
        var dbContext = Globals.Services.GetRequiredService<ApplicationDbContext<ApplicationUser>>();
        dbContext.Database.EnsureDeleted();
    }

    [TestMethod]
    public void TestMethod1()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetRequiredService<IContentLoader>();

        //Act
        var pages = contentLoader.GetDescendents(ContentReference.RootPage);
        var startPage = contentLoader.Get<StartPage>(pages.Single(x => x.ID.Equals(7)));

        //Assert
        Assert.IsNotNull(pages);
        Assert.IsTrue(pages.Count() > 0);
        Assert.IsNotNull(startPage?.MainContentArea);
        Assert.IsFalse(startPage.MainContentArea.IsEmpty);
    }
}