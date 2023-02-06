using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Startup;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
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

    public void WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null)
        where T : PageData
    {
        var page = _contentRepository.GetDefault<T>(_parent.ContentLink, new CultureInfo(_options.DefaultLanguage));
        value?.Invoke(page);

        if (string.IsNullOrEmpty(page.Name))
        {
            page.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";
        }

        _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);

        if (options == null) return;

        var pageContentBuilder = new PageContentBuilder(_contentRepository, page, _options);
        options.Invoke(pageContentBuilder);

        page = null;
        pageContentBuilder = null;
    }

    public void WithSubPages<T>(Action<T>? value = null, int totalPages = 1)
        where T : PageData
    {
        if (totalPages < 1 || totalPages > 10000)
            throw new ArgumentOutOfRangeException(nameof(totalPages));

        T page;
        var pageTypeName = typeof(T).Name;

        for (int i = 0; i < totalPages; i++)
        {
            page = _contentRepository.GetDefault<T>(_parent.ContentLink, new CultureInfo(_options.DefaultLanguage));
            value?.Invoke(page);

            page.Name = string.IsNullOrEmpty(page.Name) ? $"{pageTypeName}_{i}" : $"{page.Name}_{i}";
            _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);
        }

        page = null;
    }
}
