namespace eCommerce.Core.Entities;

public class SliderContent : BaseEntity
{
    public string ImageUrl { get; set; }
    public string ParentName { get; set; }
    public string Name { get; set; }
    public string SubName { get; set; }
    public string Href { get; set; }
}