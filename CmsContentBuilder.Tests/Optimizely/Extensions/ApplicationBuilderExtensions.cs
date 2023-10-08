using CmsContentBuilder.Optimizely.Extensions;
using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Optimizely.Startup;
using EPiServer;
using EPiServer.Core;
using Microsoft.AspNetCore.Builder;
using Optimizely.Demo.PublicWeb.Models.Blocks;
using Optimizely.Demo.PublicWeb.Models.Media;
using Optimizely.Demo.PublicWeb.Models.Pages;
using static CmsContentBuilder.Tests.Optimizely.Constants.StringConstants;

namespace CmsContentBuilder.Tests.Optimizely.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder ConfigureCmsContentBuilder(this IApplicationBuilder app)
    {
        app.UseCmsContentBuilder(
            builderOptions: o =>
            {
                o.DefaultHost = HostUrl;
                o.DefaultLanguage = Language;
                o.BuildMode = BuildMode.OnlyIfEmptyInDefaultLanguage;
                o.RootPage = ContentReference.RootPage;
                o.StartPageType = typeof(StartPage);
                o.PublishContent = true;
                o.BlocksDefaultLocation = BlocksDefaultLocation.CurrentPage;
                //o.Roles.Add("rolename");
                //o.Users.Add("username", "email@test.com", "password", Roles.WebEditors)
            },
            builder: b =>
            {
                b
                .WithPage<StartPage>(p =>
                {
                    p.Name = "Home Page";
                    p.OpenGraphImage = PropertyHelpers.AddRandomImage<ImageFile>();
                    p.MainContentArea
                    .AddItems<TeaserBlock>(b =>
                    {
                        b.Heading = PropertyHelpers.AddRandomText();
                        b.LeadText = PropertyHelpers.AddRandomText(150);
                        b.Image = PropertyHelpers.AddRandomImage<ImageFile>();
                    }, "Teaser Test", 3, p.Name);
                }, l1 =>
                {
                    l1
                    .WithPage<ArticlePage>(p =>
                    {
                        p.Name = "Article1_1";
                        p.Heading = PropertyHelpers.AddRandomText();
                        p.LeadText = PropertyHelpers.AddRandomText(150);
                        p.MainContent = PropertyHelpers.AddRandomHtml();
                        p.TopImage = PropertyHelpers.AddRandomImage<ImageFile>();
                        p.MainContentArea
                        .AddItem<AccordionContainerBlock>(b =>
                        {
                            b.Heading = PropertyHelpers.AddRandomText();
                            b.Items.AddItems<AccordionItemBlock>(b1 =>
                            {
                                b1.Heading = PropertyHelpers.AddRandomText();
                                b1.Image = PropertyHelpers.AddRandomImage<ImageFile>();
                                b1.LeadText = PropertyHelpers.AddRandomText(105);
                            }, "Accordion Item", 5);
                        }, "Accordion Container")
                        .AddItem<ImageFile>(i =>
                        {
                            i.Name = "Test Image";
                            i.ContentLink = PropertyHelpers.AddRandomImage<ImageFile>();
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
                        .WithPage<ArticlePage>(options: l3 =>
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
                        p.MainContentArea.AddItem<TeaserBlock>(null, p.Name);
                    }, 100);
                })
                .WithPage<NotFoundPage>(p =>
                {
                    p.Name = "NotFoundPage";
                    p.Teaser.Heading = PropertyHelpers.AddRandomText(20);
                    p.Teaser.Image = PropertyHelpers.AddRandomImage<ImageFile>();
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
                        block.Image = PropertyHelpers.AddRandomImage<ImageFile>();
                    }, null, 10, TeaserBlocksFolderName);
                }, 10);
            });

        return app;
    }
}
