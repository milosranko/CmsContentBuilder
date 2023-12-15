using CmsContentScaffolding.Optimizely.Models;
using EPiServer.Core;
using EPiServer.Security;

namespace CmsContentScaffolding.Optimizely.Interfaces;

internal interface IContentBuilderManager
{
	bool SiteExists { get; }
	ContentReference CurrentReference { get; set; }
	void SetOrCreateSiteContext();
	void SetStartPageSecurity(ContentReference pageRef);
	void ApplyDefaultLanguage();
	void CreateDefaultRoles(IDictionary<string, AccessLevel> roles);
	void CreateRoles(IDictionary<string, AccessLevel>? roles);
	void CreateUsers(IEnumerable<UserModel>? users);
	void SetContentName<T>(IContent content, string? name = default, string? nameSuffix = default) where T : IContentData;
	ContentReference CreateItem<T>(string? name = default, string? suffix = default, Action<T>? options = default) where T : IContentData;
}
