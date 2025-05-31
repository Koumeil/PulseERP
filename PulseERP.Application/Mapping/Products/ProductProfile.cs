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
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description != null ? src.Description.Value : null)
            )
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Value))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

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
