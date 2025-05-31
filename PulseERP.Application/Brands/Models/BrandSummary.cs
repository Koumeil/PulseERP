// Models/BrandSummary.cs
namespace PulseERP.Application.Brands.Models;

public sealed record BrandSummary(Guid Id, string Name, bool IsActive, List<Guid> ProductIds);
