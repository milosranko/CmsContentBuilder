using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using System.ComponentModel.DataAnnotations;

namespace Optimizely.Demo.PublicWeb.Models.Blocks.Local;

[ContentType(
    AvailableInEditMode = false,
    GUID = "{5EF52798-35CA-45F5-9750-54F1CC4BDC91}",
    GroupName = "SEO",
    Order = 9800)]
public class SitemapSettingsBlock : BlockData
{
    #region SEO tab

    [CultureSpecific]
    [Display(Order = 10)]
    public virtual bool Exclude { get; set; }

    [CultureSpecific]
    [Display(Order = 20)]
    [SelectOne(SelectionFactoryType = typeof(ChangeFrequencyFactory))]
    public virtual string ChangeFrequency
    {
        get
        {
            string value = this.GetPropertyValue(block => block.ChangeFrequency);
            return !string.IsNullOrEmpty(value) ? value : "weekly";
        }
        set { }
    }

    [CultureSpecific]
    [Display(Order = 30)]
    [SelectOne(SelectionFactoryType = typeof(PriorityFactory))]
    public virtual string Priority
    {
        get
        {
            string value = this.GetPropertyValue(block => block.Priority);
            return !string.IsNullOrEmpty(value) ? value : "0.5";
        }
        set { }
    }

    #endregion
}

public class ChangeFrequencyFactory : ISelectionFactory
{
    public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
    {
        var numberList = new List<SelectItem>
            {
                new SelectItem {Value = "always", Text = "Always"},
                new SelectItem {Value = "hourly", Text = "Hourly" },
                new SelectItem {Value = "daily", Text = "Daily" },
                new SelectItem {Value = "weekly", Text = "Weekly" },
                new SelectItem {Value = "monthly", Text = "Monthly" },
                new SelectItem {Value = "yearly", Text = "Yearly" },
                new SelectItem {Value = "never", Text = "Never" }
            };

        return numberList;
    }
}

public class PriorityFactory : ISelectionFactory
{
    public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
    {
        var numberList = new List<SelectItem>
            {
                new SelectItem {Value = "0.0", Text = "0.0"},
                new SelectItem {Value = "0.25", Text = "0.25" },
                new SelectItem {Value = "0.5", Text = "0.5" },
                new SelectItem {Value = "0.75", Text = "0.75" },
                new SelectItem {Value = "1.0", Text = "1.0" }
            };

        return numberList;
    }
}
