using Piranha.Models;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Piranha.Interfaces;

public interface IPageBuilder
{
	IPageBuilder WithSite<T>(Action<T>? value = null) where T : SiteContent<T>;
	IPageBuilder WithPage<T>(Action<T>? value = null, Action<IPageBuilder>? options = null) where T : Page<T>;
	IPageBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : Page<T>;
}
