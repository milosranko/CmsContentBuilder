using Piranha.AttributeBuilder;
using Piranha.Extend;
using Piranha.Models;
using PiranhaCMS.PublicWeb.Models.Pages.Base;
using PiranhaCMS.PublicWeb.Models.Regions;

namespace PiranhaCMS.PublicWeb.Models.Pages;

[PageType(Title = "Article Page", UseBlocks = true)]
[ContentTypeRoute(Title = "Default", Route = $"/{nameof(ArticlePage)}")]
public class ArticlePage : Page<ArticlePage>, IPage
{
    [Region(
        Title = "Main Content",
        Display = RegionDisplayMode.Content,
        Description = "Main content properties")]
    public ArticlePageRegion PageRegion { get; set; }
}
