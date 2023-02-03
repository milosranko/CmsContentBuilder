using Piranha.Extend;
using Piranha.Models;

namespace PiranhaCMS.PublicWeb.Models.Blocks.Base;

public abstract class BlockBase : Block, ICurrentPage
{
    public PageBase CurrentPage => default;
}
