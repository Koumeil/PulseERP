using AutoMapper;
using PulseERP.Application.DTOs.Products;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Value));

        CreateMap<CreateProductCommand, Product>()
            .ConstructUsing(cmd =>
                Product.Create(cmd.Name, cmd.Description, cmd.Price, cmd.Quantity, cmd.IsService)
            );
    }
}
