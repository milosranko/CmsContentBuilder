using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace PiranhaCMS.PublicWeb.Models.Regions;

public class LinkButton
{
    [Field(
        Title = "Link Text",
        Placeholder = "Enter link text",
        Description = "Link text")]
    public StringField LinkName { get; set; }

    [Field(
        Title = "Page Reference",
        Options = FieldOption.HalfWidth,
        Description = "Please select page reference")]
    public PageField Page { get; set; }

    [Field(
        Title = "External URL Address",
        Options = FieldOption.HalfWidth,
        Description = "Enter full external URL (including http...)")]
    public StringField ExternalUrl { get; set; }

    [Field(
        Title = "Open Link In New Window",
        Options = FieldOption.HalfWidth,
        Description = "Check if you want link to be opened in a new window/tab")]
    public CheckBoxField OpenInNewWindow { get; set; }
}
