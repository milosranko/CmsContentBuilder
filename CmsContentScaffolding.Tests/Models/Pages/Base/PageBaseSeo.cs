using CmsContentScaffolding.Optimizely.Tests.Models.Blocks.Local;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Tests.Models.Pages.Base;

public abstract class PageBaseSeo : PageBasePublic
{
    [Display(
    GroupName = "MetaData",
        Order = 5030)]
    [CultureSpecific]
    public virtual string MetaTitle { get; set; }

    [Display(
    GroupName = "MetaData",
        Order = 5040)]
    [CultureSpecific]
    public virtual bool MetaNoRobots { get; set; }

    [Display(
        GroupName = "MetaData",
        Order = 5050)]
    [CultureSpecific]
    [UIHint(UIHint.Textarea)]
    public virtual string MetaDescription { get; set; }

    [Display(
    GroupName = "MetaData",
        Order = 5100)]
    [UIHint(UIHint.Textarea)]
    public virtual string OpenGraphDescription { get; set; }

    [Display(
    GroupName = "MetaData",
        Order = 5110)]
    [UIHint(UIHint.Image)]
    public virtual ContentReference OpenGraphImage { get; set; }

    [Display(
        GroupName = "SEO",
        Order = 5120)]
    public virtual SitemapSettingsBlock SitemapSettings { get; set; }
}
