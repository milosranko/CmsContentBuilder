using EPiServer.Core;

namespace CmsContentBuilder.Optimizely.Interfaces;

public interface IPageContentBuilder
{
    void WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : PageData;
    void WithSubPages<T>(Action<T>? value = null, int totalPages = 1) where T : PageData;
}
