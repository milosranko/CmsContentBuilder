using System.Globalization;

namespace CmsContentScaffolding.Optimizely.Tests.Constants;

internal static class StringConstants
{
    public static CultureInfo Language => CultureInfo.GetCultureInfo("sr");
    public const string Site1HostUrl = "https://localhost:5000";
    public const string Site2HostUrl = "https://localhost:5001";
    public const string TeaserBlocksFolderName = "Teaser Blocks Test";
    public const string Site1EditorsRole = "Site 1 Editors";
    public const string Site2EditorsRole = "Site 2 Editors";
    public const string TestUserPassword = "Test@1234";
}
