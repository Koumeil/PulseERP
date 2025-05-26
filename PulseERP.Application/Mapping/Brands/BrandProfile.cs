using AutoMapper;
using PulseERP.Contracts.Dtos.Brands;
using PulseERP.Domain.Entities;

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
