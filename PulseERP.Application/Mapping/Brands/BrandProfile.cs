// Application/Mapping/Brands/BrandProfile.cs
using AutoMapper;
using PulseERP.Abstractions.Common.DTOs.Brands.Commands;
using PulseERP.Abstractions.Common.DTOs.Brands.Models;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Brands;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        // Domain → DTO
        // Configuration pour Brand → BrandSummary
        CreateMap<Brand, BrandSummary>()
            .ForMember(
                dest => dest.ProductIds,
                opt => opt.MapFrom(src => src.Products.Select(p => p.Id).ToList())
            );

        // CreateBrandRequest → Domain
        CreateMap<CreateBrandCommand, Brand>().ConstructUsing(cmd => new Brand(cmd.Name));

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
