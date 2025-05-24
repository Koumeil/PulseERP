using AutoMapper;
using PulseERP.Application.Common.Interfaces;
using PulseERP.Application.DTOs.Products;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Persistence;
using PulseERP.Domain.Shared;

namespace PulseERP.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IAppLogger<ProductService> _logger;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository repository,
        IAppLogger<ProductService> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Guid>> CreateAsync(CreateProductCommand command)
    {
        try
        {
            var product = Product.Create(
                command.Name,
                command.Description,
                command.Price,
                command.Quantity,
                command.IsService
            );

            await _repository.AddAsync(product);
            _logger.LogInformation($"Created product {product.Id}");

            return Result<Guid>.Success(product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create product", ex);
            return Result<Guid>.Failure(ex.Message);
        }
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null
            ? Result<ProductDto>.Failure("Product not found")
            : Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();
        var dtos = products.Select(p => _mapper.Map<ProductDto>(p)).ToList().AsReadOnly();
        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateProductCommand command)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is null)
                return Result.Failure("Product not found");

            product.UpdateName(command.Name);
            product.UpdateDescription(command.Description);
            product.UpdatePrice(command.Price);
            product.UpdateQuantity(command.Quantity);

            await _repository.UpdateAsync(product);
            _logger.LogInformation($"Updated product {product.Id}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update product {command.Id}", ex);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is null)
                return Result.Failure("Product not found");

            await _repository.DeleteAsync(product);
            _logger.LogInformation($"Deleted product {product.Id}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete product {id}", ex);
            return Result.Failure(ex.Message);
        }
    }
}
