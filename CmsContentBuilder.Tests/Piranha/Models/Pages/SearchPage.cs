using Piranha.AttributeBuilder;
using Piranha.Models;
using PiranhaCMS.PublicWeb.Models.Pages.Base;

namespace PiranhaCMS.PublicWeb.Models.Pages;

[PageType(Title = "Search Page", UseBlocks = false)]
[ContentTypeRoute(Title = "Default", Route = $"/{nameof(SearchPage)}")]
public class SearchPage : Page<SearchPage>, IPage
{ }
