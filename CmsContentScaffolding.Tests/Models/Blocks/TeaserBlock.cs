using CmsContentScaffolding.Optimizely.Tests.Models.Blocks.Base;
using CmsContentScaffolding.Optimizely.Tests.Models.Blocks.Local;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Tests.Models.Blocks;

[ContentType(
    GUID = "{C98C99EA-A630-49CD-8A45-5AEF47EE265D}",
    DisplayName = "Teaser Block")]
public class TeaserBlock : BlockBase
{
    #region Content tab

    [CultureSpecific]
    [Display(
        Name = "Heading",
        GroupName = SystemTabNames.Content,
        Order = 100)]
    public virtual string Heading { get; set; }

    [CultureSpecific]
    [Display(
        GroupName = "Content",
        Order = 110)]
    [UIHint(UIHint.Textarea, PresentationLayer.Edit)]
    public virtual string LeadText { get; set; }

    [Display(
        GroupName = "Content",
        Order = 120)]
    public virtual LinkBlock LinkButton { get; set; }

    [CultureSpecific]
    [Display(
        GroupName = "Content",
        Order = 130)]
    [UIHint(UIHint.Image)]
    public virtual ContentReference Image { get; set; }

    #endregion
}
