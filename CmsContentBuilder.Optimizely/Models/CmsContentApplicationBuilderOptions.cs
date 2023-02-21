using EPiServer.Core;

namespace CmsContentBuilder.Optimizely.Models;

public class CmsContentApplicationBuilderOptions
{
    public string DefaultLanguage { get; set; } = "en";
    public BuildModeEnum BuildMode { get; set; } = BuildModeEnum.Append;
    public PageReference RootPage { get; set; } = ContentReference.RootPage;
    public bool PublishContent { get; set; } = false;
    public BlocksDefaultLocationEnum BlocksDefaultLocation { get; set; } = BlocksDefaultLocationEnum.GlobalBlockFolder;
    public Type? StartPageType { get; set; }
}
