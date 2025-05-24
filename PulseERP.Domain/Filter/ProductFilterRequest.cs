namespace PulseERP.Domain.Filter;

public class ProductFilterRequest
{
    public string? Name { get; set; }
    public bool? IsService { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
