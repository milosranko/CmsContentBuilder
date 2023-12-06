namespace CmsContentScaffolding.Piranha.Interfaces;

public interface IContentBuilder
{
	IPageBuilder UsePages(Guid? root = default);
	//IPageContentBuilder WithPage<T>(Action<T>? value = null, Action<IPageContentBuilder>? options = null) where T : Page<T>;
	//IPageContentBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : Page<T>;
}
