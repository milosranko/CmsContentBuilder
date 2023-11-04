using EPiServer.Core;
using System.ComponentModel.DataAnnotations;

namespace CmsContentScaffolding.Optimizely.Interfaces;

public interface IPagesBuilder
{
	IPagesBuilder WithPage<T>(Action<IPagesBuilder> options) where T : IContent;
	IPagesBuilder WithPage<T>(Action<T>? value = null, Action<IPagesBuilder>? options = null) where T : IContent;
	IPagesBuilder WithPages<T>([Range(1, 10000)] int totalPages = 1) where T : IContent;
	IPagesBuilder WithPages<T>(Action<T>? value = null, [Range(1, 10000)] int totalPages = 1) where T : IContent;
}
