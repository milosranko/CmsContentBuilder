using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;

namespace CmsContentScaffolding.Optimizely.Builders;

internal class AssetsBuilder : IAssetsBuilder
{
	private readonly ContentReference _parent;
	private readonly IContentRepository _contentRepository;
	private readonly IContentBuilderManager _contentBuilderManager;
	private readonly IBlobFactory _blobFactory;
	private readonly ContentBuilderOptions _options;

	public AssetsBuilder(
		ContentReference parent,
		IContentRepository contentRepository,
		IContentBuilderManager contentBuilderManager,
		ContentBuilderOptions options,
		IBlobFactory blobFactory)
	{
		_parent = parent;
		_contentRepository = contentRepository;
		_contentBuilderManager = contentBuilderManager;
		_options = options;
		_blobFactory = blobFactory;
	}

	public IAssetsBuilder WithBlock<T>(string name, Action<T>? value = null) where T : IContentData
	{
		var site = _contentBuilderManager.GetOrCreateSite();
		var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent)
			? _parent
			: site.SiteAssetsRoot;
		var existingBlock = _contentRepository
			.GetChildren<T>(parent)
			.SingleOrDefault(x => ((IContent)x).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

		if (existingBlock is not null)
			return this;

		var block = _contentRepository.GetDefault<T>(parent, _options.Language);

		PropertyHelpers.InitProperties(block);
		value?.Invoke(block);

		var iContent = (IContent)block;

		_contentBuilderManager.GetOrSetContentName<T>(iContent, name);
		_contentRepository.Save(iContent, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

		return this;
	}

	public IAssetsBuilder WithContent<T>(Action<T>? value = null, Action<IAssetsBuilder>? options = null) where T : IContent
	{
		var site = _contentBuilderManager.GetOrCreateSite();
		var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent)
			? _parent
			: site.SiteAssetsRoot;
		var content = _contentRepository.GetDefault<T>(parent, _options.Language);

		PropertyHelpers.InitProperties(content);
		value?.Invoke(content);

		var existingContent = _contentRepository
			.GetChildren<T>(parent)
			.SingleOrDefault(x => ((IContent)x).Name.Equals(content.Name, StringComparison.InvariantCultureIgnoreCase));

		if (existingContent is null)
		{
			_contentBuilderManager.GetOrSetContentName<T>(content);
			parent = _contentRepository.Save(content, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
		}
		else
		{
			parent = existingContent.ContentLink;
		}

		if (options == null)
			return this;

		var builder = new AssetsBuilder(parent, _contentRepository, _contentBuilderManager, _options, _blobFactory);
		options?.Invoke(builder);

		return this;
	}

	public IAssetsBuilder WithFolder(string name, Action<IAssetsBuilder>? options = null)
	{
		var site = _contentBuilderManager.GetOrCreateSite();
		var parent = _parent != null && !ContentReference.IsNullOrEmpty(_parent)
			? _parent
			: site.SiteAssetsRoot;
		var existingContent = _contentRepository
			.GetChildren<ContentFolder>(parent)
			.SingleOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

		if (existingContent is null)
		{
			var content = _contentRepository.GetDefault<ContentFolder>(parent, _options.Language);
			content.Name = name;

			parent = _contentRepository.Save(content, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
		}
		else
		{
			parent = existingContent.ContentLink;
		}

		if (options == null)
			return this;

		var builder = new AssetsBuilder(parent, _contentRepository, _contentBuilderManager, _options, _blobFactory);
		options?.Invoke(builder);

		return this;
	}

	public IAssetsBuilder WithMedia<T>(Action<T>? value = null, Stream? stream = null, string? extension = null) where T : MediaData
	{
		var site = _contentBuilderManager.GetOrCreateSite();
		var parent = _parent is not null && !ContentReference.IsNullOrEmpty(_parent)
			? _parent
			: site.SiteAssetsRoot;

		var media = _contentRepository.GetDefault<T>(parent);
		value?.Invoke(media);

		var existingItem = _contentRepository
			.GetChildren<T>(parent)
			.Any(x => x.Name.Equals(media.Name, StringComparison.InvariantCultureIgnoreCase));

		if (existingItem)
			return this;

		if (stream is not null && !string.IsNullOrEmpty(extension))
		{
			var blob = _blobFactory.CreateBlob(media.BinaryDataContainer, extension);

			blob.Write(stream);
			media.BinaryData = blob;
		}

		var contentRef = _contentRepository.Save(media, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

		return this;
	}
}
