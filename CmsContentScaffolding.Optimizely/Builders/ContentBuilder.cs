using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Blobs;

namespace CmsContentScaffolding.Optimizely.Builders;

internal class ContentBuilder : IContentBuilder
{
	private readonly IContentRepository _contentRepository;
	private readonly IContentBuilderManager _contentBuilderManager;
	private readonly IBlobFactory _blobFactory;
	private readonly ContentBuilderOptions _contentBuilderOptions;

	public ContentBuilder(
		IContentRepository contentRepository,
		IContentBuilderManager contentBuilderManager,
		ContentBuilderOptions contentBuilderOptions,
		IBlobFactory blobFactory)
	{
		_contentRepository = contentRepository;
		_contentBuilderManager = contentBuilderManager;
		_contentBuilderOptions = contentBuilderOptions;
		_blobFactory = blobFactory;
	}

	public IAssetsBuilder UseAssets(ContentReference? root = null)
	{
		return new AssetsBuilder(root ?? ContentReference.EmptyReference, _contentRepository, _contentBuilderManager, _contentBuilderOptions, _blobFactory);
	}

	public IPagesBuilder UsePages(ContentReference? root = null)
	{
		return new PagesBuilder(root ?? ContentReference.EmptyReference, _contentRepository, _contentBuilderManager, _contentBuilderOptions);
	}
}
