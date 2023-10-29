using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Builders;

internal class ContentBuilder : IContentBuilder
{
    private readonly ContentReference _parent;
    private readonly IContentRepository _contentRepository;
    private readonly IContentBuilderManager _contentBuilderManager;
    private readonly ContentBuilderOptions _options;

    public ContentBuilder(
        IContentRepository contentRepository,
        ContentReference parent,
        ContentBuilderOptions options,
        IContentBuilderManager contentBuilderManager)
    {
        _parent = parent;
        _contentRepository = contentRepository;
        _options = options;
        _contentBuilderManager = contentBuilderManager;
    }

    public IContentBuilder WithPage<T>(Action<IContentBuilder> options) where T : IContent
    {
        return WithPage<T>(default, options);
    }

    public IContentBuilder WithPage<T>(Action<T>? value = null, Action<IContentBuilder>? options = null) where T : IContent
    {
        var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent)
            ? _parent
            : _options.RootPage;
        var page = _contentRepository.GetDefault<T>(parent, _options.DefaultLanguage);

        PropertyHelpers.InitProperties(page);
        value?.Invoke(page);

        _contentBuilderManager.GetOrSetContentName<T>(page);

        if (!_contentRepository.GetChildren<T>(parent).Any(x => x.Name.Equals(page.Name)))
        {
            var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

            if (_options.StartPageType != null && _options.StartPageType.Equals(typeof(T)))
            {
                _contentBuilderManager.SetAsStartPage(pageRef);
            }

            var contentToMove = _contentRepository
                .GetChildren<IContentData>(_contentBuilderManager.GetOrCreateTempFolder(), _options.DefaultLanguage)
                .Cast<IContent>();

            foreach (var item in contentToMove)
            {
                _contentRepository.Move(item.ContentLink, pageRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
            }
        }

        if (options == null)
            return this;

        var builder = new ContentBuilder(_contentRepository, page.ContentLink, _options, _contentBuilderManager);
        options?.Invoke(builder);

        return this;
    }

    public IContentBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : IContent
    {
        return WithPages<T>(default, totalPages);
    }

    public IContentBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : IContent
    {
        if (totalPages < 1 || totalPages > 10000)
            throw new ArgumentOutOfRangeException(nameof(totalPages));

        T page;
        var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent)
            ? _parent
            : _options.RootPage;

        for (int i = 0; i < totalPages; i++)
        {
            page = _contentRepository.GetDefault<T>(parent, _options.DefaultLanguage);

            PropertyHelpers.InitProperties(page);
            value?.Invoke(page);

            _contentBuilderManager.GetOrSetContentName<T>(page, default, i.ToString());

            if (_contentRepository.GetChildren<T>(parent).Any(x => x.Name.Equals(page.Name)))
                continue;

            var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
            var contentToMove = _contentRepository
                .GetChildren<IContentData>(_contentBuilderManager.GetOrCreateTempFolder(), _options.DefaultLanguage)
                .Cast<IContent>();

            foreach (var item in contentToMove)
            {
                _contentRepository.Move(item.ContentLink, pageRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
            }
        }

        return this;
    }
}