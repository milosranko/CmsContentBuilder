using CmsContentBuilder.Optimizely.Interfaces;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;

namespace CmsContentBuilder.Optimizely.Builders;

public class PageContentBuilder : IPageContentBuilder
{
    private readonly PageData _parent;
    private readonly IContentRepository _contentRepository;

    public PageContentBuilder(IContentRepository contentRepository, PageData parent)
    {
        _parent = parent;
        _contentRepository = contentRepository;
    }

    public void WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null)
        where T : PageData
    {
        var page = _contentRepository.GetDefault<T>(_parent.ContentLink);
        value?.Invoke(page);

        if (string.IsNullOrEmpty(page.Name))
        {
            page.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";
        }

        _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);

        if (options == null) return;

        var pageContentBuilder = new PageContentBuilder(_contentRepository, page);
        options.Invoke(pageContentBuilder);
    }

    public void WithSubPages<T>(Action<T>? value = null, int totalPages = 1)
        where T : PageData
    {
        if (totalPages < 1 || totalPages > 10000)
            throw new ArgumentOutOfRangeException(nameof(totalPages));

        T page;

        for (int i = 0; i < totalPages; i++)
        {
            page = _contentRepository.GetDefault<T>(_parent.ContentLink);
            value?.Invoke(page);

            if (string.IsNullOrEmpty(page.Name))
            {
                page.Name = $"{typeof(T).Name}_{i}";
            }
            else
            {
                page.Name = $"{page.Name}_{i}";
            }

            _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);
        }
    }
}
