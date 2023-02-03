using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Optimizely.Demo.PublicWeb.Models.Blocks.Base;

public abstract class BlockBase : BlockData
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
}
