// Application/Mapping/Products/ProductProfile.cs
using AutoMapper;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Products.Models;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Domain → DTO

        CreateMap<Product, ProductSummary>()
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
        CreateMap<PagedResult<Product>, PagedResult<ProductSummary>>()
            .ConvertUsing(
                (src, dest, ctx) =>
                    new PagedResult<ProductSummary>
                    {
                        Items = ctx.Mapper.Map<List<ProductSummary>>(src.Items),
                        PageNumber = src.PageNumber,
                        PageSize = src.PageSize,
                        TotalItems = src.TotalItems,
                    }
            );
    }
}
