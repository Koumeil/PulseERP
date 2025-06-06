using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.DTOs.Brands.Commands;
using PulseERP.Abstractions.Common.DTOs.Brands.Models;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;

namespace PulseERP.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<BrandService> _logger;

    public BrandService(IBrandRepository repo, IMapper mapper, ILogger<BrandService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<BrandSummary>> GetAllBrandsAsync(PaginationParams pagination)
    {
        var result = await _repo.GetAllAsync(pagination);
        var brands = _mapper.Map<List<BrandSummary>>(result.Items);

        return new PagedResult<BrandSummary>
        {
            Items = brands,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
        };
    }

    public async Task<BrandSummary> FindBrandByIdAsync(Guid id)
    {
        var brand = await _repo.FindByIdAsync(id) ?? throw new NotFoundException("Brand", id);

        return _mapper.Map<BrandSummary>(brand);
    }

    public async Task<BrandSummary> FindBrandByNameAsync(string name)
    {
        var brand = await _repo.FindByNameAsync(name) ?? throw new NotFoundException("Brand", name);

        return _mapper.Map<BrandSummary>(brand);
    }

    public async Task<BrandSummary> CreateBrandAsync(CreateBrandCommand command)
    {
        var brand = new Brand(command.Name);

        await _repo.AddAsync(brand);
        await _repo.SaveChangesAsync();

        return _mapper.Map<BrandSummary>(brand);
    }

    public async Task<BrandSummary> UpdateBrandAsync(Guid id, UpdateBrandCommand command)
    {
        var brand = await _repo.FindByIdAsync(id) ?? throw new NotFoundException("Brand", id);

        brand.UpdateName(command.Name);

        await _repo.UpdateAsync(brand);
        await _repo.SaveChangesAsync();

        return _mapper.Map<BrandSummary>(brand);
    }

    public async Task DeleteBrandAsync(Guid id)
    {
        var brand = await _repo.FindByIdAsync(id) ?? throw new NotFoundException("Brand", id);

        if (brand.IsDeleted)
            throw new InvalidOperationException($"La marque ({id}) a déjà été supprimée.");

        await _repo.DeleteAsync(brand);
        await _repo.SaveChangesAsync();
    }
}
