using Bogus;
using CmsContentScaffolding.Optimizely.Extensions;
using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Models;
using CmsContentScaffolding.Optimizely.Startup;
using EPiServer;
using EPiServer.Core;
using EPiServer.Security;
using Microsoft.AspNetCore.Builder;
using Optimizely.Demo.PublicWeb.Models.Blocks;
using Optimizely.Demo.PublicWeb.Models.Media;
using Optimizely.Demo.PublicWeb.Models.Pages;
using System.Globalization;
using static CmsContentScaffolding.Tests.Optimizely.Constants.StringConstants;

namespace CmsContentScaffolding.Tests.Optimizely.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCmsContentScaffolding(this IApplicationBuilder app)
    {
        var faker = new Faker();

        app.UseCmsContentScaffolding(
            builderOptions: o =>
            {
                o.SiteName = "Site 1";
                o.EnabledLanguages.Add(new CultureInfo("sr"));
                o.DefaultHost = Site1HostUrl;
                o.DefaultLanguage = new CultureInfo("sr");
                o.BuildMode = BuildMode.Append;
                o.StartPageType = typeof(StartPage);
                o.PublishContent = true;
                o.BlocksLocation = BlocksLocation.CurrentContent;
                o.Roles = new Dictionary<string, AccessLevel>
                {
                    { Site1EditorsRole, AccessLevel.Edit }
                };
                o.Users = new List<UserModel>
                {
                    new UserModel
                    {
                        UserName = "Site1User",
                        Email = "Site1User@test.com",
                        Password = TestUserPassword,
                        Roles = new[] { Site1EditorsRole }
                    }
                };
            },
            builder: b =>
            {
                b
                .WithPage<StartPage>(p =>
                {
                    p.Name = "Home Page";
                    p.OpenGraphImage = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    p.MainContentArea
                    .AddItems<TeaserBlock>("Teaser Test", b =>
                    {
                        b.Heading = faker.Lorem.Slug();
                        b.LeadText = faker.Lorem.Paragraph();
                        b.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    },
                    3,
                    new AssetOptions { BlocksLocation = BlocksLocation.CurrentContent });
                }, l1 =>
                {
                    l1
                    .WithPage<ArticlePage>(p =>
                    {
                        p.Name = faker.Lorem.Slug(2);
                        p.Heading = faker.Lorem.Slug();
                        p.LeadText = faker.Lorem.Paragraph();
                        p.MainContent
                        .AddStringFragment(faker.Lorem.Paragraphs())
                        .AddContentFragment(PropertyHelpers.GetOrAddRandomImage<ImageFile>())
                        .AddStringFragment(faker.Lorem.Paragraphs());
                        p.TopImage = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                        p.MainContentArea
                        .AddItem<AccordionContainerBlock>("Accordion Container", b =>
                        {
                            b.Heading = faker.Lorem.Slug();
                            b.Items.AddItems<AccordionItemBlock>("Accordion Item", b1 =>
                            {
                                b1.Heading = faker.Lorem.Slug();
                                b1.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                                b1.LeadText = faker.Lorem.Paragraph();
                            }, 5);
                        }, new AssetOptions { BlocksLocation = BlocksLocation.GlobalRoot, FolderName = "Accordion" })
                        .AddItem<ImageFile>(options: i =>
                        {
                            i.Name = "Test Image";
                            i.ContentLink = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                        })
                        .AddExistingItem<AccordionContainerBlock>("Accordion Container", new AssetOptions { BlocksLocation = BlocksLocation.GlobalRoot, FolderName = "Accordion" });
                    }, l2 =>
                    {
                        l2
                        .WithPage<ArticlePage>(p =>
                        {
                            p.Name = "Article2_1";
                            p.Heading = faker.Lorem.Slug();
                            p.LeadText = faker.Lorem.Paragraph();
                            p.MainContent = new XhtmlString(faker.Lorem.Paragraphs(5));
                        })
                        .WithPage<ArticlePage>(l3 =>
                        {
                            l3.WithPages<ArticlePage>(p =>
                            {
                                p.Heading = faker.Lorem.Slug();
                                p.LeadText = faker.Lorem.Paragraph();
                                p.MainContent = new XhtmlString(faker.Lorem.Paragraphs(7));
                            }, 20);
                        });
                    })
                    .WithPages<ArticlePage>(p =>
                    {
                        p.Heading = faker.Lorem.Slug();
                        p.LeadText = faker.Lorem.Paragraph();
                        p.MainContent = new XhtmlString(faker.Lorem.Paragraphs(10));
                        p.MainContentArea.AddItem<TeaserBlock>(p.Name);
                    }, 100);
                })
                .WithPage<NotFoundPage>(p =>
                {
                    p.Name = "Not Found Page";
                    p.Teaser.Heading = faker.Lorem.Slug(3);
                    p.Teaser.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    p.Teaser.LeadText = faker.Lorem.Paragraph(2);
                    p.Teaser.LinkButton.LinkText = faker.Internet.DomainName();
                    p.Teaser.LinkButton.LinkUrl = new Url(faker.Internet.Url());
                })
                .WithPages<ArticlePage>(p =>
                {
                    p.Name = faker.Lorem.Slug(2);
                    p.MainContentArea.AddItems<TeaserBlock>(block =>
                    {
                        block.Heading = faker.Lorem.Slug();
                        block.LeadText = faker.Lorem.Paragraph();
                        block.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    }, 10, new AssetOptions { BlocksLocation = BlocksLocation.CurrentContent });
                }, 10)
                .WithPages<ArticlePage>(p =>
                {
                    p.Name = faker.Lorem.Slug(3);
                    p.MainContentArea.AddItems<TeaserBlock>(block =>
                    {
                        block.Heading = faker.Lorem.Slug();
                        block.LeadText = faker.Lorem.Paragraph();
                        block.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    }, 2, new AssetOptions { FolderName = TeaserBlocksFolderName });
                }, 2);
            });

        app.UseCmsContentScaffolding(
            builderOptions: o =>
            {
                o.SiteName = "Site 2";
                o.DefaultHost = "https://localhost:5001";
                o.DefaultLanguage = new CultureInfo("en");
                o.BuildMode = BuildMode.OnlyIfEmptyInDefaultLanguage;
                o.StartPageType = typeof(StartPage);
                o.PublishContent = true;
                o.BlocksLocation = BlocksLocation.CurrentContent;
                o.Roles = new Dictionary<string, AccessLevel>
                {
                    { Site2EditorsRole, AccessLevel.Edit }
                };
                o.Users = new List<UserModel>
                {
                    new UserModel
                    {
                        UserName = "Site2User",
                        Email = "Site2User@test.com",
                        Password = TestUserPassword,
                        Roles = new[] { Site2EditorsRole }
                    }
                };
            },
            builder: b =>
            {
                b
                .WithPage<StartPage>(p =>
                {
                    p.Name = "Home Page";
                    p.OpenGraphImage = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    p.MainContentArea
                    .AddItems<TeaserBlock>("Teaser Test", b =>
                    {
                        b.Heading = PropertyHelpers.AddRandomText();
                        b.LeadText = PropertyHelpers.AddRandomText(150);
                        b.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    }, 3, new AssetOptions { BlocksLocation = BlocksLocation.CurrentContent });
                }, l1 =>
                {
                    l1
                    .WithPage<ArticlePage>(p =>
                    {
                        p.Name = "Article1_1";
                        p.Heading = PropertyHelpers.AddRandomText();
                        p.LeadText = PropertyHelpers.AddRandomText(150);
                        p.MainContent = PropertyHelpers.AddRandomHtml();
                        p.TopImage = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                        p.MainContentArea
                        .AddItem<AccordionContainerBlock>("Accordion Container", b =>
                        {
                            b.Heading = PropertyHelpers.AddRandomText();
                            b.Items.AddItems<AccordionItemBlock>("Accordion Item", b1 =>
                            {
                                b1.Heading = PropertyHelpers.AddRandomText();
                                b1.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                                b1.LeadText = PropertyHelpers.AddRandomText(105);
                            }, 5);
                        }, new AssetOptions { BlocksLocation = BlocksLocation.GlobalRoot, FolderName = "Accordion" })
                        .AddItem<ImageFile>(options: i =>
                        {
                            i.Name = "Test Image";
                            i.ContentLink = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                        });
                    }, l2 =>
                    {
                        l2
                        .WithPage<ArticlePage>(p =>
                        {
                            p.Name = "Article2_1";
                            p.Heading = PropertyHelpers.AddRandomText();
                            p.LeadText = PropertyHelpers.AddRandomText(150);
                            p.MainContent = PropertyHelpers.AddRandomHtml();
                        })
                        .WithPage<ArticlePage>(l3 =>
                        {
                            l3.WithPages<ArticlePage>(p =>
                            {
                                p.Heading = PropertyHelpers.AddRandomText();
                                p.LeadText = PropertyHelpers.AddRandomText(150);
                                p.MainContent = PropertyHelpers.AddRandomHtml();
                            }, 20);
                        });
                    })
                    .WithPages<ArticlePage>(p =>
                    {
                        p.Heading = PropertyHelpers.AddRandomText();
                        p.LeadText = PropertyHelpers.AddRandomText(150);
                        p.MainContent = PropertyHelpers.AddRandomHtml();
                        p.MainContentArea.AddItem<TeaserBlock>(p.Name);
                    }, 100);
                })
                .WithPage<NotFoundPage>(p =>
                {
                    p.Name = "NotFoundPage";
                    p.Teaser.Heading = PropertyHelpers.AddRandomText(20);
                    p.Teaser.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    p.Teaser.LeadText = PropertyHelpers.AddRandomText(50);
                    p.Teaser.LinkButton.LinkText = PropertyHelpers.AddRandomText(15);
                    p.Teaser.LinkButton.LinkUrl = new Url("https://google.com");
                })
                .WithPages<ArticlePage>(p =>
                {
                    p.Name = "Article2";
                    p.MainContentArea.AddItems<TeaserBlock>(block =>
                    {
                        block.Heading = PropertyHelpers.AddRandomText();
                        block.LeadText = PropertyHelpers.AddRandomText(150);
                        block.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    }, 10, new AssetOptions { BlocksLocation = BlocksLocation.CurrentContent });
                }, 10)
                .WithPages<ArticlePage>(p =>
                {
                    p.Name = "Articles4";
                    p.MainContentArea.AddItems<TeaserBlock>(block =>
                    {
                        block.Heading = PropertyHelpers.AddRandomText();
                        block.LeadText = PropertyHelpers.AddRandomText(150);
                        block.Image = PropertyHelpers.GetOrAddRandomImage<ImageFile>();
                    }, 2, new AssetOptions { FolderName = TeaserBlocksFolderName });
                }, 2);
            });

        return app;
    }
}
