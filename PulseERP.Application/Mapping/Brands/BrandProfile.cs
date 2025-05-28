using AutoMapper;
using PulseERP.Domain.Entities;
using PulseERP.Shared.Dtos.Brands;

namespace PulseERP.Application.Mapping.Brands;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        // Domain → DTO
        CreateMap<Brand, BrandDto>().ConstructUsing(src => new BrandDto(src.Id, src.Name));

        // Command → Domain
        CreateMap<CreateBrandDto, Brand>().ConstructUsing(cmd => Brand.Create(cmd.Name));
    }
}
