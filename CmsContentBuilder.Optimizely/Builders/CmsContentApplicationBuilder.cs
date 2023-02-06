using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Startup;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using System.Globalization;
using System.Reflection;

namespace CmsContentBuilder.Optimizely.Builders;

public class CmsContentApplicationBuilder : ICmsContentApplicationBuilder
{
    private readonly IContentRepository _contentRepository;
    private readonly CmsContentApplicationBuilderOptions _options;

    public CmsContentApplicationBuilder(IContentRepository contentRepository, CmsContentApplicationBuilderOptions options)
    {
        _contentRepository = contentRepository;
        _options = options;
    }

    public void WithPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null)
        where T : PageData
    {
        var page = _contentRepository.GetDefault<T>(_options.RootPage, new CultureInfo(_options.DefaultLanguage));

        InitContentAreas(page);

        value?.Invoke(page);

        if (string.IsNullOrEmpty(page.Name))
        {
            page.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";
        }

        _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);

        if (options == null) return;

        var pageContentBuilder = new PageContentBuilder(_contentRepository, page, _options);
        options?.Invoke(pageContentBuilder);
    }

    public void WithPages<T>(Action<T>? value = null, int totalPages = 1)
        where T : PageData
    {
        if (totalPages < 1 || totalPages > 10000)
            throw new ArgumentOutOfRangeException(nameof(totalPages));

        var page = _contentRepository.GetDefault<T>(_options.RootPage, new CultureInfo(_options.DefaultLanguage));
        value?.Invoke(page);
        var pageName = string.IsNullOrEmpty(page.Name) ? typeof(T).Name : page.Name;

        for (int i = 0; i < totalPages; i++)
        {
            page.Name = $"{pageName}_{i}";
            _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);
        }
    }

    private void InitContentAreas<T>(T page) where T : PageData
    {
        var contentAreaProperties = page.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType == typeof(ContentArea))
            .ToArray();

        if (contentAreaProperties.Length == 0)
            return;

        foreach (var contentArea in contentAreaProperties)
        {
            contentArea.SetValue(page, new ContentArea());
        }
    }
}