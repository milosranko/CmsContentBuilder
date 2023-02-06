using CmsContentBuilder.Optimizely.Models;
using EPiServer.Core;

namespace CmsContentBuilder.Optimizely.Startup;

public class CmsContentApplicationBuilderOptions
{
    public string DefaultLanguage { get; set; } = "en";
    public BuildModeEnum BuildMode { get; set; } = BuildModeEnum.Append;
    public PageReference RootPage { get; set; } = ContentReference.RootPage;
    public BlocksDefaultLocationEnum BlocksDefaultLocation { get; set; } = BlocksDefaultLocationEnum.GlobalBlockFolder;
    //public bool CreateNewUser { get; set; } = true;
    //public UserSettings UserSettings { get; set; } = new UserSettings
    //{
    //    UserName = "cmscontentbuilder",
    //    UserEmail = "info@mdrsolutions.rs",
    //    Password = "9hDR=1h|i]K#,o8l",
    //    UserRole = "WebAdmins"
    //};
}

//public class UserSettings
//{
//    public string UserName { get; init; }
//    public string UserEmail { get; init; }
//    public string Password { get; init; }
//    public string UserRole { get; init; }
//}
