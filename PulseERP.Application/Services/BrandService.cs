using AutoMapper;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Shared.Dtos.Brands;

namespace PulseERP.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;

    public BrandService(IBrandRepository brandRepository, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
    }

    public async Task<BrandDto?> GetByIdAsync(Guid id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<PaginationResult<BrandDto>> GetAllAsync(PaginationParams paginationParams)
    {
        var brands = await _brandRepository.GetAllAsync(paginationParams);
        return _mapper.Map<PaginationResult<BrandDto>>(brands);
    }

    public async Task<BrandDto> CreateAsync(CreateBrandDto dto)
    {
        if (await _brandRepository.NameExistsAsync(dto.Name))
            throw new InvalidOperationException($"Brand with name '{dto.Name}' already exists.");

        var brand = _mapper.Map<Brand>(dto);
        var createdBrand = await _brandRepository.AddAsync(brand);
        return _mapper.Map<BrandDto>(createdBrand);
    }

    public async Task UpdateAsync(UpdateBrandDto dto)
    {
        var existingBrand =
            await _brandRepository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Brand with ID {dto.Id} not found.");

        if (existingBrand.Name != dto.Name && await _brandRepository.NameExistsAsync(dto.Name))
            throw new InvalidOperationException($"Brand with name '{dto.Name}' already exists.");

        _mapper.Map(dto, existingBrand);
        await _brandRepository.UpdateAsync(existingBrand);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await _brandRepository.ExistsAsync(id))
            throw new KeyNotFoundException($"Brand with ID {id} not found.");

        await _brandRepository.DeleteAsync(id);
    }

    public async Task<Brand> GetOrCreateAsync(string brandName)
    {
        var brand = await _brandRepository.GetByNameAsync(brandName);

        if (brand is null)
        {
            brand = Brand.Create(brandName);
            await _brandRepository.AddAsync(brand);
        }

        return brand;
    }
}
