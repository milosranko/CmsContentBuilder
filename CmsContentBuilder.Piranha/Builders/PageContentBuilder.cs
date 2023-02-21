using CmsContentBuilder.Piranha.Interfaces;
using CmsContentBuilder.Piranha.Models;
using Piranha;
using Piranha.Models;
using System.ComponentModel.DataAnnotations;

namespace CmsContentBuilder.Piranha.Builders;

public class PageContentBuilder : IPageContentBuilder
{
    private readonly IApi _api;
    private readonly PageBase _parent;
    private readonly CmsContentApplicationBuilderOptions _options;

    public PageContentBuilder(IApi api, PageBase parent, CmsContentApplicationBuilderOptions options)
    {
        _api = api;
        _parent = parent;
        _options = options;
    }

    public IPageContentBuilder WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null)
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

        if (_options.PublishContent)
            _api.Pages.SaveAsync(page).GetAwaiter().GetResult();
        else
            _api.Pages.SaveDraftAsync(page).GetAwaiter().GetResult();

        //if (options == null) return;

        var pageContentBuilder = new PageContentBuilder(_api, page, _options);
        options?.Invoke(pageContentBuilder);

        return pageContentBuilder;
    }

    public void WithSubPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1)
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

            if (_options.PublishContent)
                _api.Pages.SaveAsync(page).GetAwaiter().GetResult();
            else
                _api.Pages.SaveDraftAsync(page).GetAwaiter().GetResult();
        }
    }
}
