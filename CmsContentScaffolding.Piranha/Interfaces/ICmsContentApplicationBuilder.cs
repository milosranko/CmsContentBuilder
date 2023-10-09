using Piranha.Models;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Piranha.Interfaces;

public interface ICmsContentApplicationBuilder
{
    ICmsContentApplicationBuilder WithSite<T>(Action<T>? value = null) where T : SiteContent<T>;
    ICmsContentApplicationBuilder WithPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : Page<T>;
    void WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : Page<T>;
}
