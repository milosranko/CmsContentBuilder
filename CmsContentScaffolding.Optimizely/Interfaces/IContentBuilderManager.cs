using CmsContentScaffolding.Optimizely.Models;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.Web;

namespace CmsContentScaffolding.Optimizely.Interfaces;

internal interface IContentBuilderManager
{
    SiteDefinition GetOrCreateSite();
    ContentReference GetOrCreateTempFolder();
    void DeleteTempFolder();
    ContentReference GetOrCreateBlockFolder(AssetOptions? assetOptions);
    void SetAsStartPage(ContentReference pageRef);
    bool IsInstallationEmpty();
    void ApplyDefaultLanguage();
    void CreateDefaultRoles(IDictionary<string, AccessLevel> roles);
    void CreateRoles(IDictionary<string, AccessLevel>? roles);
    void CreateUsers(IEnumerable<UserModel>? users);
    void GetOrSetContentName<T>(IContent content, string? name = default, string? nameSuffix = default) where T : IContentData;
}
