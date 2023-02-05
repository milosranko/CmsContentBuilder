using CmsContentBuilder.Optimizely.Models;
using EPiServer.Core;

namespace CmsContentBuilder.Optimizely.Startup;

public class CmsContentApplicationBuilderOptions
{
    public string DefaultLanguage { get; set; } = "en-US";
    public BuildModeEnum BuildMode { get; set; } = BuildModeEnum.Append;
    public PageReference RootPage { get; set; } = ContentReference.RootPage;
    public BlocksDefaultPlacementEnum BlocksDefaultPlacement { get; set; } = BlocksDefaultPlacementEnum.GlobalBlockFolder;
}