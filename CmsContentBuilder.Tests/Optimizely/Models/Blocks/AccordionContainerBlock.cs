using EPiServer.Core;
using EPiServer.DataAnnotations;
using Optimizely.Demo.PublicWeb.Models.Blocks.Base;
using System.ComponentModel.DataAnnotations;

namespace Optimizely.Demo.PublicWeb.Models.Blocks;

[ContentType(GUID = "{2AB06B13-1082-4FB2-A9E0-BAE99983BEBF}")]
public class AccordionContainerBlock : BlockBase
{
    #region Content tab

    [CultureSpecific]
    [Display(
        GroupName = "Heading",
        Order = 100)]
    public virtual string Heading { get; set; }

    [CultureSpecific]
    [Display(
        GroupName = "Items",
        Order = 110)]
    [AllowedTypes(typeof(AccordionItemBlock))]
    public virtual ContentArea Items { get; set; }

    #endregion
}
