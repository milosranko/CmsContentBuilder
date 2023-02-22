using CmsContentBuilder.Optimizely.Extensions;
using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Optimizely.Startup;
using EPiServer;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using EPiServer.ServiceLocation;
using EPiServer.Web;
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
    [ClassInitialize]
    public static void Initialize(TestContext context)
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
                config.Configure(app =>
                {
                    app.UseCmsContentBuilder(
                        builderOptions: contentBuilderOptions =>
                        {
                            contentBuilderOptions.DefaultLanguage = "sr-RS";
                            contentBuilderOptions.BuildMode = BuildModeEnum.OnlyIfEmptyInDefaultLanguage;
                            contentBuilderOptions.RootPage = ContentReference.RootPage;
                            contentBuilderOptions.PublishContent = true;
                            contentBuilderOptions.BlocksDefaultLocation = BlocksDefaultLocationEnum.CurrentPage;
                            contentBuilderOptions.StartPageType = typeof(StartPage);
                        },
                        builder: contentBuilder =>
                        {
                            contentBuilder
                            .WithPage<StartPage>(page =>
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
                                level1
                                .WithSubPage<ArticlePage>(page =>
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
                                })
                                .WithSubPages<ArticlePage>(totalPages: 1000);
                            })
                            .WithPage<ArticlePage>()
                            .WithPages<ArticlePage>(page =>
                            {
                                page.Name = "Article2";
                            }, 100);
                        });
                });
            });

        builder.Build().Start();
    }

    [ClassCleanup]
    public static void Uninitialize()
    {
        var dbContext = Globals.Services.GetRequiredService<ApplicationDbContext<ApplicationUser>>();
        dbContext.Database.EnsureDeleted();
    }

    [TestMethod]
    public void InitializationTest_ShouldGetStartPage()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetRequiredService<IContentLoader>();
        var siteDefinitionRepository = ServiceLocator.Current.GetRequiredService<ISiteDefinitionRepository>();

        //Act
        var pages = contentLoader.GetDescendents(ContentReference.RootPage);
        var startPage = contentLoader.Get<StartPage>(siteDefinitionRepository.List().First().StartPage);

        //Assert
        Assert.IsNotNull(pages);
        Assert.IsTrue(pages.Any());
        Assert.IsNotNull(startPage);
        Assert.IsNotNull(startPage.MainContentArea);
        Assert.IsFalse(startPage.MainContentArea.IsEmpty);
    }

    [TestMethod]
    public void PerformanceTest_ShouldGetAllArticlePagesUsingPageCriteriaQueryService()
    {
        //Arrange
        var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
        var pageCriteriaQueryService = ServiceLocator.Current.GetInstance<IPageCriteriaQueryService>();
        var criterias = new PropertyCriteriaCollection
        {
            new PropertyCriteria
            {
                Name = "PageTypeID",
                Type = PropertyDataType.PageType,
                Condition = CompareCondition.Equal,
                Value = contentTypeRepository.Load<ArticlePage>().ID.ToString(),
                Required = true
            }
        };

        //Act
        var res = pageCriteriaQueryService.FindAllPagesWithCriteria(PageReference.RootPage, criterias, "sr-RS", LanguageSelector.MasterLanguage());

        //Assert
        Assert.IsNotNull(res);
        Assert.IsTrue(res.Count > 1000);
    }

    [TestMethod]
    public void PerformanceTest_ShouldGetAllArticlePagesUsingContentLoader()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

        //Act
        var res = contentLoader.GetDescendents(ContentReference.RootPage)
            .Where(x =>
            {
                if (contentLoader.TryGet<PageData>(x, out var page))
                {
                    return page is ArticlePage;
                }

                return false;
            })
            .ToArray();

        //Assert
        Assert.IsNotNull(res);
        Assert.IsTrue(res.Length > 1000);
    }
}