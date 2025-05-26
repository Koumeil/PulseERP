using AutoMapper;
using PulseERP.Contracts.Dtos.Products;
using PulseERP.Domain.Entities;

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
    }
}
