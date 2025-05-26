using AutoMapper;
using PulseERP.Contracts.Dtos.Products;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Interfaces.Services;
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

    public async Task<ServiceResult<Guid>> CreateAsync(CreateProductRequest command)
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

            return ServiceResult<Guid>.Success(product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create product", ex);
            return ServiceResult<Guid>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null
            ? ServiceResult<ProductDto>.Failure("Product not found")
            : ServiceResult<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<ServiceResult<PaginationResult<ProductDto>>> GetAllAsync(
        ProductParams productParams
    )
    {
        var pagedProducts = await _repository.GetAllAsync(productParams);

        var productsDtos = pagedProducts.Items.Select(p => _mapper.Map<ProductDto>(p)).ToList();

        var result = new PaginationResult<ProductDto>(
            productsDtos,
            pagedProducts.TotalItems,
            pagedProducts.PageNumber,
            pagedProducts.PageSize
        );

        return ServiceResult<PaginationResult<ProductDto>>.Success(result);
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest command)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is null)
                return ServiceResult<ProductDto>.Failure("Product not found");

            product.UpdateName(command.Name);
            product.UpdateDescription(command.Description);
            product.UpdatePrice(command.Price);
            product.UpdateQuantity(command.Quantity);
            product.UpdateBrand(command.Brand);

            await _repository.UpdateAsync(product);
            _logger.LogInformation($"Updated product {product.Id}");

            var productDto = _mapper.Map<ProductDto>(product);

            return ServiceResult<ProductDto>.Success(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update product {id}", ex);
            return ServiceResult<ProductDto>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is null)
                return ServiceResult.Failure("Product not found");

            await _repository.DeleteAsync(product);
            _logger.LogInformation($"Deleted product {product.Id}");

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete product {id}", ex);
            return ServiceResult.Failure(ex.Message);
        }
    }
}
