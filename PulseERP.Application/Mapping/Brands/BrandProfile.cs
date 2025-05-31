// Application/Mapping/Brands/BrandProfile.cs
using AutoMapper;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Brands.Commands;
using PulseERP.Application.Brands.Models;
using PulseERP.Application.Common.Models;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Brands;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        // Domain → DTO
        CreateMap<Brand, BrandSummary>()
            .ConstructUsing(src => new BrandSummary(
                src.Id,
                src.Name,
                src.IsActive,
                src.Products.Select(p => p.Id).ToList()
            ));

        // CreateBrandRequest → Domain
        CreateMap<CreateBrandCommand, Brand>().ConstructUsing(cmd => Brand.Create(cmd.Name));

        // PaginationResult<Brand> → PaginationResult<BrandDto>
        CreateMap<PagedResult<Brand>, PagedResult<BrandSummary>>()
            .ConvertUsing(
                (src, dest, ctx) =>
                    new PagedResult<BrandSummary>
                    {
                        Items = ctx.Mapper.Map<List<BrandSummary>>(src.Items),
                        PageNumber = src.PageNumber,
                        PageSize = src.PageSize,
                        TotalItems = src.TotalItems,
                    }
            );
    }
}
