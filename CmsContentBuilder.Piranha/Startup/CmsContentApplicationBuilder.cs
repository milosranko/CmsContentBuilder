using CmsContentBuilder.Piranha.Interfaces;
using CmsContentBuilder.Piranha.Models;
using Piranha;
using Piranha.Models;

namespace CmsContentBuilder.Piranha.Startup;

public class CmsContentApplicationBuilder : ICmsContentApplicationBuilder
{
    private readonly IApi _api;
    private readonly Site _site;
    public string DefaultLanguage { get; set; } = "en-US";
    public BuildModeEnum BuildMode { get; set; } = BuildModeEnum.Overwrite;

    public CmsContentApplicationBuilder(IApi api)
    {
        _api = api;
        _site = _api.Sites.GetDefaultAsync().GetAwaiter().GetResult();
    }

    public void WithSite<T>(Action<T>? value = null)
        where T : SiteContent<T>
    {
        var availableLanguages = _api.Languages.GetAllAsync().GetAwaiter().GetResult();
        var languageId = _api.Languages.GetDefaultAsync().GetAwaiter().GetResult().Id;

        if (!availableLanguages.Any(x => x.Culture.Equals(DefaultLanguage, StringComparison.InvariantCultureIgnoreCase)))
        {
            var newLanguage = new Language
            {
                Culture = DefaultLanguage,
                Id = Guid.NewGuid(),
                IsDefault = true,
                Title = DefaultLanguage
            };

            _api.Languages.SaveAsync(newLanguage).GetAwaiter().GetResult();
            languageId = newLanguage.Id;
        }

        var defaultSite = _api.Sites.GetDefaultAsync().GetAwaiter().GetResult();
        defaultSite.SiteTypeId = typeof(T).Name;
        defaultSite.LanguageId = languageId;
        _api.Sites.SaveAsync(defaultSite).GetAwaiter().GetResult();

        var site = SiteContent<T>.CreateAsync(_api).GetAwaiter().GetResult();

        value?.Invoke(site);

        _api.Sites.SaveContentAsync(defaultSite.Id, site).GetAwaiter().GetResult();
    }

    public void WithPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null)
        where T : Page<T>
    {
        var page = Page<T>.CreateAsync(_api).GetAwaiter().GetResult();

        value?.Invoke(page);

        if (string.IsNullOrEmpty(page.Title))
        {
            page.Title = $"{typeof(T).Name}_{Guid.NewGuid()}";
        }

        page.SiteId = _site.Id;
        _api.Pages.SaveAsync(page).GetAwaiter().GetResult();

        if (options == null) return;

        var pageContentBuilder = new PageContentBuilder(_api, page);
        options?.Invoke(pageContentBuilder);
    }

    public void WithPages<T>(Action<T>? value = null, int totalPages = 1)
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

            page.SiteId = _site.Id;

            _api.Pages.SaveAsync(page).GetAwaiter().GetResult();
        }
    }
}