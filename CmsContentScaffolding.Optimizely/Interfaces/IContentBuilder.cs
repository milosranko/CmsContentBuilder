using EPiServer.Core;

namespace CmsContentScaffolding.Optimizely.Interfaces;

public interface IContentBuilder
{
	IAssetsBuilder UseAssets(ContentReference? root = null);
	IPagesBuilder UsePages(ContentReference? root = null);
}
