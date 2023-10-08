using EPiServer.Core;
using System.Globalization;

namespace CmsContentBuilder.Optimizely.Models;

public class ContentBuilderOptions
{
    public IList<CultureInfo> EnabledLanguages { get; set; } = new List<CultureInfo>
    {
        new CultureInfo("en")
    };
    public CultureInfo DefaultLanguage { get; set; } = new CultureInfo("en");
    public string DefaultHost { get; set; } = "http://localhost";
    public BuildMode BuildMode { get; set; } = BuildMode.Append;
    public PageReference RootPage { get; set; } = ContentReference.RootPage;
    public bool PublishContent { get; set; } = false;
    public BlocksDefaultLocation BlocksDefaultLocation { get; set; } = BlocksDefaultLocation.GlobalBlockFolder;
    public Type? StartPageType { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<UserModel> Users { get; set; } = new List<UserModel>();
}
