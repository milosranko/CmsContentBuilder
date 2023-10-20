using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using CmsContentScaffolding.Shared.Resources;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Core.Internal;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CmsContentScaffolding.Optimizely.Extensions;

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

    public static ContentArea AddItem<T>(this ContentArea contentArea) where T : IContentData
    {
        return AddItem<T>(contentArea, default, default, default);
    }

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        string name) where T : IContentData
    {
        return AddItem<T>(contentArea, name, default, default);
    }

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        Action<T> options) where T : IContentData
    {
        return AddItem(contentArea, default, options, default);
    }

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        string name,
        Action<T> options) where T : IContentData
    {
        return AddItem(contentArea, name, options, default);
    }

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        Action<T> options,
        AssetOptions? assetOptions = default) where T : IContentData
    {
        return AddItem(contentArea, default, options, assetOptions);
    }

    public static ContentArea AddExistingItem<T>(
        this ContentArea contentArea,
        string name,
        AssetOptions? assetOptions = default,
        IEnumerable<string>? allowedRoles = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();
        var folder = contentBuilderManager.GetOrCreateBlockFolder(assetOptions);
        var content = contentRepository
            .GetChildren<T>(folder, globalOptions.DefaultLanguage)
            .Cast<IContent>()
            .FirstOrDefault(x => x.Name.Equals(name));

        return content != null
            ? AddItemToContentArea(contentArea, content.ContentLink)
            : contentArea;
    }

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        string? name = default,
        Action<T>? options = default,
        AssetOptions? assetOptions = default,
        IEnumerable<string>? allowedRoles = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();
        var folder = contentBuilderManager.GetOrCreateBlockFolder(assetOptions);
        var content = contentRepository.GetDefault<T>(folder, globalOptions.DefaultLanguage);

        InitContentAreas(content);
        InitXHtmlStringProperties(content);

        options?.Invoke(content);

        var iContent = (IContent)content;

        if (string.IsNullOrEmpty(iContent.Name) && string.IsNullOrEmpty(name))
            iContent.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";
        else if (string.IsNullOrEmpty(iContent.Name))
            iContent.Name = name;

        if (!ContentReference.IsNullOrEmpty(iContent.ContentLink))
            return AddItemToContentArea(contentArea, iContent.ContentLink);

        var contentRef = contentRepository.Save(iContent, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
        //var contentToMove = contentRepository.GetChildren<IContent>(GetOrCreateTempFolder(globalOptions), globalOptions.DefaultLanguage);
        //foreach (var item in contentToMove.Where(x => x.ContentLink != contentRef))
        //{
        //    contentRepository.Move(item.ContentLink, contentRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
        //}

        return AddItemToContentArea(contentArea, iContent.ContentLink);
    }

    public static ContentArea AddItems<T>(this ContentArea contentArea) where T : IContentData
    {
        return AddItems<T>(contentArea, default, default, default, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        string name,
        [Range(1, 10000)] int total) where T : IContentData
    {
        return AddItems<T>(contentArea, name, default, total, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        Action<T> options,
        [Range(1, 10000)] int total) where T : IContentData
    {
        return AddItems(contentArea, default, options, total, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        string name,
        Action<T> options,
        [Range(1, 10000)] int total) where T : IContentData
    {
        return AddItems(contentArea, name, options, total, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        [Range(1, 10000)] int total) where T : IContentData
    {
        return AddItems<T>(contentArea, default, default, total, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        Action<T> options,
        [Range(1, 10000)] int total,
        AssetOptions assetOptions) where T : IContentData
    {
        return AddItems(contentArea, default, options, total, assetOptions);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        string? name = default,
        Action<T>? options = default,
        [Range(1, 10000)] int total = 1,
        AssetOptions? assetOptions = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();
        var parent = contentBuilderManager.GetOrCreateBlockFolder(assetOptions);

        T content;
        var typeName = typeof(T).Name;

        for (int i = 0; i < total; i++)
        {
            content = contentRepository.GetDefault<T>(parent, globalOptions.DefaultLanguage);

            InitContentAreas(content);
            InitXHtmlStringProperties(content);

            options?.Invoke(content);

            var iContent = (IContent)content;
            if (string.IsNullOrEmpty(iContent.Name) && string.IsNullOrEmpty(name))
                iContent.Name = $"{typeName}_{i}";
            else
                iContent.Name = string.IsNullOrEmpty(name) ? $"{typeName}_{i}" : $"{name}_{i}";

            if (!ContentReference.IsNullOrEmpty(iContent.ContentLink))
            {
                AddItemToContentArea(contentArea, iContent.ContentLink);
                continue;
            }

            var contentRef = contentRepository.Save(iContent, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
            AddItemToContentArea(contentArea, iContent.ContentLink);
        }

        return contentArea;
    }

    public static XhtmlString AddStringFragment(this XhtmlString xhtmlString, string text)
    {
        xhtmlString.Fragments.Add(new StaticFragment(text));

        return xhtmlString;
    }

    public static XhtmlString AddContentFragment(this XhtmlString xhtmlString, ContentReference contentReference)
    {
        var contentFragmentFactory = ServiceLocator.Current.GetInstance<ContentFragmentFactory>();
        var fragment = contentFragmentFactory.CreateContentFragment(contentReference, Guid.Empty, null);

        xhtmlString.Fragments.Add(fragment);

        return xhtmlString;
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

    private static ContentArea AddItemToContentArea(ContentArea contentArea, ContentReference contentReference)
    {
        contentArea.Items.Add(new ContentAreaItem
        {
            ContentLink = contentReference
        });

        return contentArea;
    }
}
