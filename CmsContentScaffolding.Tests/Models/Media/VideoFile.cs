using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Tests.Models.Media;

[ContentType(GUID = "{EE890CD8-F26D-48A7-826B-72E393B3EEF8}")]
[MediaDescriptor(ExtensionString = "flv,mp4,webm")]
public class VideoFile : VideoData
{
    public virtual string? Copyright { get; set; }

    [UIHint(UIHint.Image)]
    public virtual ContentReference? PreviewImage { get; set; }
}
