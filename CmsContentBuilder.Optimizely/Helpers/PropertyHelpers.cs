using CmsContentBuilder.Optimizely.Models;
using CmsContentBuilder.Shared.Resources;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
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

    public static ContentReference AddRandomImage<T>() where T : ImageData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();
        var options = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
        var site = GetSiteDefinition(options.DefaultLanguage);
        var image = contentRepository.GetDefault<T>(site != null ? site.GlobalAssetsRoot : ContentReference.GlobalBlockFolder);
        var blob = blobFactory.CreateBlob(image.BinaryDataContainer, ".png");

        blob.WriteAllBytes(ResourceHelpers.GetImage());
        image.BinaryData = blob;
        image.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";

        return contentRepository.Save(image, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
    }

    public static ContentArea AddBlock<T>(
        this ContentArea contentArea,
        Action<T>? blockOptions = null) where T : BlockData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var options = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
        var blockLocation = ContentReference.GlobalBlockFolder;

        switch (options.BlocksDefaultLocation)
        {
            case BlocksDefaultLocationEnum.CurrentPage:
                //TODO Discover current page
                break;
            case BlocksDefaultLocationEnum.GlobalBlockFolder:
                break;
            case BlocksDefaultLocationEnum.GlobalSiteFolder:
                blockLocation = ContentReference.SiteBlockFolder;
                break;
            default:
                break;
        }

        var block = contentRepository.GetDefault<T>(blockLocation, new CultureInfo(options.DefaultLanguage));
        InitContentAreas(block);
        blockOptions?.Invoke(block);

        var blockContent = (IContent)block;

        if (string.IsNullOrEmpty(blockContent.Name))
            blockContent.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";

        contentRepository.Save(blockContent, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

        contentArea.Items.Add(new ContentAreaItem
        {
            ContentLink = blockContent.ContentLink
        });

        return contentArea;
    }

    public static ContentArea AddBlocks<T>(
        this ContentArea contentArea,
        Action<T>? blockOptions = null,
        [Range(1, 10000)] int totalBlocks = 1) where T : BlockData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var options = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
        var blockLocation = ContentReference.GlobalBlockFolder;

        switch (options.BlocksDefaultLocation)
        {
            case BlocksDefaultLocationEnum.CurrentPage:
                //TODO Discover current page
                break;
            case BlocksDefaultLocationEnum.GlobalBlockFolder:
                break;
            case BlocksDefaultLocationEnum.GlobalSiteFolder:
                blockLocation = ContentReference.SiteBlockFolder;
                break;
            default:
                break;
        }

        T block;
        var blockTypeName = typeof(T).Name;

        for (int i = 0; i < totalBlocks; i++)
        {
            block = contentRepository.GetDefault<T>(blockLocation, new CultureInfo(options.DefaultLanguage));
            InitContentAreas(block);
            blockOptions?.Invoke(block);

            var blockContent = (IContent)block;
            blockContent.Name = string.IsNullOrEmpty(blockContent.Name) ? $"{blockTypeName}_{i}" : $"{blockContent.Name}_{i}";

            contentRepository.Save(blockContent, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

            contentArea.Items.Add(new ContentAreaItem
            {
                ContentLink = blockContent.ContentLink
            });
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

    public static void InitContentAreas<T>(T content)
        where T : ContentData
    {
        var contentAreaProperties = content.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType == typeof(ContentArea))
            .ToArray();

        if (contentAreaProperties.Length == 0)
            return;

        foreach (var contentArea in contentAreaProperties)
        {
            contentArea.SetValue(content, new ContentArea());
        }

        contentAreaProperties = null;
    }
}
