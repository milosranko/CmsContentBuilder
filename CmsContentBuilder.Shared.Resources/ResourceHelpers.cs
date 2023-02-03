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

    public static byte[] GetImage()
    {
        using var stream = typeof(ResourceHelpers).Assembly.GetManifestResourceStream("CmsContentBuilder.Shared.Resources.Images.HLD_Screenshot_01_mech_1080.png");
        var buffer = new byte[stream.Length];

        stream.Read(buffer, 0, 0);

        return buffer;
    }
}