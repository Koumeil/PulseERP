// Application/Mapping/Brands/BrandProfile.cs
using AutoMapper;
using PulseERP.Application.Dtos.Brand;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Mapping.Brands;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        // Domain → DTO
        CreateMap<Brand, BrandDto>()
            .ConstructUsing(src => new BrandDto(
                src.Id,
                src.Name,
                src.IsActive,
                src.Products.Select(p => p.Id).ToList()
            ));

        // CreateBrandRequest → Domain
        CreateMap<CreateBrandRequest, Brand>().ConstructUsing(cmd => Brand.Create(cmd.Name));

        // PaginationResult<Brand> → PaginationResult<BrandDto>
        CreateMap<PaginationResult<Brand>, PaginationResult<BrandDto>>()
            .ConvertUsing(
                (src, dest, ctx) =>
                    new PaginationResult<BrandDto>(
                        ctx.Mapper.Map<List<BrandDto>>(src.Items),
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    )
            );
    }
}
