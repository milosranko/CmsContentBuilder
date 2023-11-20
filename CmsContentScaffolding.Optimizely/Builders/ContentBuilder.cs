using CmsContentScaffolding.Optimizely.Helpers;
using CmsContentScaffolding.Optimizely.Interfaces;
using CmsContentScaffolding.Optimizely.Models;
using EPiServer;
using EPiServer.Authorization;
using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Security;

namespace CmsContentScaffolding.Optimizely.Builders;

internal class ContentBuilder : IContentBuilder
{
	private readonly IContentRepository _contentRepository;
	private readonly IContentBuilderManager _contentBuilderManager;
	private readonly IBlobFactory _blobFactory;
	private readonly ContentAssetHelper _contentAssetHelper;
	private readonly ContentBuilderOptions _contentBuilderOptions;
	private readonly bool _buildContent;
	private bool disposedValue;

	public ContentBuilder(
		IContentRepository contentRepository,
		IContentBuilderManager contentBuilderManager,
		ContentBuilderOptions contentBuilderOptions,
		IBlobFactory blobFactory,
		ContentAssetHelper contentAssetHelper)
	{
		_contentRepository = contentRepository;
		_contentBuilderManager = contentBuilderManager;
		_contentBuilderOptions = contentBuilderOptions;
		_blobFactory = blobFactory;
		_buildContent = ApplyOptions();
		_contentAssetHelper = contentAssetHelper;
	}

	public IAssetsBuilder UseAssets(ContentReference? root = null)
	{
		if (_buildContent)
			return new AssetsBuilder(root ?? ContentReference.GlobalBlockFolder, _contentRepository, _contentBuilderManager, _contentBuilderOptions, _blobFactory);

		return AssetsBuilder.Empty;
	}

	public IPagesBuilder UsePages(ContentReference? root = null)
	{
		if (_buildContent)
			return new PagesBuilder(root ?? ContentReference.RootPage, _contentRepository, _contentBuilderManager, _contentBuilderOptions, _contentAssetHelper);

		return PagesBuilder.Empty;
	}

	private bool ApplyOptions()
	{
		var proceedBuildingContent = false;

		switch (_contentBuilderOptions.BuildMode)
		{
			case BuildMode.Append:
				proceedBuildingContent = true;
				break;
			case BuildMode.Overwrite:
				proceedBuildingContent = true;
				break;
			case BuildMode.OnlyIfEmptyInDefaultLanguage:
				proceedBuildingContent = _contentBuilderManager.IsInstallationEmpty();
				break;
			case BuildMode.OnlyIfEmptyRegardlessOfLanguage:
				proceedBuildingContent = _contentBuilderManager.IsInstallationEmpty();
				break;
			default:
				break;
		}

		if (!proceedBuildingContent)
			return false;

		_contentBuilderManager.ApplyDefaultLanguage();

		if (_contentBuilderOptions.CreateDefaultRoles)
			_contentBuilderManager.CreateDefaultRoles(new Dictionary<string, AccessLevel>
			{
				{ Roles.WebEditors, AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Publish },
				{ Roles.WebAdmins, AccessLevel.FullAccess }
			});

		_contentBuilderManager.CreateRoles(_contentBuilderOptions.Roles);
		_contentBuilderManager.CreateUsers(_contentBuilderOptions.Users);

		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				PropertyHelpers.TypeProperties.Clear();
			}
			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~ContentBuilder()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
