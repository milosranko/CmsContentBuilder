using EPiServer.Security;
using System.Globalization;

namespace CmsContentScaffolding.Optimizely.Models;

/// <summary>
/// Default options for the builder
/// </summary>
public record ContentBuilderOptions
{
    /// <summary>
    /// Set language for the builder, default is English
    /// </summary>
    public CultureInfo Language { get; set; } = CultureInfo.GetCultureInfo("en");
    /// <summary>
    /// Set default host for the builder, default is http://localhost
    /// </summary>
    public string SiteHost { get; set; } = "http://localhost";
    /// <summary>
    /// Set site name, default is Demo
    /// </summary>
    public string SiteName { get; set; } = "Demo";
    /// <summary>
    /// Set type of StartPage, if not set assets won't be able to create under SiteRoot
    /// </summary>
    public Type? StartPageType { get; set; }
    /// <summary>
    /// Set build mode
    /// </summary>
    public BuildMode BuildMode { get; set; } = BuildMode.Append;
    /// <summary>
    /// Set if content should be published when created, default is False
    /// </summary>
    public bool PublishContent { get; set; } = false;
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
