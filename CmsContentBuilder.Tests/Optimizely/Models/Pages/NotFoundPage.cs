using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Optimizely.Demo.PublicWeb.Models.Pages.Base;

namespace Optimizely.Demo.PublicWeb.Models.Pages;

[ContentType(
    GUID = "{005DBD8E-B8C6-4C0F-8066-44CFE01FD535}",
    GroupName = "MetaData")]
[AvailableContentTypes(
    Availability.None)]
public class NotFoundPage : PageBasePublic
{
    #region Content tab

    #endregion

    #region Public properties

    public override void SetDefaultValues(ContentType contentType)
    {
        base.SetDefaultValues(contentType);

        VisibleInMenu = false;
    }

    #endregion
}
