using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;
using PiranhaCMS.PublicWeb.Models.Blocks.Base;
using System.ComponentModel.DataAnnotations;

namespace PiranhaCMS.PublicWeb.Models.Blocks;

[BlockType(
    Name = "Search Block",
    Category = "Teasers")]
public class SearchBlock : BlockBase
{
    [Field(
        Title = "Heading",
        Placeholder = "Enter heading text",
        Options = FieldOption.HalfWidth,
        Description = "This is block heading")]
    [Required(ErrorMessage = "Heading: required!")]
    public StringField Heading { get; set; }

    [Field(
        Title = "Lead Text",
        Placeholder = "Enter lead text",
        Options = FieldOption.HalfWidth,
        Description = "This is block lead text")]
    [StringLength(255, ErrorMessage = "Lead text: maximum length is 255 characters!")]
    public TextField LeadText { get; set; }
}
