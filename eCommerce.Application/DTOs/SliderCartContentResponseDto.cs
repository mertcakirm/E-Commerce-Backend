namespace eCommerce.Application.DTOs;

public class SliderContentResponseDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public string ParentName { get; set; }
    public string Name { get; set; }
    public string SubName { get; set; }
    public string Href { get; set; }

}

public class CartContentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Href { get; set; }
    public string CartSize { get; set; }
    public string ImageUrl { get; set; }
}
