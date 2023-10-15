using EPiServer.Core;
using System.Globalization;

namespace CmsContentScaffolding.Optimizely.Models;

public class ContentBuilderOptions
{
    public IList<CultureInfo> EnabledLanguages { get; set; } = new List<CultureInfo>
    {
        new CultureInfo("en")
    };
    public CultureInfo DefaultLanguage { get; set; } = new CultureInfo("en");
    public string DefaultHost { get; set; } = "http://localhost";
    public string SiteName { get; set; } = "Demo";
    public BuildMode BuildMode { get; set; } = BuildMode.Append;
    public PageReference RootPage { get; set; } = ContentReference.RootPage;
    public bool PublishContent { get; set; } = false;
    public BlocksLocation BlocksLocation { get; set; } = BlocksLocation.GlobalRoot;
    public Type? StartPageType { get; set; }
    public IList<string> Roles { get; set; } = new List<string> { EPiServer.Authorization.Roles.WebEditors, EPiServer.Authorization.Roles.WebAdmins };
    public IList<UserModel> Users { get; set; } = new List<UserModel>();
}
