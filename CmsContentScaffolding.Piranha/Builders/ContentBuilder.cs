using CmsContentScaffolding.Piranha.Interfaces;
using CmsContentScaffolding.Piranha.Models;
using Piranha;
using Piranha.Models;

namespace CmsContentScaffolding.Piranha.Builders;

public class ContentBuilder : IContentBuilder
{
	private readonly IApi _api;
	private readonly Site _site;
	private readonly CmsContentApplicationBuilderOptions _options;

	public ContentBuilder(IApi api, CmsContentApplicationBuilderOptions options)
	{
		_api = api;
		_site = _api.Sites.GetDefaultAsync().GetAwaiter().GetResult();
		_options = options;
	}

	public IPageBuilder UsePages(Guid? root = default)
	{
		if (root.HasValue && root.Value != Guid.Empty)
		{
			var parent = _api.Pages.GetByIdAsync(root.Value).GetAwaiter().GetResult();
			return new PageBuilder(_api, parent.Id, parent.SiteId, _options);
		}

		return new PageBuilder(_api, default, _site != null ? _site.Id : default, _options);
	}
}