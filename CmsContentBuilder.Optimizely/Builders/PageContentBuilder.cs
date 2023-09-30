using CmsContentBuilder.Optimizely.Extensions;
using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CmsContentBuilder.Optimizely.Builders;

public class PageContentBuilder : IPageContentBuilder
{
    private readonly PageData _parent;
    private readonly IContentRepository _contentRepository;
    private readonly CmsContentApplicationBuilderOptions _options;

    public PageContentBuilder(IContentRepository contentRepository, PageData parent, CmsContentApplicationBuilderOptions options)
    {
        _parent = parent;
        _contentRepository = contentRepository;
        _options = options;
    }

    public IPageContentBuilder WithSubPage<T>(
        Action<T>? value = null,
        Action<IPageContentBuilder>? options = null)
        where T : PageData
    {
        var page = _contentRepository.GetDefault<T>(_parent.ContentLink, new CultureInfo(_options.DefaultLanguage));
        page.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";
        var contentAreas = PropertyHelpers.InitContentAreas(page);
        value?.Invoke(page);

        var contentRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

        if (contentAreas.Any())
        {
            //TODO Check if there is a collection of blocks waiting to be created under that page
        }

        var pageContentBuilder = new PageContentBuilder(_contentRepository, page, _options);
        options?.Invoke(pageContentBuilder);

        return this;
    }

    public void WithSubPages<T>(
        Action<T>? value = null,
        [Range(1, 10000)] int totalPages = 1)
        where T : PageData
    {
        if (totalPages < 1 || totalPages > 10000)
            throw new ArgumentOutOfRangeException(nameof(totalPages));

        T page;
        var pageTypeName = typeof(T).Name;

        for (int i = 0; i < totalPages; i++)
        {
            page = _contentRepository.GetDefault<T>(_parent.ContentLink, new CultureInfo(_options.DefaultLanguage));
            var contentAreas = PropertyHelpers.InitContentAreas(page);
            value?.Invoke(page);

            page.Name = string.IsNullOrEmpty(page.Name) ? $"{pageTypeName}_{i}" : $"{page.Name}_{i}";
            var contentRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

            if (contentAreas.Any())
            {
                //TODO Check if there is a collection of blocks waiting to be created under that page
            }
        }
    }
}
