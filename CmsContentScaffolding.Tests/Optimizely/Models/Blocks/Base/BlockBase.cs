﻿using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Optimizely.Demo.PublicWeb.Models.Blocks.Base;

public abstract class BlockBase : BlockData
{
    private readonly Injected<IPageRouteHelper> _pageRouteHelper;

    public PageData? CurrentPage
    {
        get
        {
            try
            {
                return _pageRouteHelper.Service.Page;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
