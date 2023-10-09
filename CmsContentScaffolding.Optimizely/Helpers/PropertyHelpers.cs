using CmsContentScaffolding.Optimizely.Models;
using CmsContentScaffolding.Shared.Resources;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

    public static ContentReference GetOrAddRandomImage<T>() where T : MediaData
    {
        var options = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var site = GetSiteDefinition(options.DefaultLanguage);
        var mediaFolder = site != null ? site.GlobalAssetsRoot : ContentReference.GlobalBlockFolder;
        var randomImage = ResourceHelpers.GetImage();
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var existingItems = contentRepository
            .GetChildren<T>(mediaFolder)
            .Where(x => x.Name.Equals(randomImage.Name, StringComparison.InvariantCultureIgnoreCase));

        if (existingItems != null && existingItems.Any())
        {
            return existingItems.ElementAt(0).ContentLink;
        }

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
        string folderName) where T : IContentData
    {
        return AddItem(contentArea, default, options, folderName);
    }

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        string? name = default,
        Action<T>? options = null,
        string? folderName = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var location = GetOrCreateBlockFolder(folderName, globalOptions);
        var content = contentRepository.GetDefault<T>(location, globalOptions.DefaultLanguage);

        InitContentAreas(content);

        options?.Invoke(content);

        var iContent = (IContent)content;

        if (string.IsNullOrEmpty(iContent.Name) && string.IsNullOrEmpty(name))
            iContent.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";
        else if (string.IsNullOrEmpty(iContent.Name))
            iContent.Name = name;

        if (!ContentReference.IsNullOrEmpty(iContent.ContentLink))
        {
            return AddItemToContentArea(contentArea, iContent.ContentLink);
        }

        contentRepository.Save(iContent, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

        return AddItemToContentArea(contentArea, iContent.ContentLink);
    }

    public static ContentReference GetOrCreateContent<T>(
        Action<T>? options = null,
        string? folderName = default) where T : IContent
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var parent = GetOrCreateBlockFolder(folderName, globalOptions);
        var content = contentRepository.GetDefault<T>(parent, globalOptions.DefaultLanguage);

        options?.Invoke(content);

        if (string.IsNullOrEmpty(content.Name))
            content.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";

        var foundContent = contentRepository
            .GetChildren<T>(content.ParentLink)
            .FirstOrDefault(x => x.Name.Equals(content.Name, StringComparison.InvariantCultureIgnoreCase));

        if (foundContent == null)
        {
            contentRepository.Save(content, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
            return content.ContentLink;
        }

        return foundContent.ContentLink;
    }

    public static ContentArea AddItems<T>(this ContentArea contentArea) where T : IContentData
    {
        return AddItems<T>(contentArea, default, default, default, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        string name) where T : IContentData
    {
        return AddItems<T>(contentArea, name, default, default, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        string name,
        Action<T> options) where T : IContentData
    {
        return AddItems(contentArea, name, options, default, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        [Range(1, 10000)] int totalBlocks) where T : IContentData
    {
        return AddItems<T>(contentArea, default, default, totalBlocks, default);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        Action<T> options,
        [Range(1, 10000)] int totalBlocks,
        string folderName) where T : IContentData
    {
        return AddItems(contentArea, default, options, totalBlocks, folderName);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        string? name = default,
        Action<T>? options = null,
        [Range(1, 10000)] int totalBlocks = 1,
        string? folderName = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var parent = GetOrCreateBlockFolder(folderName, globalOptions);
        T content;
        var typeName = typeof(T).Name;

        for (int i = 0; i < totalBlocks; i++)
        {
            content = contentRepository.GetDefault<T>(parent, globalOptions.DefaultLanguage);
            var totalContentAreas = InitContentAreas(content);
            options?.Invoke(content);

            var iContent = (IContent)content;
            if (string.IsNullOrEmpty(iContent.Name) && string.IsNullOrEmpty(name))
                iContent.Name = $"{typeName}_{i}";
            else
                iContent.Name = string.IsNullOrEmpty(name) ? $"{typeName}_{i}" : $"{iContent.Name}_{i}";

            if (!ContentReference.IsNullOrEmpty(iContent.ContentLink))
            {
                AddItemToContentArea(contentArea, iContent.ContentLink);
                continue;
            }

            contentRepository.Save(iContent, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

            AddItemToContentArea(contentArea, iContent.ContentLink);
        }

        return contentArea;
    }

    public static SiteDefinition? GetSiteDefinition(CultureInfo language)
    {
        var siteDefinitionRepository = ServiceLocator.Current.GetRequiredService<ISiteDefinitionRepository>();

        return siteDefinitionRepository
            .List()
            .Where(x => x.GetHosts(language, false).Any())
            .SingleOrDefault();
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

    private static ContentArea AddItemToContentArea(ContentArea contentArea, ContentReference contentReference)
    {
        contentArea.Items.Add(new ContentAreaItem
        {
            ContentLink = contentReference
        });

        return contentArea;
    }

    private static ContentReference GetOrCreateBlockFolder(
        string? folderName,
        ContentBuilderOptions options)
    {
        if (string.IsNullOrEmpty(folderName))
            return options.BlocksDefaultLocation switch
            {
                //TODO BlocksDefaultLocation.CurrentPage => Need to create blocks after the page is created
                //Temporarely store block data and page details so they can be created at the end of the process
                BlocksDefaultLocation.CurrentPage => ContentReference.GlobalBlockFolder,
                BlocksDefaultLocation.GlobalBlockFolder => ContentReference.GlobalBlockFolder,
                BlocksDefaultLocation.GlobalSiteFolder => ContentReference.SiteBlockFolder,
                _ => ContentReference.GlobalBlockFolder,
            };

        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var blockLocation = ContentReference.GlobalBlockFolder;

        if (!string.IsNullOrEmpty(folderName))
        {
            var foundFolder = contentRepository
                .GetChildren<ContentFolder>(blockLocation)
                .FirstOrDefault(x => x.Name.Equals(folderName, StringComparison.InvariantCultureIgnoreCase));

            if (foundFolder == null)
            {
                var folder = contentRepository.GetDefault<ContentFolder>(blockLocation, options.DefaultLanguage);
                folder.Name = folderName;
                contentRepository.Save(folder, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
                blockLocation = folder.ContentLink;
            }
            else
            {
                blockLocation = foundFolder.ContentLink;
            }
        }

        return blockLocation;
    }
}
