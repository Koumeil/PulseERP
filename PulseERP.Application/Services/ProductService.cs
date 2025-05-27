using AutoMapper;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Contracts.Dtos.Products;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

namespace PulseERP.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ISerilogAppLoggerService<ProductService> _logger;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository repository,
        ISerilogAppLoggerService<ProductService> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PaginationResult<ProductDto>> GetAllAsync(ProductParams productParams)
    {
        var pagedProducts = await _repository.GetAllAsync(productParams);
        var productsDtos = _mapper.Map<List<ProductDto>>(pagedProducts.Items);

        return new PaginationResult<ProductDto>(
            productsDtos,
            pagedProducts.TotalItems,
            pagedProducts.PageNumber,
            pagedProducts.PageSize
        );
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException($"Product with id '{id}' was not found.", id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest command)
    {
        var brand = Brand.Create(command.Brand);
        var product = Product.Create(
            command.Name,
            command.Description,
            brand,
            command.Price,
            command.Quantity,
            command.IsService
        );

        await _repository.AddAsync(product);
        _logger.LogInformation("Created product {ProductId}", product.Id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest command)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException($"Product with id '{id}' was not found.", id);

        product.UpdateDetails(
            command.Name,
            command.Description,
            command.Brand,
            command.Price,
            command.Quantity
        );

        await _repository.UpdateAsync(product);
        _logger.LogInformation("Updated product {ProductId}", product.Id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException($"Product with id '{id}' was not found.", id);

        await _repository.DeleteAsync(product);
        _logger.LogInformation("Deleted product {ProductId}", product.Id);
    }
}
