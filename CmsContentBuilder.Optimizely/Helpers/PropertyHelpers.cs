using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Shared.Resources;
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

namespace CmsContentBuilder.Optimizely.Extensions;

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

    public static ContentReference AddRandomImage<T>() where T : MediaData
    {
        var options = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
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

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        Action<T>? options = null,
        string? folderName = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
        var location = GetOrCreateBlockFolder(folderName, globalOptions);
        var content = contentRepository.GetDefault<T>(location, new CultureInfo(globalOptions.DefaultLanguage));

        InitContentAreas(content);

        options?.Invoke(content);

        var iContent = (IContent)content;

        if (string.IsNullOrEmpty(iContent.Name))
            iContent.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";

        if (!ContentReference.IsNullOrEmpty(iContent.ContentLink))
        {
            return AddItemToContentArea(contentArea, iContent.ContentLink);
        }

        contentRepository.Save(iContent, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

        return AddItemToContentArea(contentArea, iContent.ContentLink);
    }

    public static ContentReference GetOrCreateItem<T>(
        Action<T>? options = null,
        string? folderName = default) where T : IContent
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
        var parent = GetOrCreateBlockFolder(folderName, globalOptions);
        var content = contentRepository.GetDefault<T>(parent, new CultureInfo(globalOptions.DefaultLanguage));

        options?.Invoke(content);

        return GetOrCreateDefaultContent(content);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        Action<T>? options = null,
        [Range(1, 10000)] int totalBlocks = 1,
        string? folderName = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
        var parent = GetOrCreateBlockFolder(folderName, globalOptions);
        T content;
        var typeName = typeof(T).Name;

        for (int i = 0; i < totalBlocks; i++)
        {
            content = contentRepository.GetDefault<T>(parent, new CultureInfo(globalOptions.DefaultLanguage));
            var totalContentAreas = InitContentAreas(content);
            options?.Invoke(content);

            var iContent = (IContent)content;
            iContent.Name = string.IsNullOrEmpty(iContent.Name) ? $"{typeName}_{i}" : $"{iContent.Name}_{i}";

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

    public static SiteDefinition? GetSiteDefinition(string language)
    {
        var siteDefinitionRepository = ServiceLocator.Current.GetRequiredService<ISiteDefinitionRepository>();
        var culture = new CultureInfo(language);

        return siteDefinitionRepository
            .List()
            .Where(x => x.GetHosts(culture, false).Any())
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
        CmsContentApplicationBuilderOptions options)
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
                var folder = contentRepository.GetDefault<ContentFolder>(blockLocation, new CultureInfo(options.DefaultLanguage));
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

    private static ContentReference GetOrCreateDefaultContent<T>(
        T content) where T : IContent
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
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
}
