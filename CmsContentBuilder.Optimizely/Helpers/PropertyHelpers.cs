using CmsContentBuilder.Optimizely.Startup;
using CmsContentBuilder.Shared.Resources;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System.Globalization;

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

    public static MediaData AddRandomImage()
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var image = contentRepository.GetDefault<ImageData>(ContentReference.GlobalBlockFolder);
        var options = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();

        image.BinaryData.WriteAllBytes(ResourceHelpers.GetImage());

        contentRepository.Save(image, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

        return image;
    }

    public static ContentArea AddBlock<T>(this ContentArea contentArea, Action<T>? blockOptions = null)
        where T : BlockData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var options = ServiceLocator.Current.GetInstance<CmsContentApplicationBuilderOptions>();
        var blockLocation = ContentReference.GlobalBlockFolder;

        switch (options.BlocksDefaultLocation)
        {
            case Models.BlocksDefaultLocationEnum.CurrentPage:
                //TODO Discover current page
                break;
            case Models.BlocksDefaultLocationEnum.GlobalBlockFolder:
                break;
            case Models.BlocksDefaultLocationEnum.GlobalSiteFolder:
                blockLocation = ContentReference.SiteBlockFolder;
                break;
            default:
                break;
        }

        var block = contentRepository.GetDefault<T>(blockLocation, new CultureInfo(options.DefaultLanguage));

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
}
