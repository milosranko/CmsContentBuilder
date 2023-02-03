using Piranha.AttributeBuilder;
using Piranha.Extend;
using Piranha.Models;
using PiranhaCMS.PublicWeb.Models.Pages.Base;
using PiranhaCMS.PublicWeb.Models.Regions;

namespace PiranhaCMS.PublicWeb.Models.Pages;

[PageType(Title = "404 Not Found Page", UseBlocks = false)]
[ContentTypeRoute(Title = "Default", Route = $"/{nameof(NotFoundPage)}")]
public class NotFoundPage : Page<NotFoundPage>, IPage
{
    [Region(
        Title = "Main Content",
        Display = RegionDisplayMode.Content,
        Description = "Main content properties")]
    public ArticlePageRegion PageRegion { get; set; }
}
