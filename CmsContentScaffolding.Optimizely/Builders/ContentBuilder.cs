using CmsContentScaffolding.Optimizely.Extensions;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.DataAnnotations;
using EPiServer.Security;
using EPiServer.Web;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Builders;

public class ContentBuilder : IContentBuilder
{
    private readonly PageData _parent;
    private readonly ISiteDefinitionRepository _siteDefinitionRepository;
    private readonly IContentRepository _contentRepository;
    private readonly ContentBuilderOptions _options;

    public ContentBuilder(
        IContentRepository contentRepository,
        PageData parent,
        ContentBuilderOptions options,
        ISiteDefinitionRepository siteDefinitionRepository)
    {
        _parent = parent;
        _contentRepository = contentRepository;
        _options = options;
        _siteDefinitionRepository = siteDefinitionRepository;
    }

    public IContentBuilder WithPage<T>(Action<IContentBuilder> options) where T : PageData
    {
        return WithPage<T>(default, options);
    }

    public IContentBuilder WithPage<T>(Action<T>? value = null, Action<IContentBuilder>? options = null) where T : PageData
    {
        var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent.ContentLink)
            ? _parent.ContentLink
            : _options.RootPage;
        var page = _contentRepository.GetDefault<T>(parent, _options.DefaultLanguage);
        var contentAreas = PropertyHelpers.InitContentAreas(page);

        value?.Invoke(page);

        GetOrSetPageName<T>(page);

        var existingPage = _contentRepository.GetChildren<T>(parent).SingleOrDefault(x => x.Name.Equals(page.Name));
        if (existingPage is null)
        {
            var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

            if (_options.StartPageType != null && _options.StartPageType.Equals(typeof(T)))
            {
                SetAsStartPage(pageRef);
            }

            var contentToMove = _contentRepository.GetChildren<IContent>(PropertyHelpers.GetOrCreateTempFolder(), _options.DefaultLanguage);
            foreach (var item in contentToMove)
            {
                _contentRepository.Move(item.ContentLink, pageRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
            }
        }

        if (options == null)
            return this;

        var builder = new ContentBuilder(_contentRepository, page, _options, _siteDefinitionRepository);
        options?.Invoke(builder);

        return this;
    }

    public IContentBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : PageData
    {
        return WithPages<T>(default, totalPages);
    }

    public IContentBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData
    {
        if (totalPages < 1 || totalPages > 10000)
            throw new ArgumentOutOfRangeException(nameof(totalPages));

        T page;
        var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent.ContentLink)
            ? _parent.ContentLink
            : _options.RootPage;

        for (int i = 0; i < totalPages; i++)
        {
            page = _contentRepository.GetDefault<T>(parent, _options.DefaultLanguage);
            var contentAreas = PropertyHelpers.InitContentAreas(page);
            value?.Invoke(page);

            GetOrSetPageName<T>(page, i.ToString());

            var existingPage = _contentRepository.GetChildren<T>(parent).SingleOrDefault(x => x.Name.Equals(page.Name));
            if (existingPage is null)
            {
                var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

                var contentToMove = _contentRepository.GetChildren<IContent>(PropertyHelpers.GetOrCreateTempFolder(), _options.DefaultLanguage);
                foreach (var item in contentToMove)
                {
                    _contentRepository.Move(item.ContentLink, pageRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
                }
            }
        }

        return this;
    }

    private void GetOrSetPageName<T>(PageData page, string? nameSuffix = default) where T : PageData
    {
        if (!string.IsNullOrEmpty(page.Name))
            return;

        var type = typeof(T);
        var displayName = type
            .GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .Cast<ContentTypeAttribute>()
            .FirstOrDefault()?.DisplayName;

        if (!string.IsNullOrEmpty(displayName))
        {
            page.Name = $"{displayName} {nameSuffix ?? Guid.NewGuid().ToString()}";
            return;
        }

        page.Name = $"{type.Name} {nameSuffix ?? Guid.NewGuid().ToString()}";
    }

    private void SetAsStartPage(ContentReference pageRef)
    {
        var site = PropertyHelpers.GetOrCreateSite();

        if (ContentReference.RootPage == site.StartPage)
        {
            var updateSite = site.CreateWritableClone();
            updateSite.StartPage = pageRef;
            _siteDefinitionRepository.Save(updateSite);
        }
    }
}