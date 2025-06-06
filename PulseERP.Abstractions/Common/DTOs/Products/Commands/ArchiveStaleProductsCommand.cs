namespace PulseERP.Abstractions.Common.DTOs.Products.Commands;

public class ArchiveStaleProductsCommand
{
    public int InactivityDays { get; set; }
    public int OutOfStockDays { get; set; }
}
