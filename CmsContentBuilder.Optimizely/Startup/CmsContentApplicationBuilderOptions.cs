using CmsContentBuilder.Optimizely.Models;

namespace CmsContentBuilder.Optimizely.Startup;

public class CmsContentApplicationBuilderOptions
{
    public string DefaultLanguage { get; set; } = "en-US";
    public BuildModeEnum BuildMode { get; set; } = BuildModeEnum.Append;
}