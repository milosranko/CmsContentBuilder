using EPiServer.Core;
using System.ComponentModel.DataAnnotations;

namespace CmsContentBuilder.Optimizely.Interfaces;

public interface IPageContentBuilder
{
    void WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : PageData;
    void WithSubPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData;
}
