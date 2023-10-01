using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System.ComponentModel.DataAnnotations;

namespace Optimizely.Demo.PublicWeb.Models.Blocks.Base;

public abstract class BlockBase : BlockData, IContent
{
    public PageData CurrentPage
    {
        get
        {
            try
            {
                return ServiceLocator.Current.GetInstance<IPageRouteHelper>().Page;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    #region IContent Properties - READ-ONLY Properties that expose data for IContent & simplify boxing requirements

    [ScaffoldColumn(false)]
    public virtual string Name { get; set; }
    [ScaffoldColumn(false)]
    public virtual ContentReference ContentLink { get; set; }
    [ScaffoldColumn(false)]
    public virtual ContentReference ParentLink { get; set; }
    [ScaffoldColumn(false)]
    public virtual Guid ContentGuid { get; set; }
    [ScaffoldColumn(false)]
    public virtual bool IsDeleted { get; set; }

    #endregion
}
