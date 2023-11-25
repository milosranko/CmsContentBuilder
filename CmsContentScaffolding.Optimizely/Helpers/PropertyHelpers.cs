using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace CmsContentScaffolding.Optimizely.Helpers;

public static class PropertyHelpers
{
	public static IDictionary<Type, PropertyInfo[]> TypeProperties = new Dictionary<Type, PropertyInfo[]>();

	public static ContentReference GetOrAddImage<TMedia>(string name, Stream stream, int width = 1200, int height = 800) where TMedia : MediaData
	{
		var options = ServiceLocator.Current.GetInstance<ContentBuilderOptions>();
		var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
		var mediaFolder = ContentReference.IsNullOrEmpty(SiteDefinition.Current.SiteAssetsRoot)
			? SiteDefinition.Current.GlobalAssetsRoot
			: SiteDefinition.Current.SiteAssetsRoot;
		var existingItems = contentRepository
			.GetChildren<TMedia>(mediaFolder)
			.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

		if (existingItems != null && existingItems.Any())
			return existingItems.ElementAt(0).ContentLink;

		var blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();
		var image = contentRepository.GetDefault<TMedia>(mediaFolder);
		var blob = blobFactory.CreateBlob(image.BinaryDataContainer, ".png");

		blob.Write(stream);
		image.BinaryData = blob;
		image.Name = name;

		return contentRepository.Save(image, options.PublishContent ? SaveAction.Publish : SaveAction.Default, AccessLevel.NoAccess);
	}

	public static void InitProperties<T>(T content) where T : IContentData
	{
		var type = typeof(T);

		if (!TypeProperties.ContainsKey(type))
			TypeProperties.Add(type, type
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.PropertyType.Name.Equals(nameof(ContentArea)) || x.PropertyType.Name.Equals(nameof(XhtmlString)))
				.ToArray());

		for (int i = 0; i < TypeProperties[type].Length; i++)
		{
			if (TypeProperties[type][i].GetValue(content) is null)
				TypeProperties[type][i].SetValue(content, TypeProperties[type][i].PropertyType.Name.Equals(nameof(ContentArea)) ? new ContentArea() : new XhtmlString());
		}
	}
}
