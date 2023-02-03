using Piranha.AttributeBuilder;
using Piranha.Models;
using PiranhaCMS.PublicWeb.Models.Pages.Base;

namespace PiranhaCMS.PublicWeb.Models.Pages;

[PageType(Title = "Start Page", UseBlocks = true)]
[ContentTypeRoute(Title = "Default", Route = $"/{nameof(StartPage)}")]
public class StartPage : Page<StartPage>, IPage
{ }
