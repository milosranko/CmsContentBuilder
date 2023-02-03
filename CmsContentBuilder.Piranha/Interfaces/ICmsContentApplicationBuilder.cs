using CmsContentBuilder.Piranha.Models;
using Piranha.Models;

namespace CmsContentBuilder.Piranha.Interfaces;

public interface ICmsContentApplicationBuilder
{
    BuildModeEnum BuildMode { get; set; }
    string DefaultLanguage { get; set; }

    void WithSite<T>(Action<T>? value = null) where T : SiteContent<T>;
    void WithPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : Page<T>;
    void WithPages<T>(Action<T>? value = null, int totalPages = 1) where T : Page<T>;
}