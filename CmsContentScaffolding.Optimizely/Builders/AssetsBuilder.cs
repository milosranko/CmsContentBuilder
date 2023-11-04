using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;

namespace CmsContentScaffolding.Optimizely.Builders;

internal class AssetsBuilder : IAssetsBuilder
{
	private readonly ContentReference _parent;
	private readonly IContentRepository _contentRepository;
	private readonly IContentBuilderManager _contentBuilderManager;
	private readonly ContentBuilderOptions _options;

	public AssetsBuilder(
		ContentReference parent,
		IContentRepository contentRepository,
		IContentBuilderManager contentBuilderManager,
		ContentBuilderOptions options)
	{
		_parent = parent;
		_contentRepository = contentRepository;
		_contentBuilderManager = contentBuilderManager;
		_options = options;
	}

	public IAssetsBuilder WithBlock<T>(string name, Action<T>? value = null) where T : IContentData
	{
		throw new NotImplementedException();
	}

	public IAssetsBuilder WithContent<T>(Action<T>? value = null, Action<IAssetsBuilder>? options = null) where T : IContent
	{
		throw new NotImplementedException();
	}

	public IAssetsBuilder WithFolder(string name, Action<IAssetsBuilder>? options = null)
	{
		throw new NotImplementedException();
	}

	public IAssetsBuilder WithMedia()
	{
		throw new NotImplementedException();
	}
}
