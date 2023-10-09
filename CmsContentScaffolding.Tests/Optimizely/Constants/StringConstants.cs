using System.Globalization;

namespace CmsContentScaffolding.Tests.Optimizely.Constants;

internal static class StringConstants
{
    public static CultureInfo Language => new("sr");
    public const string HostUrl = "http://localhost:5001";
    public const string TeaserBlocksFolderName = "Teaser Blocks Test";
    public const string TestRole = "Test Role";
    public const string TestUserPassword = "Test@1234";
}
