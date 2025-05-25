using AutoMapper;
using PulseERP.Contracts.Dtos.Products;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Filters.Products;
using PulseERP.Domain.Repositories;

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
                Brand.Create(command.Brand),
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

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(ProductParams productParams)
    {
        var products = await _repository.GetAllAsync(productParams);
        var productsDtos = products.Select(p => _mapper.Map<ProductDto>(p)).ToList().AsReadOnly();
        return Result<IReadOnlyList<ProductDto>>.Success(productsDtos);
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
