using Piranha.Models;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Piranha.Interfaces;

public interface IPageContentBuilder
{
    IPageContentBuilder WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : Page<T>;
    void WithSubPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : Page<T>;
}
