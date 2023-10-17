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
        var site = GetOrCreateSite();
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
        AssetOptions? assetOptions = null) where T : IContentData
    {
        return AddItem(contentArea, default, options, assetOptions);
    }

    public static ContentArea AddItem<T>(
        this ContentArea contentArea,
        string? name = default,
        Action<T>? options = null,
        AssetOptions? assetOptions = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var folder = GetOrCreateBlockFolder(assetOptions, globalOptions);
        var content = contentRepository.GetDefault<T>(folder, globalOptions.DefaultLanguage);
        var contentAreas = InitContentAreas(content);

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
        AssetOptions assetOptions) where T : IContentData
    {
        return AddItems(contentArea, default, options, totalBlocks, assetOptions);
    }

    public static ContentArea AddItems<T>(
        this ContentArea contentArea,
        string? name = default,
        Action<T>? options = null,
        [Range(1, 10000)] int totalBlocks = 1,
        AssetOptions? assetOptions = default) where T : IContentData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var parent = GetOrCreateBlockFolder(assetOptions, globalOptions);

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
        AssetOptions? assetOptions,
        ContentBuilderOptions options)
    {
        var site = GetOrCreateSite();

        if (assetOptions == null)
            return options.BlocksLocation switch
            {
                BlocksLocation.CurrentContent => GetOrCreateTempFolder(),
                BlocksLocation.GlobalRoot => ContentReference.GlobalBlockFolder,
                BlocksLocation.SiteRoot => ContentReference.IsNullOrEmpty(site.SiteAssetsRoot) ? site.GlobalAssetsRoot : site.SiteAssetsRoot,
                _ => ContentReference.GlobalBlockFolder,
            };

        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var blockLocation = assetOptions.BlocksLocation switch
        {
            BlocksLocation.CurrentContent => GetOrCreateTempFolder(),
            BlocksLocation.GlobalRoot => ContentReference.GlobalBlockFolder,
            BlocksLocation.SiteRoot => ContentReference.IsNullOrEmpty(site.SiteAssetsRoot) ? site.GlobalAssetsRoot : site.SiteAssetsRoot,
            _ => ContentReference.GlobalBlockFolder,
        };

        if (!string.IsNullOrEmpty(assetOptions.FolderName) && blockLocation != ContentReference.EmptyReference)
        {
            var existingFolder = contentRepository
                .GetChildren<ContentFolder>(blockLocation)
                .FirstOrDefault(x => x.Name.Equals(assetOptions.FolderName, StringComparison.InvariantCultureIgnoreCase));

            if (existingFolder == null)
            {
                var folder = contentRepository.GetDefault<ContentFolder>(blockLocation, options.DefaultLanguage);
                folder.Name = assetOptions.FolderName;
                contentRepository.Save(folder, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
                blockLocation = folder.ContentLink;
            }
            else
            {
                blockLocation = existingFolder.ContentLink;
            }
        }

        if (ContentReference.IsNullOrEmpty(blockLocation))
        {
            return GetOrCreateTempFolder();
        }

        return blockLocation;
    }

    private const string tempFolderName = "Temp";

    public static ContentReference GetOrCreateTempFolder()
    {
        var options = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var existingFolder = contentRepository
            .GetChildren<ContentFolder>(ContentReference.GlobalBlockFolder, options.DefaultLanguage)
            .FirstOrDefault(x => x.Name.Equals(tempFolderName));

        if (existingFolder != null)
            return existingFolder.ContentLink;

        var folder = contentRepository.GetDefault<ContentFolder>(ContentReference.GlobalBlockFolder, options.DefaultLanguage);
        folder.Name = tempFolderName;

        return contentRepository.Save(folder, SaveAction.Default, AccessLevel.NoAccess);
    }

    public static SiteDefinition GetOrCreateSite()
    {
        var options = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
        var siteDefinitionRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
        var existingSite = siteDefinitionRepository
            .List()
            .Where(x => x.Name.Equals(options.SiteName))
            .SingleOrDefault();

        if (existingSite is not null)
            return existingSite;

        var siteUri = new Uri(options.DefaultHost);
        var siteDefinition = new SiteDefinition
        {
            Name = options.SiteName,
            StartPage = ContentReference.RootPage,
            SiteUrl = siteUri,
            Hosts = new List<HostDefinition>
            {
                new HostDefinition
                {
                    Name = siteUri.Authority,
                    Language = options.DefaultLanguage,
                    Type = HostDefinitionType.Primary,
                    UseSecureConnection = siteUri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)
                }
            }
        };

        siteDefinitionRepository.Save(siteDefinition);

        return siteDefinition;
    }
}
