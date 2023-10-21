using CmsContentScaffolding.Optimizely.Models;
using EPiServer.Core;
using EPiServer.Web;

namespace CmsContentScaffolding.Optimizely.Interfaces;

public interface IContentBuilderManager
{
    SiteDefinition GetOrCreateSite();
    ContentReference GetOrCreateTempFolder();
    void DeleteTempFolder();
    ContentReference GetOrCreateBlockFolder(AssetOptions? assetOptions);
    void SetAsStartPage(ContentReference pageRef);
    bool IsInstallationEmpty();
    void ApplyDefaultLanguage();
    void CreateRoles(IEnumerable<string> roles);
    void CreateUsers(IEnumerable<UserModel> users);
}
