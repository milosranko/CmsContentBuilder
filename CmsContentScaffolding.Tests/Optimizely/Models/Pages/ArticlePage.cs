using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using Optimizely.Demo.PublicWeb.Models.Blocks;
using Optimizely.Demo.PublicWeb.Models.Pages.Base;
using System.ComponentModel.DataAnnotations;

namespace Optimizely.Demo.PublicWeb.Models.Pages;

[ContentType(
    GUID = "{28C6817F-9819-4114-9E04-CEB03FD919F7}",
    GroupName = "Default")]
[AvailableContentTypes(
    Availability.Specific,
    Include = new[]
    {
        typeof(ArticlePage)
    })]
public class ArticlePage : PageBaseSeo
{
    #region Content tab

    [CultureSpecific]
    [Display(
        GroupName = SystemTabNames.Content,
        Order = 10)]
    public virtual string Heading { get; set; }

    [CultureSpecific]
    [Display(
        GroupName = SystemTabNames.Content,
        Order = 20)]
    [UIHint(UIHint.Image)]
    public virtual ContentReference TopImage { get; set; }

    [CultureSpecific]
    [Display(
    GroupName = SystemTabNames.Content,
        Order = 30)]
    [UIHint(UIHint.Textarea, PresentationLayer.Edit)]
    public virtual string LeadText { get; set; }

    [CultureSpecific]
    [Display(
        GroupName = SystemTabNames.Content,
        Order = 40)]
    public virtual XhtmlString MainContent { get; set; }

    [CultureSpecific]
    [Display(
        GroupName = SystemTabNames.Content,
        Order = 50)]
    [AllowedTypes(new[] {
        typeof(TeaserBlock),
        typeof(AccordionContainerBlock),
        typeof(ImageData)
    })]
    public virtual ContentArea MainContentArea { get; set; }

    #endregion

    #region Settings

    [CultureSpecific]
    [Display(
        GroupName = SystemTabNames.Settings,
        Order = 100)]
    [ClientEditor(
        ClientEditingClass = "dijit/form/HorizontalSlider",
        DefaultValue = "0",
        EditorConfiguration = "{'minimum': 0, 'maximum': 100, 'discreteValues': 101}")]
    public virtual int Importance { get; set; }

    #endregion
}
