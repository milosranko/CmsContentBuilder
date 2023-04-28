using CmsContentBuilder.Shared.Resources.Extensions;

namespace CmsContentBuilder.Shared.Resources;

public static class ResourceHelpers
{
    public static string GetText()
    {
        using var stream = typeof(ResourceHelpers).Assembly.GetManifestResourceStream("CmsContentBuilder.Shared.Resources.Texts.LoremIpsum.txt");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static string GetHtmlText()
    {
        using var stream = typeof(ResourceHelpers).Assembly.GetManifestResourceStream("CmsContentBuilder.Shared.Resources.Texts.LoremIpsumHtml.txt");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static (string Name, byte[] Bytes) GetImage()
    {
        var image = typeof(ResourceHelpers).Assembly.GetManifestResourceNames()
            .Where(x => x.EndsWith(".png"))
            .Random();

        using var stream = typeof(ResourceHelpers).Assembly.GetManifestResourceStream(image);

        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);

        return (image, buffer);
    }
}