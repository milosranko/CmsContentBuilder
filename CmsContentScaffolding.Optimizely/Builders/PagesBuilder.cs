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

	public IPagesBuilder WithPage<T>(Action<IPagesBuilder> options) where T : IContent
	{
		return WithPage<T>(default, options);
	}

	public IPagesBuilder WithPage<T>(Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : IContent
	{
		var page = _contentRepository.GetDefault<T>(_parent, _options.Language);

		PropertyHelpers.InitProperties(page);
		value?.Invoke(page);

		_contentBuilderManager.GetOrSetContentName<T>(page);

		if (!_contentRepository.GetChildren<T>(_parent).Any(x => x.Name.Equals(page.Name)))
		{
			var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

			if (_options.StartPageType != null && _options.StartPageType.Equals(typeof(T)))
			{
				_contentBuilderManager.SetAsStartPage(pageRef);
			}

			var contentToMove = _contentRepository
				.GetChildren<IContentData>(_contentBuilderManager.GetOrCreateTempFolder(), _options.Language)
				.Cast<IContent>();

			foreach (var item in contentToMove)
			{
				_contentRepository.Move(item.ContentLink, pageRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
			}
		}

		if (options == null)
			return this;

		var builder = new PagesBuilder(page.ContentLink, _contentRepository, _contentBuilderManager, _options);
		options?.Invoke(builder);

		return this;
	}

	public IPagesBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : IContent
	{
		return WithPages<T>(default, totalPages);
	}

	public IPagesBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : IContent
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

			if (_contentRepository.GetChildren<T>(_parent).Any(x => x.Name.Equals(page.Name)))
				continue;

			var pageRef = _contentRepository.Save(page, _options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
			var contentToMove = _contentRepository
				.GetChildren<IContentData>(_contentBuilderManager.GetOrCreateTempFolder(), _options.Language)
				.Cast<IContent>();

			foreach (var item in contentToMove)
			{
				_contentRepository.Move(item.ContentLink, pageRef, AccessLevel.NoAccess, AccessLevel.NoAccess);
			}
		}

		return this;
	}
}