using Piranha.Models;
using System.ComponentModel.DataAnnotations;

namespace CmsContentBuilder.Piranha.Interfaces;

public interface IPageContentBuilder
{
    void WithSubPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : Page<T>;
    void WithSubPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : Page<T>;
}
