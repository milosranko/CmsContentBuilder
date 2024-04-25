using EPiServer.Core;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CmsContentScaffolding.Optimizely.Interfaces;

/// <summary>
/// Pages builder
/// </summary>
public interface IPagesBuilder
{
    /// <summary>
    /// Create or update page and set it as Start page for the current host
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <returns></returns>
    IPagesBuilder WithStartPage<T>(Action<IPagesBuilder> options) where T : PageData;
    /// <summary>
    /// Create or update page and set it as Start page for the current host
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    IPagesBuilder WithStartPage<T>(Action<T> value, Action<IPagesBuilder>? options = null) where T : PageData;
    /// <summary>
    /// Create or update page and set it as Start page for the current host and create translation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="translationLanguage"></param>
    /// <param name="translation"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    IPagesBuilder WithStartPage<T>(Action<T> value, CultureInfo translationLanguage, Action<T> translation, Action<IPagesBuilder>? options = null) where T : PageData;
    /// <summary>
    /// Create or update page and set it as Start page for the current host
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="contentReference"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    IPagesBuilder WithStartPage<T>(out ContentReference contentReference, Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : PageData;
    /// <summary>
    /// Create or update page in the site tree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <returns>IPagesBuilder</returns>
    IPagesBuilder WithPage<T>(Action<IPagesBuilder> options) where T : PageData;
    /// <summary>
    /// Create or update page in the site tree with translation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="translationLanguage"></param>
    /// <param name="translation"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    IPagesBuilder WithPage<T>(Action<T> value, CultureInfo translationLanguage, Action<T> translation, Action<IPagesBuilder>? options = null) where T : PageData;
    /// <summary>
    /// Create or update page in the site tree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="contentReference">Outputs reference for later use</param>
    /// <param name="value">Page properties</param>
    /// <param name="options"></param>
    /// <returns>IPagesBuilder</returns>
    IPagesBuilder WithPage<T>(out ContentReference contentReference, Action<T>? value = null, Action<IPagesBuilder>? options = null, bool SetAsStartPage = false, CultureInfo? translationLanguage = null, Action<T>? translation = null) where T : PageData;
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
    /// <param name="totalPages"></param>
    /// <returns></returns>
    IPagesBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData;
    /// <summary>
    /// Bulk create pages in the site tree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="totalPages">Total number of pages to create</param>
    /// <returns>IPagesBuilder</returns>
    IPagesBuilder WithPages<T>(out ContentReference[] contentReferences, [Range(1, 10000)] int totalPages = 1) where T : PageData;
    /// <summary>
    /// Bulk create pages in the site tree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">Page properties</param>
    /// <param name="totalPages">Total number of pages to create</param>
    /// <returns></returns>
    IPagesBuilder WithPages<T>(out ContentReference[] contentReferences, Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : PageData;
}
