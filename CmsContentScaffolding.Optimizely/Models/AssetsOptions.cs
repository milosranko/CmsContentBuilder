using EPiServer.Core;

namespace CmsContentScaffolding.Optimizely.Models;

public class AssetOptions
{
    public BlocksLocation? BlocksLocation { get; set; } = Models.BlocksLocation.SiteRoot;
    public string? FolderName { get; set; } = default;
    public ContentReference? Parent { get; set; } = default;
}
