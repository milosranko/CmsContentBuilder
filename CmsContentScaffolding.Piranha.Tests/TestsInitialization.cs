namespace CmsContentScaffolding.Piranha.Tests;

[TestClass]
public class TestsInitialization
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        //Cleanup temporary files
        var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");

        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }
}
