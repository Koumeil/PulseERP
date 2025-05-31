using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Common;
using PulseERP.Application.Dtos.Product;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

namespace PulseERP.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IBrandRepository brandRepository,
        IMapper mapper,
        ILogger<ProductService> logger
    )
    {
        _repository = repository;
        _brandRepository = brandRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginationResult<ProductDto>> GetAllAsync(
        PaginationParams pagination,
        ProductParams productParams
    )
    {
        var pagedProducts = await _repository.GetAllAsync(pagination, productParams);
        var dtos = _mapper.Map<List<ProductDto>>(pagedProducts.Items);
        return new PaginationResult<ProductDto>(
            dtos,
            pagedProducts.TotalItems,
            pagedProducts.PageSize,
            pagedProducts.PageNumber
        );
    }

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return ServiceResult<ProductDto>.Fail("Product.NotFound", "Produit introuvable");

        return ServiceResult<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request)
    {
        try
        {
            var brand = await _brandRepository.GetByNameAsync(request.Brand);

            if (brand is null)
            {
                brand = Brand.Create(request.Brand);
                await _brandRepository.AddAsync(brand);
                await _brandRepository.SaveChangesAsync();
            }

            var product = Product.Create(
                request.Name,
                request.Description,
                brand,
                request.Price,
                request.Quantity,
                request.IsService
            );

            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Product {ProductName} created with ID {ProductId}",
                product.Name.Value,
                product.Id
            );

            return ServiceResult<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error on create product: {Error}", ex.Message);
            return ServiceResult<ProductDto>.Fail("DomainError", ex.Message);
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return ServiceResult<ProductDto>.Fail("Product.NotFound", "Produit introuvable");

        Brand? brand = product.Brand;
        if (request.BrandId.HasValue)
        {
            brand = await _brandRepository.GetByIdAsync(request.BrandId.Value);
            if (brand is null)
                return ServiceResult<ProductDto>.Fail("Brand.NotFound", "Marque introuvable");
        }

        product.UpdateDetails(
            name: request.Name ?? product.Name.Value,
            description: request.Description,
            brand: brand,
            price: request.Price ?? product.Price.Value,
            isService: request.IsService ?? product.IsService
        );

        if (request.Quantity.HasValue)
        {
            int quantityDiff = request.Quantity.Value - product.Quantity;
            if (quantityDiff > 0)
                product.IncreaseStock(quantityDiff);
            else if (quantityDiff < 0)
                product.DecreaseStock(-quantityDiff);
        }

        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} updated", product.Id);

        return ServiceResult<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return ServiceResult<bool>.Fail("Product.NotFound", "Produit introuvable");

        product.Discontinue();

        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} discontinued (deleted)", product.Id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult> ActivateAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return ServiceResult.Fail("Product.NotFound", "Produit introuvable");

        product.Activate();
        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} activated", product.Id);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeactivateAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return ServiceResult.Fail("Product.NotFound", "Produit introuvable");

        product.Deactivate();
        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deactivated", product.Id);

        return ServiceResult.Ok();
    }
}

    
