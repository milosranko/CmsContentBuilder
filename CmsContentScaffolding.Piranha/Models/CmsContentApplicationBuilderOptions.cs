namespace CmsContentScaffolding.Piranha.Models;

public class CmsContentApplicationBuilderOptions
{
    public BuildModeEnum BuildMode { get; set; } = BuildModeEnum.Append;
    public string DefaultLanguage { get; set; } = "en-US";
    public bool PublishContent { get; set; } = false;
}