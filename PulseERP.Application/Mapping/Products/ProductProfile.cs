// Application/Mapping/Products/ProductProfile.cs
using AutoMapper;
using PulseERP.Application.Dtos.Product;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Mapping.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Domain → DTO
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Value))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // PaginationResult<Product> → PaginationResult<ProductDto>
        CreateMap<PaginationResult<Product>, PaginationResult<ProductDto>>()
            .ConvertUsing(
                (src, dest, ctx) =>
                    new PaginationResult<ProductDto>(
                        ctx.Mapper.Map<List<ProductDto>>(src.Items),
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    )
            );
    }
}
