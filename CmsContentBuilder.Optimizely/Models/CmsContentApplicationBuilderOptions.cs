using EPiServer.Core;

namespace CmsContentBuilder.Optimizely.Models;

public class ContentBuilderOptions
{
    public string DefaultLanguage { get; set; } = "en";
    public string DefaultHost { get; set; } = "http://localhost";
    public BuildMode BuildMode { get; set; } = BuildMode.Append;
    public PageReference RootPage { get; set; } = ContentReference.RootPage;
    public bool PublishContent { get; set; } = false;
    public BlocksDefaultLocation BlocksDefaultLocation { get; set; } = BlocksDefaultLocation.GlobalBlockFolder;
    public Type? StartPageType { get; set; }
}
