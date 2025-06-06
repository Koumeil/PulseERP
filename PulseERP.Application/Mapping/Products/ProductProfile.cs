using AutoMapper;
using PulseERP.Abstractions.Common.DTOs.Products.Models;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Mapping.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Convert Money → decimal automatiquement
        CreateMap<Money, decimal>().ConvertUsing(src => src.Amount);

        // Mapping Product → ProductSummary
        CreateMap<Product, ProductSummary>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description != null ? src.Description.Value : null)
            )
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Inventory.Quantity));

        // Mapping Product → ProductDetails
        // Mapping Product → ProductDetails
        CreateMap<Product, ProductDetails>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description != null ? src.Description.Value : null)
            )
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount)) // Change to only map the Amount if necessary
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Inventory.Quantity))
            .ForMember(dest => dest.LastSoldAt, opt => opt.MapFrom(src => src.LastSoldAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // CreateMap<Product, ProductDetails>()
        //     .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value))
        //     .ForMember(
        //         dest => dest.Description,
        //         opt => opt.MapFrom(src => src.Description != null ? src.Description.Value : null)
        //     )
        //     .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
        //     .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
        //     .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        //     .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Inventory.Quantity));

        // Pagination mapping
        CreateMap<PagedResult<Product>, PagedResult<ProductSummary>>()
            .ConvertUsing(
                (src, _, ctx) =>
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
                (src, _, ctx) =>
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
