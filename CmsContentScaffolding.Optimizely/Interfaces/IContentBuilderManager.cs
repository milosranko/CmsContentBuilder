using CmsContentScaffolding.Optimizely.Models;
using EPiServer.Core;
using EPiServer.Web;

namespace CmsContentScaffolding.Optimizely.Interfaces;

public interface IContentBuilderManager
{
    SiteDefinition GetOrCreateSite();
    ContentReference GetOrCreateTempFolder();
    ContentReference GetOrCreateBlockFolder(AssetOptions? assetOptions);
    ContentReference GetOrAddRandomImage<T>(int width = 1200, int height = 800) where T : MediaData;
    void SetAsStartPage(ContentReference pageRef);
    bool IsInstallationEmpty();
    void ApplyDefaultLanguage();
    void CreateRoles(IEnumerable<string> roles);
    void CreateUsers(IEnumerable<UserModel> users);
}
