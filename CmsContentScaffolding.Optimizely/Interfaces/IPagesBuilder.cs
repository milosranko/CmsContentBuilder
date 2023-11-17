using EPiServer.Core;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Interfaces;

/// <summary>
/// Pages builder
/// </summary>
public interface IPagesBuilder
{
	/// <summary>
	/// Create or update page in the site tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="options"></param>
	/// <returns>IPagesBuilder</returns>
	IPagesBuilder WithPage<T>(Action<IPagesBuilder> options) where T : PageData;
	/// <summary>
	/// Create or update page in the site tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="contentReference">Outputs reference for later use</param>
	/// <param name="value">Page properties</param>
	/// <param name="options"></param>
	/// <returns>IPagesBuilder</returns>
	IPagesBuilder WithPage<T>(out ContentReference contentReference, Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : PageData;
	/// <summary>
	/// Create or update page in the site tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value">Page properties</param>
	/// <param name="options"></param>
	/// <returns>IPagesBuilder</returns>
	IPagesBuilder WithPage<T>(Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : PageData;
	/// <summary>
	/// Bulk create pages in the site tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="totalPages">Total number of pages to create</param>
	/// <returns>IPagesBuilder</returns>
	IPagesBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : PageData;
	/// <summary>
	/// Bulk create pages in the site tree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value">Page properties</param>
	/// <param name="totalPages">Total number of pages to create</param>
	/// <returns></returns>
	IPagesBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData;
}
