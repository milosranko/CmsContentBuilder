using EPiServer.Core;

namespace CmsContentScaffolding.Optimizely.Models;

public class AssetOptions
{
    public BlocksLocation? BlocksLocation { get; set; }
    public string? FolderName { get; set; }
    public ContentReference? Parent { get; set; }
}
