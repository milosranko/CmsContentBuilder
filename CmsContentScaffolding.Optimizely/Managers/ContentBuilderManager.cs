using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Shell.Security;
using EPiServer.Web;

namespace CmsContentScaffolding.Optimizely.Managers;

internal class ContentBuilderManager : IContentBuilderManager
{
	private readonly ISiteDefinitionRepository _siteDefinitionRepository;
	private readonly IContentRepository _contentRepository;
	private readonly IContentSecurityRepository _contentSecurityRepository;
	private readonly IContentLoader _contentLoader;
	private readonly ILanguageBranchRepository _languageBranchRepository;
	private readonly IContentTypeRepository _contentTypeRepository;
	private readonly ContentAssetHelper _contentAssetHelper;
	private readonly UIRoleProvider _uIRoleProvider;
	private readonly UIUserProvider _uIUserProvider;
	private readonly ContentBuilderOptions _options;
	public ContentReference CurrentReference { get; set; }

	public ContentBuilderManager(
		ISiteDefinitionRepository siteDefinitionRepository,
		IContentRepository contentRepository,
		ContentBuilderOptions options,
		IContentLoader contentLoader,
		ILanguageBranchRepository languageBranchRepository,
		UIRoleProvider uIRoleProvider,
		UIUserProvider uIUserProvider,
		IContentSecurityRepository contentSecurityRepository,
		IContentTypeRepository contentTypeRepository,
		ContentAssetHelper contentAssetHelper)
	{
		_siteDefinitionRepository = siteDefinitionRepository;
		_contentRepository = contentRepository;
		_options = options;
		_contentLoader = contentLoader;
		_languageBranchRepository = languageBranchRepository;
		_uIRoleProvider = uIRoleProvider;
		_uIUserProvider = uIUserProvider;
		_contentSecurityRepository = contentSecurityRepository;
		_contentTypeRepository = contentTypeRepository;
		_contentAssetHelper = contentAssetHelper;
	}

	public SiteDefinition GetOrCreateSite()
	{
		var existingSite = _siteDefinitionRepository
			.List()
			.Where(x => x.Name.Equals(_options.SiteName))
			.SingleOrDefault();

		if (existingSite is not null)
			return existingSite;

		var siteUri = new Uri(_options.DefaultHost);
		var siteDefinition = new SiteDefinition
		{
			Name = _options.SiteName,
			StartPage = ContentReference.RootPage,
			SiteUrl = siteUri,
			Hosts = new List<HostDefinition>
			{
				new()
				{
					Name = siteUri.Authority,
					Language = _options.Language,
					Type = HostDefinitionType.Primary,
					UseSecureConnection = siteUri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)
				}
			}
		};

		_siteDefinitionRepository.Save(siteDefinition);

