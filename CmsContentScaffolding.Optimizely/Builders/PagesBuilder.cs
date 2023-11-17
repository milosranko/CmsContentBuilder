using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Builders;

internal class PagesBuilder : IPagesBuilder
{
	private readonly ContentReference _parent;
	private readonly IContentRepository _contentRepository;
	private readonly IContentBuilderManager _contentBuilderManager;
	private readonly ContentBuilderOptions _options;

	public PagesBuilder(
		ContentReference parent,
		IContentRepository contentRepository,
		IContentBuilderManager contentBuilderManager,
		ContentBuilderOptions options)
	{
		_parent = ContentReference.IsNullOrEmpty(parent)
			? ContentReference.RootPage
			: parent;
		_contentRepository = contentRepository;
		_options = options;
		_contentBuilderManager = contentBuilderManager;
	}

	public IPagesBuilder WithPage<T>(Action<IPagesBuilder> options) where T : PageData
	{
		return WithPage<T>(out var contentReference, default, options);
	}

	public IPagesBuilder WithPage<T>(Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : PageData
	{
		return WithPage(out var contentReference, value, options);
	}

	public IPagesBuilder WithPage<T>(out ContentReference contentReference, Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : PageData
	{
		contentReference = ContentReference.EmptyReference;
		var page = _contentRepository.GetDefault<T>(_parent, _options.Language);

		PropertyHelpers.InitProperties(page);
		value?.Invoke(page);

		_contentBuilderManager.GetOrSetContentName<T>(page);

		var existingPage = _contentRepository.GetChildren<T>(_parent, _options.Language).FirstOrDefault(x => x.Name.Equals(page.Name));

		if (existingPage is null)
		{
			contentReference = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

			if (_options.StartPageType != null && _options.StartPageType.Equals(typeof(T)))
				_contentBuilderManager.SetAsStartPage(contentReference);

			var contentToMove = _contentRepository
				.GetChildren<IContentData>(_contentBuilderManager.GetOrCreateTempFolder(), _options.Language)
				.Cast<IContent>();

			foreach (var item in contentToMove)
				_contentRepository.Move(item.ContentLink, contentReference, AccessLevel.NoAccess, AccessLevel.NoAccess);
		}
		else
		{
			var existingPageWritable = (T)existingPage.CreateWritableClone();
			value?.Invoke(existingPageWritable);
			contentReference = _contentRepository.Save(existingPageWritable, SaveAction.Patch, AccessLevel.NoAccess);
		}

		if (options == null)
			return this;

		var builder = new PagesBuilder(contentReference, _contentRepository, _contentBuilderManager, _options);
		options?.Invoke(builder);

		return this;
	}

	public IPagesBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : PageData
	{
		return WithPages<T>(default, totalPages);
	}

	public IPagesBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData
	{
		if (totalPages < 1 || totalPages > 10000)
			throw new ArgumentOutOfRangeException(nameof(totalPages));

		T page;

		for (int i = 0; i < totalPages; i++)
		{
			page = _contentRepository.GetDefault<T>(_parent, _options.Language);

			PropertyHelpers.InitProperties(page);
			value?.Invoke(page);

			_contentBuilderManager.GetOrSetContentName<T>(page, default, i.ToString());

			if (_contentRepository.GetChildren<T>(_parent, _options.Language).Any(x => x.Name.Equals(page.Name)))
				continue;

			var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
			var contentToMove = _contentRepository
				.GetChildren<IContentData>(_contentBuilderManager.GetOrCreateTempFolder(), _options.Language)
				.Cast<IContent>();

			foreach (var item in contentToMove)
				_contentRepository.Move(item.ContentLink, pageRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
		}

		return this;
	}
}