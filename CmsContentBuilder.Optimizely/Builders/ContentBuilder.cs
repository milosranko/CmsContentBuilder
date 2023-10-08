using CmsContentBuilder.Optimizely.Extensions;
using CmsContentBuilder.Optimizely.Interfaces;
using CmsContentBuilder.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace CmsContentBuilder.Optimizely.Builders;

public class ContentBuilder : IContentBuilder
{
    private readonly PageData _parent;
    private readonly IContentRepository _contentRepository;
    private readonly ContentBuilderOptions _options;

    public ContentBuilder(IContentRepository contentRepository, PageData parent, ContentBuilderOptions options)
    {
        _parent = parent;
        _contentRepository = contentRepository;
        _options = options;
    }

    public IContentBuilder WithPage<T>(
        Action<T>? value = null,
        Action<IContentBuilder>? options = null)
        where T : PageData
    {
        var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent.ContentLink)
            ? _parent.ContentLink
            : _options.RootPage;
        var page = _contentRepository.GetDefault<T>(parent, _options.DefaultLanguage);
        var contentAreas = PropertyHelpers.InitContentAreas(page);
        value?.Invoke(page);

        if (string.IsNullOrEmpty(page.Name))
        {
            page.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";
        }

        var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

        if (_options.StartPageType != null &&
            _options.StartPageType.Equals(typeof(T)) &&
            ContentReference.IsNullOrEmpty(ContentReference.StartPage))
        {
            SetAsStartPage(pageRef);
        }

        if (contentAreas.Any())
        {
            //TODO Check if there is a collection of blocks waiting to be created under that page
        }

        if (options == null)
            return this;

        var builder = new ContentBuilder(_contentRepository, page, _options);
        options?.Invoke(builder);

        return this;
    }

    public void WithPages<T>(
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
            page = _contentRepository.GetDefault<T>(_options.RootPage, _options.DefaultLanguage);
            var contentAreas = PropertyHelpers.InitContentAreas(page);
            value?.Invoke(page);

            page.Name = string.IsNullOrEmpty(page.Name) ? $"{pageTypeName}_{i}" : $"{page.Name}_{i}";
            _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

            if (contentAreas.Any())
            {
                //TODO Check if there is a collection of blocks waiting to be created under that page
            }
        }
    }

    private void SetAsStartPage(ContentReference pageRef)
    {
        var siteDefinitionRepository = ServiceLocator.Current.GetRequiredService<ISiteDefinitionRepository>();
        var siteUri = new Uri(_options.DefaultHost);

        if (PropertyHelpers.GetSiteDefinition(_options.DefaultLanguage) != null)
            return;

        var siteDefinition = new SiteDefinition
        {
            Name = "Demo",
            StartPage = pageRef,
            Id = Guid.NewGuid(),
            SiteUrl = siteUri,
            Hosts = new List<HostDefinition>
            {
                new HostDefinition
                {
                    Name = siteUri.Authority,
                    Language = _options.DefaultLanguage,
                    Type = HostDefinitionType.Undefined,
                    UseSecureConnection = siteUri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)
                }
            }
        };

        siteDefinitionRepository.Save(siteDefinition);
    }
}