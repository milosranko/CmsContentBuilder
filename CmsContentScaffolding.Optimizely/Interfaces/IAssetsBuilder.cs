using EPiServer.Core;

namespace CmsContentScaffolding.Optimizely.Interfaces;

/// <summary>
/// Assets builder
/// </summary>
public interface IAssetsBuilder
{
	/// <summary>
	/// Create new folder in the assets tree
	/// </summary>
	/// <param name="name"></param>
	/// <param name="contentReference">Outputs reference for later use</param>
	/// <param name="options"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithFolder(string name, out ContentReference contentReference, Action<IAssetsBuilder>? options = null);
	/// <summary>
	/// Create new folder in the assets tree
	/// </summary>
	/// <param name="name"></param>
	/// <param name="options"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithFolder(string name, Action<IAssetsBuilder>? options = null);
	/// <summary>
	/// Create new content in the assets tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="contentReference">Outputs reference for later use</param>
	/// <param name="value"></param>
	/// <param name="options"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithContent<T>(out ContentReference contentReference, Action<T>? value = null, Action<IAssetsBuilder>? options = null) where T : IContent;
	/// <summary>
	/// Create new content in the assets tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <param name="options"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithContent<T>(Action<T>? value = null, Action<IAssetsBuilder>? options = null) where T : IContent;
	/// <summary>
	/// Create new block in the assets tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <param name="contentReference">Outputs reference for later use</param>
	/// <param name="value"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithBlock<T>(string name, out ContentReference contentReference, Action<T>? value = null) where T : IContentData;
	/// <summary>
	/// Create new block in the assets tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <param name="contentReference"></param>
	/// <param name="value"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithBlock<T>(string name, Action<T>? value = null) where T : IContentData;
	/// <summary>
	/// Create new media in the assets tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <param name="contentReference"></param>
	/// <param name="value"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithMedia<T>(Action<T>? value = null, Stream? stream = null, string? extension = null) where T : MediaData;
	/// <summary>
	/// Create new media in the assets tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="name"></param>
	/// <param name="contentReference">Outputs reference for later use</param>
	/// <param name="value"></param>
	/// <returns>IAssetsBuilder</returns>
	IAssetsBuilder WithMedia<T>(out ContentReference contentReference, Action<T>? value = null, Stream? stream = null, string? extension = null) where T : MediaData;
}