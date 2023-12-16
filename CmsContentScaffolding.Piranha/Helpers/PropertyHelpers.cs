using CmsContentScaffolding.Shared.Resources;
using Piranha;
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace CmsContentScaffolding.Piranha.Extensions;

public static class PropertyHelpers
{
	public static string AddRandomText(int maxLength = 50)
	{
		return ResourceHelpers.GetText().Substring(0, maxLength);
	}

	public static string AddRandomHtml()
	{
		return ResourceHelpers.GetHtmlText();
	}

	public static ImageField AddRandomImage(IApi api, string name)
	{
		var imageId = Guid.NewGuid();
		var image = ResourceHelpers.GetImageStream();
		var buffer = new byte[image.Length];

		image.Read(buffer, 0, buffer.Length);

		api.Media.SaveAsync(new BinaryMediaContent
		{
			Id = imageId,
			Filename = name,
			Data = buffer
		}).GetAwaiter().GetResult();

		return new ImageField { Id = imageId };
	}

	public static void Add(this ImageField field, string imagePath)
	{
		//TODO Upload image and get guid
		var media = new MediaFieldBase<ImageField>
		{
			Id = Guid.NewGuid()
		};

		field.Id = Guid.NewGuid();
	}

	public static IList<Block> Add<T>(this IList<Block> blocks, Action<T>? blockOptions = null)
		where T : Block, new()
	{
		var block = new T();
		blockOptions?.Invoke(block);

		blocks.Add(block);

		return blocks;
	}
}
