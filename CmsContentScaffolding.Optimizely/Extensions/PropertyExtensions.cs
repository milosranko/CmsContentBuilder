using CmsContentScaffolding.Optimizely.Interfaces;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Core.Internal;
using EPiServer.ServiceLocation;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Extensions;

public static class PropertyExtensions
{
	#region ContentArea extensions

	#region AddItem methods

	public static ContentArea AddItem<T>(this ContentArea contentArea) where T : IContentData
	{
		return AddItem<T>(contentArea, default, default);
	}

	public static ContentArea AddItem<T>(
		this ContentArea contentArea,
		string name) where T : IContentData
	{
		return AddItem<T>(contentArea, name, default);
	}

	public static ContentArea AddItem<T>(
		this ContentArea contentArea,
		Action<T> options) where T : IContentData
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
		Action<T>? options = default) where T : IContentData
	{
		var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();

		return AddItemToContentArea(contentArea, contentBuilderManager.CreateItem(name, default, options));
	}

	#endregion

	#region AddItems methods

	public static ContentArea AddItems<T>(this ContentArea contentArea) where T : IContentData
	{
		return AddItems<T>(contentArea, default, default, default);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		string name,
		[Range(1, 10000)] int total) where T : IContentData
	{
		return AddItems<T>(contentArea, name, default, total);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		Action<T> options,
		[Range(1, 10000)] int total) where T : IContentData
	{
		return AddItems(contentArea, default, options, total);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		[Range(1, 10000)] int total) where T : IContentData
	{
		return AddItems<T>(contentArea, default, default, total);
	}

	public static ContentArea AddItems<T>(
		this ContentArea contentArea,
		string? name = default,
		Action<T>? options = default,
		[Range(1, 10000)] int total = 1) where T : IContentData
	{
		var contentBuilderManager = ServiceLocator.Current.GetInstance<IContentBuilderManager>();

		for (int i = 0; i < total; i++)
			AddItemToContentArea(contentArea, contentBuilderManager.CreateItem(name, i.ToString(), options));

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
