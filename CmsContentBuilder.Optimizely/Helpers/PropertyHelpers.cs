using CmsContentBuilder.Shared.Resources;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;

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

        image.BinaryData.WriteAllBytes(ResourceHelpers.GetImage());

        contentRepository.Save(image, SaveAction.Default, AccessLevel.NoAccess);

        return image;
    }

    public static ContentArea AddBlock<T>(this ContentArea contentArea, Action<T>? blockOptions = null)
        where T : BlockData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var block = contentRepository.GetDefault<T>(ContentReference.GlobalBlockFolder);

        blockOptions?.Invoke(block);

        var blockContent = (IContent)block;

        if (string.IsNullOrEmpty(blockContent.Name))
            blockContent.Name = $"{typeof(T).Name}_{Guid.NewGuid()}";

        contentRepository.Save(blockContent, SaveAction.Default, AccessLevel.NoAccess);

        contentArea.Items.Add(new ContentAreaItem
        {
            ContentLink = blockContent.ContentLink
        });

        return contentArea;
    }

    //public static void Add(this ImageField field, string imagePath)
    //{
    //    //TODO Upload image and get guid
    //    var media = new MediaFieldBase<ImageField>
    //    {
    //        Id = Guid.NewGuid()
    //    };

    //    field.Id = Guid.NewGuid();
    //}

    //public static IList<Block> Add<T>(this IList<Block> blocks, Action<T> blockOptions)
    //    where T : Block, new()
    //{
    //    var block = new T();
    //    blockOptions.Invoke(block);

    //    blocks.Add(block);

    //    return blocks;
    //}
}
