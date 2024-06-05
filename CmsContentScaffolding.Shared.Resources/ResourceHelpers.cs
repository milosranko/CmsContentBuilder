using Bogus;
using CmsContentScaffolding.Shared.Resources.Extensions;
using System.Reflection;

namespace CmsContentScaffolding.Shared.Resources;

public static class ResourceHelpers
{
    private static Lazy<Faker> _faker = new(new Faker());
    private static Lazy<Assembly> _assembly = new(typeof(ResourceHelpers).Assembly);

    public static Faker Faker => _faker.Value;

    public static string GetText()
    {
        using var stream = _assembly.Value.GetManifestResourceStream("CmsContentScaffolding.Shared.Resources.Texts.LoremIpsum.txt");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static string GetHtmlText()
    {
        using var stream = _assembly.Value.GetManifestResourceStream("CmsContentScaffolding.Shared.Resources.Texts.LoremIpsumHtml.txt");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static Stream GetImageStream()
    {
        var image = _assembly.Value.GetManifestResourceNames()
            .Where(x => x.EndsWith(".png"))
            .Random();

        return _assembly.Value.GetManifestResourceStream(image);
    }

    public static Stream GetVideoStream()
    {
        var file = _assembly.Value.GetManifestResourceNames()
            .Where(x => x.EndsWith(".mp4"))
            .Random();

        return _assembly.Value.GetManifestResourceStream(file);
    }
}