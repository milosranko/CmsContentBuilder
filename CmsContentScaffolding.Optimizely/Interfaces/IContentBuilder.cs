using EPiServer.Core;

namespace CmsContentScaffolding.Optimizely.Interfaces;

/// <summary>
/// Content builder options for creating assets or pages
/// </summary>
public interface IContentBuilder : IDisposable
{
	/// <summary>
	/// Use assets
	/// </summary>
	/// <param name="root">Assets root reference</param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder UseAssets(ContentReference? root = null);
	/// <summary>
	/// Use pages
	/// </summary>
	/// <param name="root">Pages root reference</param>
	/// <returns>IPagesBuilder</returns>
	IPagesBuilder UsePages(ContentReference? root = null);
}
