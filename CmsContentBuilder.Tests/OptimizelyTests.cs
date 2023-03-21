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
using Optimizely.Demo.PublicWeb.Models.Media;
using Optimizely.Demo.PublicWeb.Models.Pages;
using System.Globalization;

namespace CmsContentBuilder.Tests;

[TestClass]
public class OptimizelyTests
{
    private const string Language = "sr";

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureCmsDefaults()
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddConfiguration(context.Configuration)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.unittest.json", true, true)
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
                        builderOptions: o =>
                        {
                            o.DefaultLanguage = Language;
                            o.BuildMode = BuildModeEnum.OnlyIfEmptyInDefaultLanguage;
                            o.RootPage = ContentReference.RootPage;
                            o.StartPageType = typeof(StartPage);
                            o.PublishContent = true;
                            o.BlocksDefaultLocation = BlocksDefaultLocationEnum.CurrentPage;
                        },
                        builder: b =>
                        {
                            b
                            .WithPage<StartPage>(p =>
                            {
                                p.Name = "StartPage";
                                p.MainContentArea
                                .AddBlock<TeaserBlock>()
                                .AddBlock<TeaserBlock>(block =>
                                {
                                    block.Heading = PropertyHelpers.AddRandomText();
                                    block.LeadText = PropertyHelpers.AddRandomText(150);
                                });
                            }, l1 =>
                            {
                                l1
                                .WithSubPage<ArticlePage>(p =>
                                {
                                    p.Name = "Article1_1";
                                    p.Heading = PropertyHelpers.AddRandomText();
                                    p.LeadText = PropertyHelpers.AddRandomText(150);
                                    p.MainContent = PropertyHelpers.AddRandomHtml();
                                    p.TopImage = PropertyHelpers.AddRandomImage<ImageFile>();
                                }, l2 =>
                                {
                                    l2
                                    .WithSubPage<ArticlePage>(p =>
                                    {
                                        p.Name = "Article2_1";
                                        p.Heading = PropertyHelpers.AddRandomText();
                                        p.LeadText = PropertyHelpers.AddRandomText(150);
                                        p.MainContent = PropertyHelpers.AddRandomHtml();
                                    })
                                    .WithSubPage<ArticlePage>(options: l3 =>
                                    {
                                        l3.WithSubPages<ArticlePage>(p =>
                                        {
                                            p.Heading = PropertyHelpers.AddRandomText();
                                            p.LeadText = PropertyHelpers.AddRandomText(150);
                                            p.MainContent = PropertyHelpers.AddRandomHtml();
                                        }, 20);
                                    });
                                })
                                .WithSubPages<ArticlePage>(p =>
                                {
                                    p.Heading = PropertyHelpers.AddRandomText();
                                    p.LeadText = PropertyHelpers.AddRandomText(150);
                                    p.MainContent = PropertyHelpers.AddRandomHtml();
                                    p.MainContentArea.AddBlock<TeaserBlock>();
                                }, 100);
                            })
                            .WithPage<ArticlePage>()
                            .WithPages<ArticlePage>(p =>
                            {
                                p.Name = "Article2";
                                p.MainContentArea.AddBlocks<TeaserBlock>(block =>
                                {
                                    block.Heading = PropertyHelpers.AddRandomText();
                                    block.LeadText = PropertyHelpers.AddRandomText(150);
                                    block.Image = PropertyHelpers.AddRandomImage<ImageFile>();
                                }, 10);
                            }, 10);
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
        var siteDefinition = siteDefinitionRepository
            .List()
            .Where(x => x.GetHosts(new CultureInfo(Language), false).Any())
            .Single();
        var startPage = contentLoader.Get<StartPage>(siteDefinition.StartPage);

        //Assert
        Assert.IsNotNull(pages);
        Assert.IsTrue(pages.Any());
        Assert.IsNotNull(startPage);
        Assert.IsNotNull(startPage.MainContentArea);
        Assert.IsFalse(startPage.MainContentArea.IsEmpty);
    }

    [TestMethod]
    public void SiteDefinitions_ShouldHaveDefaultSiteDefinition()
    {
        //Arrange
        var siteDefinitionRepository = ServiceLocator.Current.GetRequiredService<ISiteDefinitionRepository>();

        //Act
        var siteDefinitions = siteDefinitionRepository.List();

        //Assert
        Assert.IsTrue(siteDefinitions.Any());
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
        var res = pageCriteriaQueryService.FindAllPagesWithCriteria(PageReference.RootPage, criterias, Language, LanguageSelector.MasterLanguage());

        //Assert
        Assert.IsNotNull(res);
        Assert.IsTrue(res.Count > 100);
    }

    [TestMethod]
    public void PerformanceTest_ShouldGetAllArticlePagesUsingContentLoader()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

        //Act
        var res = contentLoader
            .GetDescendents(ContentReference.RootPage)
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
        Assert.IsTrue(res.Length > 100);
    }

    [TestMethod]
    public void ArticlePageBlocksTest_ShouldGetAllBlocksFromMainContentArea()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

        //Act
        var res = contentLoader
            .GetChildren<ArticlePage>(ContentReference.RootPage, new LoaderOptions { LanguageLoaderOption.MasterLanguage() })
            .Where(x => x.MainContentArea != null && x.MainContentArea.Count.Equals(10))
            .ToArray();

        //Assert
        Assert.IsNotNull(res);
        Assert.IsTrue(res.Length == 10);
    }
}