using CmsContentScaffolding.Optimizely.Startup;
using CmsContentScaffolding.Tests.Optimizely.Extensions;
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
using Optimizely.Demo.PublicWeb.Models.Pages;
using static CmsContentScaffolding.Tests.Optimizely.Constants.StringConstants;

namespace CmsContentScaffolding.Tests;

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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddConfiguration(context.Configuration)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.unittest.json", false, true)
                .Build();
            })
            .ConfigureServices((context, services) =>
            {
                services
                .AddSingleton<IHttpContextFactory, DefaultHttpContextFactory>()
                .AddCmsAspNetIdentity<ApplicationUser>()
                .AddCms()
                .AddCmsContentScaffolding();

                Globals.Services = services.BuildServiceProvider();

                var dbContext = Globals.Services.GetRequiredService<ApplicationDbContext<ApplicationUser>>();
                dbContext.Database.EnsureCreated();
            })
            .ConfigureWebHostDefaults(config =>
            {
                config.UseUrls(Site1HostUrl, Site2HostUrl);
                config.Configure(app =>
                {
                    app.UseCmsContentScaffolding();
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
            .Where(x => x.GetHosts(Language, false).Any())
            .Single();
        var startPage = contentLoader.Get<StartPage>(siteDefinition.StartPage, Language);

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
        var res = pageCriteriaQueryService.FindAllPagesWithCriteria(
            ContentReference.RootPage,
            criterias,
            Language.TwoLetterISOLanguageName,
            LanguageSelector.MasterLanguage());

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
            .GetChildren<ArticlePage>(ContentReference.RootPage, Language)
            .Where(x => x.MainContentArea != null && x.MainContentArea.Count > 0)
            .ToArray();

        //Assert
        Assert.IsNotNull(res);
        Assert.IsTrue(res.Length > 0);
    }

    [TestMethod]
    public void LocalBlockTest_ShouldHaveValues()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

        //Act
        var res = contentLoader
            .GetChildren<NotFoundPage>(ContentReference.RootPage, Language)
            .Single();

        //Assert
        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Teaser);
        Assert.IsNotNull(res.Teaser.Heading);
        Assert.IsFalse(ContentReference.IsNullOrEmpty(res.Teaser.Image));
        Assert.IsNotNull(res.Teaser.LinkButton);
        Assert.IsFalse(string.IsNullOrEmpty(res.Teaser.LinkButton.LinkText));
        Assert.IsNotNull(res.Teaser.LinkButton.LinkUrl);
    }

    [TestMethod]
    public async Task GetSiteStartPage_ShouldReturnHtml()
    {
        //Arrange
        var client = new HttpClient
        {
            BaseAddress = new Uri(Site1HostUrl)
        };

        //Act
        var res = await client.GetAsync("/");

        //Assert
        Assert.IsNotNull(res);
        client.Dispose();
    }

    [TestMethod]
    public async Task GetSiteArticlePage_ShouldReturnHtml()
    {
        //Arrange
        var client = new HttpClient
        {
            BaseAddress = new Uri(Site1HostUrl)
        };

        //Act
        var res = await client.GetAsync("/article1");

        //Assert
        Assert.IsNotNull(res);
        client.Dispose();
    }

    [TestMethod]
    public void GetBlocksFromFolder_ShouldReturnBlocks()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        var siteDefinitionRepository = ServiceLocator.Current.GetRequiredService<ISiteDefinitionRepository>();
        var res = contentLoader.GetChildren<ContentFolder>(siteDefinitionRepository.Get("Site 1").SiteAssetsRoot, Language);

        //Act
        var blocks = contentLoader.GetChildren<BlockData>(res.First().ContentLink, Language);

        //Assert
        Assert.IsNotNull(res);
        Assert.IsNotNull(blocks);
        Assert.IsTrue(blocks.Count() > 0);
    }

    [TestMethod]
    public void GetAssetsFromSite1StartPage_ShouldReturnAssets()
    {
        //Arrange
        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        var siteDefinitionRepository = ServiceLocator.Current.GetRequiredService<ISiteDefinitionRepository>();
        var assetsHelper = ServiceLocator.Current.GetRequiredService<ContentAssetHelper>();
        var site = siteDefinitionRepository.Get("Site 1");

        //Act
        var res = contentLoader.GetChildren<IContentData>(assetsHelper.GetAssetFolder(site.StartPage).ContentLink, Language);

        //Assert
        Assert.IsNotNull(res);
        Assert.IsTrue(res.Count() > 0);
    }
}