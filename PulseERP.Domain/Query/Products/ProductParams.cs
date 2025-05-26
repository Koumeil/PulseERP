namespace PulseERP.Domain.Query.Products;

public class ProductParams
{
    public string? Brand { get; set; }
    public string? Sort { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
