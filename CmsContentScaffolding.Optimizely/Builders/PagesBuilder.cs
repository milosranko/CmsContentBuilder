﻿using CmsContentScaffolding.Optimizely.Helpers;
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
	public static IPagesBuilder Empty => new PagesBuilder();

	#region Constructors

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

	#endregion

	#region WithPage methods

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

	public IPagesBuilder WithPage<T>(out ContentReference contentReference, Action<T>? value = null, Action<IPagesBuilder>? options = null, bool isStartPage = false)
		where T : PageData
	{
		contentReference = ContentReference.EmptyReference;
		if (_stop) return Empty;

		var page = CreatePageDraftAndInvoke(value);
		var existingPage = TryGetExistingPage<T>(page.Name, isStartPage);

		if (existingPage is null)
		{
			page.URLSegment = _urlSegmentGenerator.Create(page.Name);
			contentReference = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
		}
		else
		{
			DeletePageDraft(page);
			UpdateExistingPage(existingPage, isStartPage, value);

			contentReference = existingPage.ContentLink;
		}

		if (isStartPage)
			_contentBuilderManager.SetStartPageSecurity(contentReference);

		if (options == null)
			return this;

		var builder = new PagesBuilder(contentReference, _contentRepository, _contentBuilderManager, _options, _contentAssetHelper, _urlSegmentGenerator);
		options?.Invoke(builder);

		return this;
	}

	#endregion

	#region WithPages methods

	public IPagesBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData
	{
		return WithPages<T>(out var contentReferences, value, totalPages);
	}

	public IPagesBuilder WithPages<T>(out ContentReference[] contentReferences, [Range(1, 10000)] int totalPages = 1) where T : PageData
	{
		return WithPages<T>(out contentReferences, default, totalPages);
	}

	public IPagesBuilder WithPages<T>(out ContentReference[] contentReferences, Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData
	{
		contentReferences = Array.Empty<ContentReference>();

		if (_stop) return Empty;

		if (totalPages < 1 || totalPages > 10000)
			throw new ArgumentOutOfRangeException(nameof(totalPages));

		contentReferences = new ContentReference[totalPages];
		T page;
		T? existingPage;

		for (int i = 0; i < totalPages; i++)
		{
			page = CreatePageDraftAndInvoke(value, i.ToString());
			existingPage = TryGetExistingPage<T>(page.Name, false);

			if (existingPage is null)
			{
				page.URLSegment = _urlSegmentGenerator.Create(page.Name);
				contentReferences[i] = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
			}
			else
			{
				DeletePageDraft(page);
				UpdateExistingPage(existingPage, false, value);
				contentReferences[i] = existingPage.ContentLink;
			}
		}

		return this;
	}

	#endregion

	#region Private methods

	private T? TryGetExistingPage<T>(string pageName, bool isStartPage) where T : PageData
	{
		if (isStartPage &&
			_options.StartPageType != null &&
			_options.StartPageType.Equals(typeof(T)))
		{
			return _contentRepository
				.GetChildren<T>(_parent, _options.Language)
				.SingleOrDefault(x => x.Name.Equals(_options.StartPageType.Name));
		}

		if (_options.BuildMode == BuildMode.Overwrite)
		{
			return _contentRepository
				.GetChildren<T>(_parent, _options.Language)
				.FirstOrDefault(x => x.Name.Equals(pageName));
		}

		return default;
	}

	private void UpdateExistingPage<T>(T existingPage, bool isStartPage, Action<T>? value) where T : PageData
	{
		if (!isStartPage && _options.BuildMode != BuildMode.Overwrite)
			return;

		var existingPageWritable = (T)existingPage.CreateWritableClone();
		PropertyHelpers.InitProperties(existingPageWritable);

		var assetsFolder = _contentAssetHelper.GetOrCreateAssetFolder(existingPageWritable.ContentLink);
		_contentBuilderManager.CurrentReference = assetsFolder.ContentLink;

		value?.Invoke(existingPageWritable);

		existingPageWritable.URLSegment = _urlSegmentGenerator.Create(existingPageWritable.Name);
		_contentRepository.Save(existingPageWritable, SaveAction.Patch, AccessLevel.NoAccess);
	}

	private T CreatePageDraftAndInvoke<T>(Action<T>? value = null, string? nameSuffix = default) where T : PageData
	{
		var page = _contentRepository.GetDefault<T>(_parent, _options.Language);
		PropertyHelpers.InitProperties(page);
		page.Name = Constants.TempPageName;

		_contentRepository.Save(page, SaveAction.SkipValidation | SaveAction.Default, AccessLevel.NoAccess);
		var assetsFolder = _contentAssetHelper.GetOrCreateAssetFolder(page.ContentLink);
		_contentBuilderManager.CurrentReference = assetsFolder.ContentLink;

		value?.Invoke(page);
		_contentBuilderManager.SetContentName<T>(page, default, nameSuffix);

		return page;
	}

	private void DeletePageDraft<T>(T page) where T : PageData
	{
		if (page.ContentAssetsID != Guid.Empty)
		{
			var assetsFolder = _contentAssetHelper.GetAssetFolder(page.ContentLink);
			if (assetsFolder is not null)
			{
				var assets = _contentRepository
					.GetChildren<IContentData>(_contentAssetHelper.GetAssetFolder(page.ContentLink).ContentLink)
					.Cast<IContent>();

				foreach (var item in assets)
				{
					_contentRepository.Delete(item.ContentLink, true, AccessLevel.NoAccess);
				}
			}
		}

		_contentRepository.Delete(page.ContentLink, true, AccessLevel.NoAccess);
	}

	#endregion
}