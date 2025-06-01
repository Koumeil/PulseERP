using AutoMapper;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Products.Models;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Mapping global Money → decimal
        CreateMap<Money, decimal>().ConvertUsing(src => src.Value);

        // Domain → ProductSummary
        CreateMap<Product, ProductSummary>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description != null ? src.Description.Value : null)
            )
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        // Domain → ProductDetails
        CreateMap<Product, ProductDetails>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description != null ? src.Description.Value : null)
            )
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // PagedResult mappings
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

        CreateMap<PagedResult<Product>, PagedResult<ProductDetails>>()
            .ConvertUsing(
                (src, dest, ctx) =>
                    new PagedResult<ProductDetails>
                    {
                        Items = ctx.Mapper.Map<List<ProductDetails>>(src.Items),
                        PageNumber = src.PageNumber,
                        PageSize = src.PageSize,
                        TotalItems = src.TotalItems,
                    }
            );
    }
}
