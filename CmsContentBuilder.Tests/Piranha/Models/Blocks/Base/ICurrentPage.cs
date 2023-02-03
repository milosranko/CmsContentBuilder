using Piranha.Models;

namespace PiranhaCMS.PublicWeb.Models.Blocks.Base;

public interface ICurrentPage
{
    PageBase CurrentPage { get; }
}
