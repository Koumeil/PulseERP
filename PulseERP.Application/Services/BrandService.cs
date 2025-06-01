using AutoMapper;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Brands.Commands;
using PulseERP.Application.Brands.Models;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;

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

    public async Task<BrandSummary> CreateAsync(CreateBrandCommand command)
    {
        var brand = Brand.Create(command.Name);
        await _repo.AddAsync(brand);
        await _repo.SaveChangesAsync();
        return _mapper.Map<BrandSummary>(brand);
    }

    public async Task<BrandSummary> UpdateAsync(Guid id, UpdateBrandCommand command)
    {
        var brand = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Brand", id);

        brand.UpdateName(command.Name);
        await _repo.UpdateAsync(brand);
        await _repo.SaveChangesAsync();

        return _mapper.Map<BrandSummary>(brand);
    }

    public async Task DeleteAsync(Guid id)
    {
        var brand = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Brand", id);
        brand.Deactivate();
        await _repo.UpdateAsync(brand);
        await _repo.SaveChangesAsync();
    }

    public async Task<BrandSummary> GetByIdAsync(Guid id)
    {
        var brand = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Brand", id);
        return _mapper.Map<BrandSummary>(brand);
    }

    public async Task<PagedResult<BrandSummary>> GetAllAsync(PaginationParams pagination)
    {
        var paged = await _repo.GetAllAsync(pagination);

        // mapping des entités -> DTO
        var items = _mapper.Map<List<BrandSummary>>(paged.Items);

        // initialisation par propriétés
        return new PagedResult<BrandSummary>
        {
            Items = items,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalItems = paged.TotalItems,
        };
    }
}
