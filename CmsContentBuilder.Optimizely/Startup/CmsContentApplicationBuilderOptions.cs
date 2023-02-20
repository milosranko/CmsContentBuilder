using CmsContentBuilder.Optimizely.Models;
using EPiServer.Core;

namespace CmsContentBuilder.Optimizely.Startup;

public class CmsContentApplicationBuilderOptions
{
    public string DefaultLanguage { get; set; } = "en";
    public BuildModeEnum BuildMode { get; set; } = BuildModeEnum.Append;
    public PageReference RootPage { get; set; } = ContentReference.RootPage;
    public bool PublishContent { get; set; } = false;
    public BlocksDefaultLocationEnum BlocksDefaultLocation { get; set; } = BlocksDefaultLocationEnum.GlobalBlockFolder;
}
