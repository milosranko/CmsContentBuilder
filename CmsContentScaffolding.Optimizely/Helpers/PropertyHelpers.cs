using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using CmsContentScaffolding.Shared.Resources;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace CmsContentScaffolding.Optimizely.Helpers;

public static class PropertyHelpers
{
    public static string AddRandomText(int maxLength = 50)
    {
        return ResourceHelpers.GetText().Substring(0, maxLength);
    }

    public static XhtmlString AddRandomHtml()
    {
        return new XhtmlString(ResourceHelpers.GetHtmlText());
    }

    public static ContentReference GetOrAddRandomImage<T>(int width = 1200, int height = 800) where T : MediaData
    {
        var options = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();
        var site = contentBuilderManager.GetOrCreateSite();
        var mediaFolder = ContentReference.IsNullOrEmpty(site.SiteAssetsRoot) ? site.GlobalAssetsRoot : site.SiteAssetsRoot;
        var randomImage = ResourceHelpers.GetImage();
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var existingItems = contentRepository
            .GetChildren<T>(mediaFolder)
            .Where(x => x.Name.Equals(randomImage.Name, StringComparison.InvariantCultureIgnoreCase));

        if (existingItems != null && existingItems.Any())
            return existingItems.ElementAt(0).ContentLink;

        var blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();
        var image = contentRepository.GetDefault<T>(mediaFolder);
        var blob = blobFactory.CreateBlob(image.BinaryDataContainer, ".png");

        blob.WriteAllBytes(randomImage.Bytes);
        image.BinaryData = blob;
        image.Name = randomImage.Name;

        return contentRepository.Save(image, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
    }

    public static IEnumerable<ContentArea> InitContentAreas<T>(T content)
        where T : IContentData
    {
        var contentAreaProperties = content.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType.Equals(typeof(ContentArea)))
            .ToArray();

        if (contentAreaProperties.Length == 0)
            return Enumerable.Empty<ContentArea>();

        var contentAreas = new List<ContentArea>(contentAreaProperties.Length);

        foreach (var contentArea in contentAreaProperties)
        {
            contentArea.SetValue(content, new ContentArea());
            contentAreas.Add((ContentArea)contentArea.GetValue(content));
        }

        contentAreaProperties = null;

        return contentAreas;
    }

    public static void InitXHtmlStringProperties<T>(T content)
        where T : IContentData
    {
        var xhtmlStringProperties = content.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType.Equals(typeof(XhtmlString)))
            .ToArray();

        if (xhtmlStringProperties.Length == 0)
            return;

        foreach (var xhtmlString in xhtmlStringProperties)
        {
            xhtmlString.SetValue(content, new XhtmlString());
        }
    }
}
