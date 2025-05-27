using AutoMapper;
using PulseERP.Contracts.Dtos.Products;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Mapping.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Value));

        CreateMap<CreateProductRequest, Product>()
            .ConstructUsing(cmd =>
                Product.Create(
                    cmd.Name,
                    cmd.Description,
                    Brand.Create(cmd.Brand),
                    cmd.Price,
                    cmd.Quantity,
                    cmd.IsService
                )
            );

        // Mapping PaginationResult<Product> -> PaginationResult<ProductDto> avec ConvertUsing
        CreateMap<PaginationResult<Product>, PaginationResult<ProductDto>>()
            .ConvertUsing(
                (src, dest, context) =>
                {
                    var mappedItems = context.Mapper.Map<List<ProductDto>>(src.Items);
                    return new PaginationResult<ProductDto>(
                        mappedItems,
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    );
                }
            );
        // // Mapping PaginationResult<Product> -> PaginationResult<ProductDto>
        // CreateMap<PaginationResult<Product>, PaginationResult<ProductDto>>()
        //     .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
        //     .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
        //     .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
        //     .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems));
    }
}
