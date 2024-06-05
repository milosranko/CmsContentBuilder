using Piranha.Extend;
using Piranha.Models;

namespace PiranhaCMS.PublicWeb.Models.Blocks.Base;

public abstract class BlockGroupBase : BlockGroup, ICurrentPage
{
    public PageBase CurrentPage => default;
}
