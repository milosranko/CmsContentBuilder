using EPiServer.Core;

namespace CmsContentScaffolding.Optimizely.Tests.Models.Media.Base;

public abstract class ImageFileBase : ImageData
{
    public virtual string? Copyright { get; set; }
    public virtual string? AlternateText { get; set; }
}
