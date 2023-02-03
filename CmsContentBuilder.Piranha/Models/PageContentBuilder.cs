using CmsContentBuilder.Piranha.Interfaces;
using Piranha;
using Piranha.Models;

namespace CmsContentBuilder.Piranha.Models;

public class PageContentBuilder : IPageContentBuilder
{
    private readonly IApi _api;
    private readonly PageBase _parent;

    public PageContentBuilder(IApi api, PageBase parent)
    {
        _api = api;
        _parent = parent;
    }

    public void WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null)
        where T : Page<T>
    {
        var page = Page<T>.CreateAsync(_api).GetAwaiter().GetResult();
        value?.Invoke(page);

        page.ParentId = _parent.Id;
        page.SiteId = _parent.SiteId;

        if (string.IsNullOrEmpty(page.Title))
        {
            page.Title = $"{typeof(T).Name}_{Guid.NewGuid()}";
        }

        _api.Pages.SaveAsync(page).GetAwaiter().GetResult();

        if (options == null) return;

        var pageContentBuilder = new PageContentBuilder(_api, page);
        options.Invoke(pageContentBuilder);
    }

    public void WithSubPages<T>(Action<T>? value = null, int totalPages = 1)
        where T : Page<T>
    {
        if (totalPages < 1 || totalPages > 10000)
            throw new ArgumentOutOfRangeException(nameof(totalPages));

        T page;

        for (int i = 0; i < totalPages; i++)
        {
            page = Page<T>.CreateAsync(_api).GetAwaiter().GetResult();
            value?.Invoke(page);

            if (string.IsNullOrEmpty(page.Title))
            {
                page.Title = $"{typeof(T).Name}_{i}";
            }
            else
            {
                page.Title = $"{page.Title}_{i}";
            }

            page.ParentId = _parent.Id;
            page.SiteId = _parent.SiteId;

            _api.Pages.SaveAsync(page).GetAwaiter().GetResult();
        }
    }
}
