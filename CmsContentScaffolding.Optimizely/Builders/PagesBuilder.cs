using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Web;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Builders;

internal class PagesBuilder : IPagesBuilder
{
	private readonly ContentReference _parent;
	private readonly IContentRepository _contentRepository;
	private readonly IContentBuilderManager _contentBuilderManager;
	private readonly ContentAssetHelper _contentAssetHelper;
	private readonly IUrlSegmentGenerator _urlSegmentGenerator;
	private readonly ContentBuilderOptions _options;
	private readonly bool _stop = false;

	public PagesBuilder() => _stop = true;

	public PagesBuilder(
		ContentReference parent,
		IContentRepository contentRepository,
		IContentBuilderManager contentBuilderManager,
		ContentBuilderOptions options,
		ContentAssetHelper contentAssetHelper,
		IUrlSegmentGenerator urlSegmentGenerator)
	{
		_parent = ContentReference.IsNullOrEmpty(parent)
			? ContentReference.RootPage
			: parent;
		_contentRepository = contentRepository;
		_options = options;
		_contentBuilderManager = contentBuilderManager;
		_contentAssetHelper = contentAssetHelper;
		_urlSegmentGenerator = urlSegmentGenerator;
	}

	public static IPagesBuilder Empty => new PagesBuilder();

	public IPagesBuilder WithStartPage<T>(out ContentReference contentReference, Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : PageData
	{
		return WithPage(out contentReference, value, options, true);
	}

	public IPagesBuilder WithStartPage<T>(Action<T> value, Action<IPagesBuilder>? options = null) where T : PageData
	{
		return WithPage(out var contentReference, value, options, true);
	}

	public IPagesBuilder WithStartPage<T>(Action<IPagesBuilder> options) where T : PageData
	{
		return WithPage<T>(out var contentReference, default, options, true);
	}

	public IPagesBuilder WithPage<T>(Action<IPagesBuilder> options) where T : PageData
	{
		return WithPage<T>(out var contentReference, default, options);
	}

	public IPagesBuilder WithPage<T>(Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : PageData
	{
		return WithPage(out var contentReference, value, options);
	}

	public IPagesBuilder WithPage<T>(out ContentReference contentReference, Action<T>? value = null, Action<IPagesBuilder>? options = null, bool setAsStartPage = false) where T : PageData
	{
		contentReference = ContentReference.EmptyReference;

		if (_stop) return Empty;

		var page = _contentRepository.GetDefault<T>(_parent, _options.Language);
		PropertyHelpers.InitProperties(page);
		page.Name = Constants.TempPageName;
		//Save draft
		contentReference = _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);
		var assetsFolder = _contentAssetHelper.GetOrCreateAssetFolder(contentReference);
		_contentBuilderManager.CurrentReference = assetsFolder.ContentLink;
		page.ContentAssetsID = assetsFolder.ContentGuid;
		value?.Invoke(page);

		_contentBuilderManager.SetContentName<T>(page);
		page.URLSegment = _urlSegmentGenerator.Create(page.Name);

		var existingPage = _contentRepository.GetChildren<T>(_parent, _options.Language).FirstOrDefault(x => x.Name.Equals(page.Name));

		if (existingPage is null)
		{
			//Save final
			contentReference = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

			if (setAsStartPage)
				_contentBuilderManager.SetAsStartPage(page.ContentLink);
		}
		else
		{
			//Delete temporary page
			_contentRepository.Delete(contentReference, true, AccessLevel.NoAccess);
			//Update existing one
			var existingPageWritable = (T)existingPage.CreateWritableClone();
			value?.Invoke(existingPageWritable);
			contentReference = _contentRepository.Save(existingPageWritable, SaveAction.Patch, AccessLevel.NoAccess);
		}

		if (options == null)
			return this;

		var builder = new PagesBuilder(contentReference, _contentRepository, _contentBuilderManager, _options, _contentAssetHelper, _urlSegmentGenerator);
		options?.Invoke(builder);

		return this;
	}

	public IPagesBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : PageData
	{
		return WithPages<T>(default, totalPages);
	}

	public IPagesBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData
	{
		if (_stop) return Empty;

		if (totalPages < 1 || totalPages > 10000)
			throw new ArgumentOutOfRangeException(nameof(totalPages));

		T page;

		for (int i = 0; i < totalPages; i++)
		{
			page = _contentRepository.GetDefault<T>(_parent, _options.Language);

			PropertyHelpers.InitProperties(page);
			page.Name = Constants.TempPageName;
			//Save draft
			var contentReference = _contentRepository.Save(page, SaveAction.Default, AccessLevel.NoAccess);
			var assetsFolder = _contentAssetHelper.GetOrCreateAssetFolder(contentReference);
			_contentBuilderManager.CurrentReference = assetsFolder.ContentLink;

			value?.Invoke(page);

			_contentBuilderManager.SetContentName<T>(page, default, i.ToString());
			page.URLSegment = _urlSegmentGenerator.Create(page.Name);

			//Skip if page with same name already exists
			if (_contentRepository.GetChildren<T>(_parent, _options.Language).Any(x => x.Name.Equals(page.Name, StringComparison.InvariantCultureIgnoreCase)))
				continue;

			//Save final
			_contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
		}

		return this;
	}
}