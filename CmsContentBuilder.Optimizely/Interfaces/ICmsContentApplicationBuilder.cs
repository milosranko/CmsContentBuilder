using EPiServer.Core;

namespace CmsContentBuilder.Optimizely.Interfaces;

public interface ICmsContentApplicationBuilder
{
    void WithPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : PageData;
    void WithPages<T>(Action<T>? value = null, int totalPages = 1) where T : PageData;
}