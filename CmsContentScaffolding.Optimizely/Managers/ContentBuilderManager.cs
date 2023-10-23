using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.DataAnnotations;
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
    private readonly UIRoleProvider _uIRoleProvider;
    private readonly UIUserProvider _uIUserProvider;
    private readonly ContentBuilderOptions _options;
    private const string TempFolderName = "Temp";
    private const string TempFolderGuid = "F2EDE111-2E2E-4D0E-A2B9-DE4BB87984BA";

    public ContentBuilderManager(
        ISiteDefinitionRepository siteDefinitionRepository,
        IContentRepository contentRepository,
        ContentBuilderOptions options,
        IContentLoader contentLoader,
        ILanguageBranchRepository languageBranchRepository,
        UIRoleProvider uIRoleProvider,
        UIUserProvider uIUserProvider,
        IContentSecurityRepository contentSecurityRepository)
    {
        _siteDefinitionRepository = siteDefinitionRepository;
        _contentRepository = contentRepository;
        _options = options;
        _contentLoader = contentLoader;
        _languageBranchRepository = languageBranchRepository;
        _uIRoleProvider = uIRoleProvider;
        _uIUserProvider = uIUserProvider;
        _contentSecurityRepository = contentSecurityRepository;
    }

    public ContentReference GetOrCreateBlockFolder(AssetOptions? assetOptions)
    {
        var site = GetOrCreateSite();

        if (assetOptions == null)
            return _options.BlocksLocation switch
            {
                BlocksLocation.CurrentContent => GetOrCreateTempFolder(),
                BlocksLocation.GlobalRoot => ContentReference.GlobalBlockFolder,
                BlocksLocation.SiteRoot => ContentReference.IsNullOrEmpty(site.SiteAssetsRoot) ? site.GlobalAssetsRoot : site.SiteAssetsRoot,
                _ => ContentReference.GlobalBlockFolder,
            };

        var blockLocation = assetOptions.BlocksLocation switch
        {
            BlocksLocation.CurrentContent => GetOrCreateTempFolder(),
            BlocksLocation.GlobalRoot => ContentReference.GlobalBlockFolder,
            BlocksLocation.SiteRoot => ContentReference.IsNullOrEmpty(site.SiteAssetsRoot) ? site.GlobalAssetsRoot : site.SiteAssetsRoot,
            _ => ContentReference.GlobalBlockFolder,
        };

        if (!string.IsNullOrEmpty(assetOptions.FolderName) && blockLocation != ContentReference.EmptyReference)
        {
            var existingFolder = _contentRepository
                .GetChildren<ContentFolder>(blockLocation)
                .FirstOrDefault(x => x.Name.Equals(assetOptions.FolderName, StringComparison.InvariantCultureIgnoreCase));

            if (existingFolder == null)
            {
                var folder = _contentRepository.GetDefault<ContentFolder>(blockLocation, _options.DefaultLanguage);
                folder.Name = assetOptions.FolderName;
                _contentRepository.Save(folder, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
                blockLocation = folder.ContentLink;
            }
            else
            {
                blockLocation = existingFolder.ContentLink;
            }
        }

        if (ContentReference.IsNullOrEmpty(blockLocation))
        {
            return GetOrCreateTempFolder();
        }

        return blockLocation;
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
                new HostDefinition
                {
                    Name = siteUri.Authority,
                    Language = _options.DefaultLanguage,
                    Type = HostDefinitionType.Primary,
                    UseSecureConnection = siteUri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)
                }
            }
        };

        _siteDefinitionRepository.Save(siteDefinition);

        return siteDefinition;
    }

    public ContentReference GetOrCreateTempFolder()
    {
        var existingFolder = _contentRepository
            .GetChildren<ContentFolder>(ContentReference.GlobalBlockFolder, _options.DefaultLanguage)
            .FirstOrDefault(x => x.Name.Equals(TempFolderName));

        if (existingFolder != null)
            return existingFolder.ContentLink;

        var folder = _contentRepository.GetDefault<ContentFolder>(ContentReference.GlobalBlockFolder, _options.DefaultLanguage);
        folder.Name = TempFolderName;
        folder.ContentGuid = new Guid(TempFolderGuid);

        return _contentRepository.Save(folder, SaveAction.Default, AccessLevel.NoAccess);
    }

    public void DeleteTempFolder()
    {
        var tempFolder = GetOrCreateTempFolder();
        _contentRepository.Delete(tempFolder, false, AccessLevel.NoAccess);
    }

    public void SetAsStartPage(ContentReference pageRef)
    {
        var site = GetOrCreateSite();

        if (ContentReference.RootPage != site.StartPage)
            return;

        var updateSite = site.CreateWritableClone();
        updateSite.StartPage = pageRef;
        _siteDefinitionRepository.Save(updateSite);

        if (_options.Roles is null || !_options.Roles.Any())
            return;

        if (_contentSecurityRepository.Get(updateSite.StartPage).CreateWritableClone() is IContentSecurityDescriptor startPageSecurity)
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
            if (_languageBranchRepository.ListAll().Any(x => x.Culture.Equals(_options.DefaultLanguage)) && !(ContentReference.RootPage == site.StartPage))
            {
                var pages = _contentLoader.GetChildren<IContentData>(site.StartPage, _options.DefaultLanguage);

                return pages is null || pages.Count().Equals(0);
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

        if (svLang != null)
        {
            _languageBranchRepository.Disable(svLang.Culture);
        }

        foreach (var lang in _options.EnabledLanguages)
        {
            if (availableLanguages.Any(x => x.Culture.Equals(lang)))
            {
                var existingLanguage = availableLanguages.Single(x => x.Culture.Equals(lang));

                if (!existingLanguage.Enabled)
                    _languageBranchRepository.Enable(existingLanguage.Culture);
            }
            else
            {
                var newLanguageBranch = new LanguageBranch(lang);
                _languageBranchRepository.Save(newLanguageBranch);
                _languageBranchRepository.Enable(newLanguageBranch.Culture);
            }
        }

        var rootPage = _contentLoader.Get<PageData>(_options.RootPage);
        if (!rootPage.ExistingLanguages.Any(x => x.Equals(_options.DefaultLanguage)))
        {
            var rootPageClone = rootPage.CreateWritableClone();
            rootPageClone.ExistingLanguages.Append(_options.DefaultLanguage);
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
        if (!string.IsNullOrEmpty(content.Name))
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

        var type = typeof(T);
        var displayName = type
            .GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .Cast<ContentTypeAttribute>()
            .FirstOrDefault()?.DisplayName;

        if (!string.IsNullOrEmpty(displayName))
        {
            content.Name = $"{displayName} {nameSuffix ?? Guid.NewGuid().ToString()}";
            return;
        }

        content.Name = $"{type.Name} {nameSuffix ?? Guid.NewGuid().ToString()}";
    }
}
