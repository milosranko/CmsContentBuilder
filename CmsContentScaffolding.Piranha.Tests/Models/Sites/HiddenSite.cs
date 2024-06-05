using Piranha.AttributeBuilder;
using Piranha.Models;

namespace PiranhaCMS.PublicWeb.Models.Sites;

[SiteType(Title = "Hidden site")]
public class HiddenSite : SiteContent<HiddenSite>
{ }