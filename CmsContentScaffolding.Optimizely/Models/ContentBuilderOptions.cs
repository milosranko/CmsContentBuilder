using EPiServer.Security;
using System.Globalization;

namespace CmsContentScaffolding.Optimizely.Models;

/// <summary>
/// Default options for the builder
/// </summary>
public class ContentBuilderOptions
{
	/// <summary>
	/// Set language for the builder, default is English
	/// </summary>
	public CultureInfo Language { get; set; } = new CultureInfo("en");
	/// <summary>
	/// Set default host for the builder, default is http://localhost
	/// </summary>
	public string DefaultHost { get; set; } = "http://localhost";
	/// <summary>
	/// Set site name, default is Demo
	/// </summary>
	public string SiteName { get; set; } = "Demo";
	/// <summary>
	/// Set build mode
	/// </summary>
	public BuildMode BuildMode { get; set; } = BuildMode.Append;
	/// <summary>
	/// Set if content should be published when created, default is False
	/// </summary>
	public bool PublishContent { get; set; } = false;
	/// <summary>
	/// Set default assets location, default is GlobalRoot
	/// </summary>
	public BlocksLocation BlocksLocation { get; set; } = BlocksLocation.GlobalRoot;
	/// <summary>
	/// Set to False if you don't want WebAdmins and WebEditors roles to be created
	/// </summary>
	public bool CreateDefaultRoles { get; set; } = true;
	/// <summary>
	/// Define new roles
	/// </summary>
	public IDictionary<string, AccessLevel>? Roles { get; set; }
	/// <summary>
	/// Define new users thah will have an access to site instance
	/// </summary>
	public IList<UserModel>? Users { get; set; }
}
