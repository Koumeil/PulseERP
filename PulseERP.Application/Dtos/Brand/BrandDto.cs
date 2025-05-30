using System;

namespace PulseERP.Application.Dtos.Brand;

public record BrandDto(Guid Id, string Name, bool IsActive, List<Guid> ProductIds);
