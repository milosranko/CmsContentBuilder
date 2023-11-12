using CmsContentScaffolding.Optimizely.Extensions;
using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Models;
using CmsContentScaffolding.Optimizely.Startup;
using CmsContentScaffolding.Shared.Resources;
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
		app.UseCmsContentScaffolding(
			builderOptions: o =>
			{
				o.SiteName = "Site 1";
				o.DefaultHost = Site1HostUrl;
				o.Language = new CultureInfo("sr");
				o.BuildMode = BuildMode.Append;
				o.StartPageType = typeof(StartPage);
				o.PublishContent = true;
				o.BlocksLocation = BlocksLocation.SiteRoot;
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
				b.UseAssets(ContentReference.GlobalBlockFolder)
				.WithFolder("Folder 1", l1 =>
				{
					l1
					.WithFolder("Folder 1_1", l2 =>
					{
						l2.WithBlock<TeaserBlock>("Teaser 2 Name", x => x.Heading = "Test");
					})
					.WithMedia<VideoFile>(x => x.Name = "Test video", ResourceHelpers.GetVideo(), ".mp4")
					.WithBlock<TeaserBlock>("Teaser Name", x => x.Heading = "Test");
				})
				.WithContent<ContentFolder>(x => x.Name = "Folder1")
				.WithContent<ImageFile>(x => x.Name = "Image 1")
				.WithBlock<TeaserBlock>("Teaser 1", x => x.Heading = "Test");

				b.UsePages(ContentReference.RootPage)
				.WithPage<StartPage>(p =>
				{
					p.Name = "Home Page";
					p.OpenGraphImage = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					p.MainContentArea
					.AddItems<TeaserBlock>("Teaser Test", b =>
					{
						b.Heading = ResourceHelpers.Faker.Lorem.Slug();
						b.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						b.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 2", ResourceHelpers.GetImage());
					},
					3,
					new AssetOptions { BlocksLocation = BlocksLocation.CurrentContent });
				}, l1 =>
				{
					l1
					.WithPage<ArticlePage>(p =>
					{
						p.Name = ResourceHelpers.Faker.Lorem.Slug();
						p.Heading = ResourceHelpers.Faker.Lorem.Slug();
						p.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						p.MainContent
						.AddStringFragment(ResourceHelpers.Faker.Lorem.Paragraphs())
						.AddContentFragment(PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage()))
						.AddStringFragment(ResourceHelpers.Faker.Lorem.Paragraphs());
						p.TopImage = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
						p.MainContentArea
						.AddItem<AccordionContainerBlock>("Accordion Container", b =>
						{
							b.Heading = ResourceHelpers.Faker.Lorem.Slug();
							b.Items.AddItems<AccordionItemBlock>("Accordion Item", b1 =>
							{
								b1.Heading = ResourceHelpers.Faker.Lorem.Slug();
								b1.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
								b1.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
							}, 5);
						}, new AssetOptions { BlocksLocation = BlocksLocation.GlobalRoot, FolderName = "Accordion" })
						.AddItem<ImageFile>(options: i =>
						{
							i.Name = "Test Image";
							i.ContentLink = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
						})
						.AddExistingItem<AccordionContainerBlock>("Accordion Container", new AssetOptions { BlocksLocation = BlocksLocation.GlobalRoot, FolderName = "Accordion" });
					}, l2 =>
					{
						l2
						.WithPage<ArticlePage>(p =>
						{
							p.Name = "Article2_1";
							p.Heading = ResourceHelpers.Faker.Lorem.Slug();
							p.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
							p.MainContent = new XhtmlString(ResourceHelpers.Faker.Lorem.Paragraphs());
						})
						.WithPage<ArticlePage>(l3 =>
						{
							l3.WithPages<ArticlePage>(p =>
							{
								p.Heading = ResourceHelpers.Faker.Lorem.Slug();
								p.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
								p.MainContent = new XhtmlString(ResourceHelpers.Faker.Lorem.Paragraphs());
							}, 20);
						});
					})
					.WithPages<ArticlePage>(p =>
					{
						p.Heading = ResourceHelpers.Faker.Lorem.Slug();
						p.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						p.MainContent = new XhtmlString(ResourceHelpers.Faker.Lorem.Paragraphs(10));
						p.MainContentArea.AddItem<TeaserBlock>(p.Name);
					}, 100);
				})
				.WithPage<NotFoundPage>(p =>
				{
					p.Name = "Not Found Page";
					p.Teaser.Heading = ResourceHelpers.Faker.Lorem.Slug(3);
					p.Teaser.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					p.Teaser.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
					p.Teaser.LinkButton.LinkText = ResourceHelpers.Faker.Internet.DomainName();
					p.Teaser.LinkButton.LinkUrl = new Url(ResourceHelpers.Faker.Internet.Url());
				})
				.WithPages<ArticlePage>(p =>
				{
					p.Name = ResourceHelpers.Faker.Lorem.Slug(2);
					p.MainContentArea.AddItems<TeaserBlock>(block =>
					{
						block.Heading = ResourceHelpers.Faker.Lorem.Slug();
						block.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						block.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					}, 10);
				}, 10)
				.WithPages<ArticlePage>(p =>
				{
					p.Name = ResourceHelpers.Faker.Lorem.Slug(3);
					p.MainContentArea.AddItems<TeaserBlock>(block =>
					{
						block.Heading = ResourceHelpers.Faker.Lorem.Slug();
						block.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						block.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					}, 2, new AssetOptions { BlocksLocation = BlocksLocation.GlobalRoot, FolderName = TeaserBlocksFolderName });
				}, 2);
			});

		app.UseCmsContentScaffolding(
			builderOptions: o =>
			{
				o.SiteName = "Site 2";
				o.DefaultHost = "https://localhost:5001";
				o.Language = new CultureInfo("en");
				o.BuildMode = BuildMode.OnlyIfEmptyInDefaultLanguage;
				o.StartPageType = typeof(StartPage);
				o.PublishContent = true;
				o.BlocksLocation = BlocksLocation.SiteRoot;
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
				b.UsePages()
				.WithPage<StartPage>(p =>
				{
					p.Name = "Home Page";
					p.OpenGraphImage = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					p.MainContentArea
					.AddItems<TeaserBlock>("Teaser Test", b =>
					{
						b.Heading = ResourceHelpers.Faker.Lorem.Slug();
						b.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						b.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					}, 3, new AssetOptions { BlocksLocation = BlocksLocation.CurrentContent });
				}, l1 =>
				{
					l1.WithPage<ArticlePage>();
				})
				.WithPage<NotFoundPage>(p =>
				{
					p.Name = "NotFoundPage";
					p.Teaser.Heading = ResourceHelpers.Faker.Lorem.Slug();
					p.Teaser.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					p.Teaser.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
					p.Teaser.LinkButton.LinkText = ResourceHelpers.Faker.Internet.DomainName();
					p.Teaser.LinkButton.LinkUrl = new Url(ResourceHelpers.Faker.Internet.Url());
				})
				.WithPages<ArticlePage>(p =>
				{
					p.Name = "Article2";
					p.MainContentArea.AddItems<TeaserBlock>(block =>
					{
						block.Heading = ResourceHelpers.Faker.Lorem.Slug();
						block.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						block.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					}, 10, new AssetOptions { BlocksLocation = BlocksLocation.CurrentContent });
				}, 10)
				.WithPages<ArticlePage>(p =>
				{
					p.Name = "Articles4";
					p.MainContentArea.AddItems<TeaserBlock>(block =>
					{
						block.Heading = ResourceHelpers.Faker.Lorem.Slug();
						block.LeadText = ResourceHelpers.Faker.Lorem.Paragraph();
						block.Image = PropertyHelpers.GetOrAddImage<ImageFile>("Image 1", ResourceHelpers.GetImage());
					}, 2, new AssetOptions { FolderName = TeaserBlocksFolderName });
				}, 2);
			});

		return app;
	}
}
