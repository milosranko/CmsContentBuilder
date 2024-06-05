using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Tests.Models.Media;

[ContentType(GUID = "{EE3BD195-7CB0-4756-AB5F-E5E223CD9820}")]
public class GenericMedia : MediaData
{
    public virtual string? Description { get; set; }
}
