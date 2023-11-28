using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Core.Internal;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Extensions;

public static class PropertyExtensions
{
	#region ContentArea extensions

	#region AddItem methods

	public static ContentArea AddItem<T>(this ContentArea contentArea) where T : IContentData, new()
	{
		return AddItem<T>(contentArea, default, default);
	}

	public static ContentArea AddItem<T>(
		this ContentArea contentArea,
		string name) where T : IContentData, new()
	{
		return AddItem<T>(contentArea, name, default);
	}

	public static ContentArea AddItem<T>(
		this ContentArea contentArea,
		Action<T> options) where T : IContentData, new()
	{
		return AddItem(contentArea, default, options);
	}

	public static ContentArea AddExistingItem(
		this ContentArea contentArea,
		ContentReference contentReference)
	{
		if (ContentReference.IsNullOrEmpty(contentReference))
			return contentArea;

		return AddItemToContentArea(contentArea, contentReference);
	}

	public static ContentArea AddItem<T>(
		this ContentArea contentArea,
		string? name = default,
		Action<T>? options = default) where T : IContentData, new()
	{
		var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
		var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
		var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();
		var content = contentRepository.GetDefault<T>(contentBuilderManager.CurrentReference, globalOptions.Language);

		PropertyHelpers.InitProperties(content);
		options?.Invoke(content);

		var iContent = (IContent)content;
		contentBuilderManager.SetContentName<T>(iContent, name);

		if (!ContentReference.IsNullOrEmpty(iContent.ContentLink))
			return AddItemToContentArea(contentArea, iContent.ContentLink);

		contentRepository.Save(iContent, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);

		return AddItemToContentArea(contentArea, iContent.ContentLink);
	}

	#endregion

	#region AddItems methods

	public static ContentArea AddItems<T>(this ContentArea contentArea) where T : IContentData, new()
	{
		return AddItems<T>(contentArea, default, default, default);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		string name,
		[Range(1, 10000)] int total) where T : IContentData, new()
	{
		return AddItems<T>(contentArea, name, default, total);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		Action<T> options,
		[Range(1, 10000)] int total) where T : IContentData, new()
	{
		return AddItems(contentArea, default, options, total);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		[Range(1, 10000)] int total) where T : IContentData, new()
	{
		return AddItems<T>(contentArea, default, default, total);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		string? name = default,
		Action<T>? options = default,
		[Range(1, 10000)] int total = 1) where T : IContentData, new()
	{
		var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
		var globalOptions = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
		var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();

		T content;
		var typeName = typeof(T).Name;

		for (int i = 0; i < total; i++)
		{
			content = contentRepository.GetDefault<T>(contentBuilderManager.CurrentReference, globalOptions.Language);

			PropertyHelpers.InitProperties(content);
			options?.Invoke(content);

			var iContent = (IContent)content;
			contentBuilderManager.SetContentName<T>(iContent, name, i.ToString());

			if (!ContentReference.IsNullOrEmpty(iContent.ContentLink))
			{
				AddItemToContentArea(contentArea, iContent.ContentLink);
				continue;
			}

			contentRepository.Save(iContent, globalOptions.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
			AddItemToContentArea(contentArea, iContent.ContentLink);
		}

		return contentArea;
	}

	#endregion

	#endregion

	#region XhtmlString extensions

	public static XhtmlString AddStringFragment(this XhtmlString xhtmlString, string text)
	{
		xhtmlString.Fragments.Add(new StaticFragment(text));

		return xhtmlString;
	}

	public static XhtmlString AddContentFragment(this XhtmlString xhtmlString, ContentReference contentReference)
	{
		var contentFragmentFactory = ServiceLocator.Current.GetInstance<ContentFragmentFactory>();
		var fragment = contentFragmentFactory.CreateContentFragment(contentReference, Guid.Empty, null);

		xhtmlString.Fragments.Add(fragment);

		return xhtmlString;
	}

	#endregion

	#region Private methods

	private static ContentArea AddItemToContentArea(ContentArea contentArea, ContentReference contentReference)
	{
		contentArea.Items.Add(new ContentAreaItem
		{
			ContentLink = contentReference
		});

		return contentArea;
	}

	#endregion
}
