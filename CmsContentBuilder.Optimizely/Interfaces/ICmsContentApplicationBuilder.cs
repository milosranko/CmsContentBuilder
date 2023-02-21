using EPiServer.Core;
using System.ComponentModel.DataAnnotations;

namespace CmsContentBuilder.Optimizely.Interfaces;

public interface ICmsContentApplicationBuilder
{
    ICmsContentApplicationBuilder WithPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : PageData;
    void WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData;
}