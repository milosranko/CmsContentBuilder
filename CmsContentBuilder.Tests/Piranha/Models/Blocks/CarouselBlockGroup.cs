using Piranha.Extend;
using Piranha.Models;
using PiranhaCMS.PublicWeb.Models.Blocks.Base;

namespace PiranhaCMS.PublicWeb.Models.Blocks;

[BlockGroupType(
    Name = "Carousel Block Group",
    Display = BlockDisplayMode.Vertical,
    Category = "Carousel")]
[BlockItemType(Type = typeof(CarouselItemBlock))]
public class CarouselBlockGroup : BlockGroupBase
{ }
