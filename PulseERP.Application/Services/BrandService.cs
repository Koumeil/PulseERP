using AutoMapper;
using PulseERP.Application.Common;
using PulseERP.Application.Dtos.Brand;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;
    private readonly IMapper _mapper;

    public BrandService(IBrandRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<BrandDto>> CreateAsync(CreateBrandRequest request)
    {
        var brand = Brand.Create(request.Name);
        await _repo.AddAsync(brand);
        await _repo.SaveChangesAsync();
        return ServiceResult<BrandDto>.Ok(_mapper.Map<BrandDto>(brand));
    }

    public async Task<ServiceResult<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest request)
    {
        var brand = await _repo.GetByIdAsync(id);
        if (brand is null)
            return ServiceResult<BrandDto>.Fail("Brand.NotFound", "Marque introuvable");

        brand.UpdateName(request.Name);
        await _repo.UpdateAsync(brand);
        await _repo.SaveChangesAsync();

        return ServiceResult<BrandDto>.Ok(_mapper.Map<BrandDto>(brand));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        var brand = await _repo.GetByIdAsync(id);
        if (brand is null)
            return ServiceResult<bool>.Fail("Brand.NotFound", "Marque introuvable");

        // soft-delete, ou .Deactivate() si tu préfères garder l’historique
        brand.Deactivate();
        await _repo.UpdateAsync(brand);
        await _repo.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<BrandDto>> GetByIdAsync(Guid id)
    {
        var brand = await _repo.GetByIdAsync(id);
        if (brand is null)
            return ServiceResult<BrandDto>.Fail("Brand.NotFound", "Marque introuvable");

        return ServiceResult<BrandDto>.Ok(_mapper.Map<BrandDto>(brand));
    }

    public async Task<PaginationResult<BrandDto>> GetAllAsync(PaginationParams pagination)
    {
        var paged = await _repo.GetAllAsync(pagination);
        var dtos = _mapper.Map<List<BrandDto>>(paged.Items);
        return new PaginationResult<BrandDto>(
            dtos,
            paged.TotalItems,
            paged.PageSize,
            paged.PageNumber
        );
    }
}