		return siteDefinition;
	}

	public void SetAsStartPage(ContentReference pageRef)
	{
		var site = GetOrCreateSite();

		if (!ContentReference.RootPage.CompareToIgnoreWorkID(site.StartPage) || site.StartPage.CompareToIgnoreWorkID(pageRef))
			return;

		var siteWritable = site.CreateWritableClone();
		siteWritable.StartPage = pageRef;
		siteWritable.SiteAssetsRoot = GetOrCreateSiteAssetsRoot(siteWritable);
		_siteDefinitionRepository.Save(siteWritable);

		if (_options.Roles is null || !_options.Roles.Any())
			return;

		if (_contentSecurityRepository.Get(siteWritable.StartPage).CreateWritableClone() is IContentSecurityDescriptor startPageSecurity)
		{
			if (startPageSecurity.IsInherited)
				startPageSecurity.ToLocal();

			foreach (var role in _options.Roles)
			{
				startPageSecurity.AddEntry(new AccessControlEntry(role.Key, role.Value, SecurityEntityType.Role));
			}

			_contentSecurityRepository.Save(startPageSecurity.ContentLink, startPageSecurity, SecuritySaveType.Replace);
		}
	}

	public bool IsInstallationEmpty()
	{
		var site = GetOrCreateSite();

		if (_options.BuildMode == BuildMode.OnlyIfEmptyInDefaultLanguage)
		{
			if (_languageBranchRepository.ListAll().Any(x => x.Culture.Equals(_options.Language)) && !ContentReference.RootPage.CompareToIgnoreWorkID(site.StartPage))
			{
				var pages = _contentLoader.GetChildren<IContentData>(site.StartPage, _options.Language);
				return pages is null || !pages.Any();
			}

			return true;
		}
		else if (_options.BuildMode.Equals(BuildMode.OnlyIfEmptyRegardlessOfLanguage))
		{
			var pages = _contentLoader.GetChildren<IContentData>(site.RootPage);
			return pages is null || !pages.Any();
		}
		return false;
	}

	public void ApplyDefaultLanguage()
	{
		var availableLanguages = _languageBranchRepository.ListAll();
		var svLang = availableLanguages.SingleOrDefault(x => x.LanguageID.Equals("sv"));

		if (svLang != null && !_options.Language.TwoLetterISOLanguageName.Equals("sv"))
		{
			_languageBranchRepository.Disable(svLang.Culture);
		}

		if (availableLanguages.Any(x => x.Culture.Equals(_options.Language)))
		{
			var existingLanguage = availableLanguages.Single(x => x.Culture.Equals(_options.Language));

			if (!existingLanguage.Enabled)
				_languageBranchRepository.Enable(existingLanguage.Culture);
		}
		else
		{
			var newLanguageBranch = new LanguageBranch(_options.Language);
			_languageBranchRepository.Save(newLanguageBranch);
			_languageBranchRepository.Enable(newLanguageBranch.Culture);
		}

		var rootPage = _contentLoader.Get<PageData>(ContentReference.RootPage);
		if (!rootPage.ExistingLanguages.Any(x => x.Equals(_options.Language)))
		{
			var rootPageClone = rootPage.CreateWritableClone();
			rootPageClone.ExistingLanguages.Append(_options.Language);
			_contentRepository.Save(rootPageClone, SaveAction.Default, AccessLevel.NoAccess);
		}
	}

	public void CreateDefaultRoles(IDictionary<string, AccessLevel> roles)
	{
		if (!roles.Any())
			return;

		var rootPageSecurity = _contentSecurityRepository.Get(ContentReference.RootPage).CreateWritableClone() as IContentSecurityDescriptor;

		foreach (var role in roles)
		{
			if (_uIRoleProvider.RoleExistsAsync(role.Key).GetAwaiter().GetResult())
				continue;

			_uIRoleProvider.CreateRoleAsync(role.Key).GetAwaiter().GetResult();

			if (rootPageSecurity == null || rootPageSecurity.Entries.Any(x => x.Name.Equals(role.Key)))
				continue;

			rootPageSecurity.AddEntry(new AccessControlEntry(role.Key, role.Value, SecurityEntityType.Role));
			_contentSecurityRepository.Save(rootPageSecurity.ContentLink, rootPageSecurity, SecuritySaveType.Replace);
		}
	}

	public void CreateRoles(IDictionary<string, AccessLevel>? roles)
	{
		if (roles is null || !roles.Any())
			return;

		foreach (var role in roles)
		{
			if (_uIRoleProvider.RoleExistsAsync(role.Key).GetAwaiter().GetResult())
				continue;

			_uIRoleProvider.CreateRoleAsync(role.Key).GetAwaiter().GetResult();
		}
	}

	public void CreateUsers(IEnumerable<UserModel>? users)
	{
		if (users is null || !users.Any())
			return;

		IUIUser? uiUser;

		foreach (var user in users)
		{
			uiUser = _uIUserProvider.GetUserAsync(user.UserName).GetAwaiter().GetResult();

			if (uiUser != null)
				continue;

			_uIUserProvider.CreateUserAsync(user.UserName, user.Password, user.Email, null, null, true).GetAwaiter().GetResult();

			if (user.Roles.Any())
			{
				_uIRoleProvider.AddUserToRolesAsync(user.UserName, user.Roles).GetAwaiter().GetResult();
			}
		}
	}

	public void GetOrSetContentName<T>(IContent content, string? name = default, string? nameSuffix = default) where T : IContentData
	{
		if (!string.IsNullOrEmpty(content.Name) && !content.Name.Equals(Constants.TempPageName, StringComparison.InvariantCultureIgnoreCase))
			return;

		if (!string.IsNullOrEmpty(name))
		{
			if (!string.IsNullOrEmpty(nameSuffix))
			{
				content.Name = $"{name} {nameSuffix}";
				return;
			}

			content.Name = name;
			return;
		}

		content.Name = $"{_contentTypeRepository.Load<T>().Name} {nameSuffix ?? Guid.NewGuid().ToString()}";
	}

	private ContentReference GetOrCreateSiteAssetsRoot(SiteDefinition site)
	{
		if (!site.SiteAssetsRoot.CompareToIgnoreWorkID(site.GlobalAssetsRoot))
			return site.SiteAssetsRoot;

		var siteRoot = _contentRepository.GetDefault<ContentFolder>(site.StartPage);
		siteRoot.Name = site.Name;

		return _contentRepository.Save(siteRoot, SaveAction.Publish, AccessLevel.NoAccess);
	}
}
