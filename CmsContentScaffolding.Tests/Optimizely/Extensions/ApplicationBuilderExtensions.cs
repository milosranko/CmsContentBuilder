using CmsContentScaffolding.Optimizely.Extensions;
using CmsContentScaffolding.Optimizely.Models;
using CmsContentScaffolding.Optimizely.Startup;
using EPiServer;
using EPiServer.Authorization;
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
        app.UseCmsContentScaffolding(
            builderOptions: o =>
            {
                o.SiteName = "Site 1";
                o.EnabledLanguages.Add(new CultureInfo("sr"));
                o.DefaultHost = HostUrl;
                o.DefaultLanguage = new CultureInfo("sr");
                o.BuildMode = BuildMode.OnlyIfEmptyInDefaultLanguage;
                o.StartPageType = typeof(StartPage);
                o.PublishContent = true;
                o.BlocksLocation = BlocksLocation.CurrentContent;
                o.Roles.Add(TestRole);
                o.Users.Add(new UserModel
                {
                    UserName = "TestUser",
                    Email = "email@test.com",
                    Password = TestUserPassword,
                    Roles = new[] { TestRole, Roles.WebEditors }
                });
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
