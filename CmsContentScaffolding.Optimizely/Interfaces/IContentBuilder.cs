using EPiServer.Core;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Interfaces;

public interface IContentBuilder
{
    IContentBuilder WithPage<T>(Action<IContentBuilder> options) where T : PageData;
    IContentBuilder WithPage<T>(Action<T>? value = null, Action<IContentBuilder>? options = null) where T : PageData;
    IContentBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : PageData;
    IContentBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData;
}