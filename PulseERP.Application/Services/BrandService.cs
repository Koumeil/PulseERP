using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Brands.Commands;
using PulseERP.Application.Brands.Models;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Application.Services;

/// <summary>
/// Provides CRUD operations and business logic for brands.
/// </summary>
public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<BrandService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrandService"/> class.
    /// </summary>
    /// <param name="repo">Brand repository dependency.</param>
    /// <param name="mapper">Mapper for DTO conversions.</param>
    /// <param name="logger">Logger for audit and diagnostics.</param>
    public BrandService(IBrandRepository repo, IMapper mapper, ILogger<BrandService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new brand. Throws <see cref="ConflictException"/> if name is not unique.
    /// </summary>
    /// <param name="command">The command containing brand details.</param>
    /// <returns>Summary of created brand.</returns>
    /// <exception cref="ConflictException">Thrown if brand name already exists.</exception>
    public async Task<BrandSummary> CreateAsync(CreateBrandCommand command)
    {
        _logger.LogInformation("Attempting to create brand: {BrandName}", command.Name);

        if (await _repo.ExistsByNameAsync(command.Name))
        {
            var msg = $"Brand creation failed: name '{command.Name}' already exists.";
            _logger.LogWarning(msg);
            throw new ConflictException(msg);
        }

        var brand = Brand.Create(command.Name);
        await _repo.AddAsync(brand);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Brand '{BrandName}' created successfully (Id: {BrandId})",
            brand.Name,
            brand.Id
        );
        return _mapper.Map<BrandSummary>(brand);
    }

    /// <summary>
    /// Updates the name of an existing brand.
    /// </summary>
    /// <param name="id">Brand identifier.</param>
    /// <param name="command">Update command with the new name.</param>
    /// <returns>Summary of updated brand.</returns>
    /// <exception cref="NotFoundException">If brand is not found.</exception>
    /// <exception cref="ConflictException">If new name is already taken.</exception>
    public async Task<BrandSummary> UpdateAsync(Guid id, UpdateBrandCommand command)
    {
        _logger.LogInformation(
            "Attempting to update brand (Id: {BrandId}) with new name: {BrandName}",
            id,
            command.Name
        );

        var brand = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Brand", id);

        if (await _repo.ExistsByNameAsync(command.Name, excludeId: id))
        {
            var msg =
                $"Brand update failed: name '{command.Name}' already exists for another brand.";
            _logger.LogWarning(msg);
            throw new ConflictException(msg);
        }

        brand.UpdateName(command.Name);
        await _repo.UpdateAsync(brand);
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "Brand (Id: {BrandId}) renamed to '{BrandName}' successfully",
            id,
            brand.Name
        );
        return _mapper.Map<BrandSummary>(brand);
    }

    /// <summary>
    /// Deactivates a brand (soft delete).
    /// </summary>
    /// <param name="id">Brand identifier.</param>
    /// <exception cref="NotFoundException">If brand is not found.</exception>
    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Attempting to deactivate brand (Id: {BrandId})", id);

        var brand = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Brand", id);

        brand.Deactivate();
        await _repo.UpdateAsync(brand);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Brand (Id: {BrandId}) deactivated successfully", id);
    }

    /// <summary>
    /// Gets brand summary by id.
    /// </summary>
    /// <param name="id">Brand identifier.</param>
    /// <returns>Summary of the brand.</returns>
    /// <exception cref="NotFoundException">If brand is not found.</exception>
    public async Task<BrandSummary> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Fetching brand by id: {BrandId}", id);

        var brand = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Brand", id);

        return _mapper.Map<BrandSummary>(brand);
    }

    /// <summary>
    /// Gets all brands, paged.
    /// </summary>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>Paged result of brand summaries.</returns>
    public async Task<PagedResult<BrandSummary>> GetAllAsync(PaginationParams pagination)
    {
        _logger.LogDebug(
            "Fetching paginated brands: page {PageNumber}, size {PageSize}",
            pagination.PageNumber,
            pagination.PageSize
        );

        var paged = await _repo.GetAllAsync(pagination);

        // Mapping entities to DTOs
        var items = _mapper.Map<List<BrandSummary>>(paged.Items);

        return new PagedResult<BrandSummary>
        {
            Items = items,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalItems = paged.TotalItems,
        };
    }
}
