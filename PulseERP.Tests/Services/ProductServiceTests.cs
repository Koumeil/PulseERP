using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PulseERP.Abstractions.Common.DTOs.Products.Commands;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Application.Mapping.Products;
using PulseERP.Application.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _brandRepositoryMock = new Mock<IBrandRepository>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProductProfile());
        });

        var mapper = configuration.CreateMapper();
        var logger = new Mock<ILogger<ProductService>>().Object;

        _productService = new ProductService(
            _productRepositoryMock.Object,
            _brandRepositoryMock.Object,
            mapper,
            logger
        );
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnPagedProducts_WhenFilterIsValid()
    {
        var filter = new ProductFilter();
        var productList = new List<Product>
        {
            CreateTestProduct("Product 1", "Description 1", "Brand 1"),
            CreateTestProduct("Product 2", "Description 2", "Brand 2", 20)
        };

        var pagedResult = new PagedResult<Product>
        {
            Items = productList,
            TotalItems = 2,
            PageNumber = 1,
            PageSize = 10,
        };

        _productRepositoryMock.Setup(x => x.GetAllAsync(filter)).ReturnsAsync(pagedResult);

        var result = await _productService.GetAllProductsAsync(filter);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.Items.Count);

        Assert.Contains(result.Items, p => p.Name == "Product 1" && p.Price == 10);
        Assert.Contains(result.Items, p => p.Name == "Product 2" && p.Price == 20);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        var productId = Guid.NewGuid();
        var product = CreateTestProduct();

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync(product);

        var result = await _productService.GetProductByIdAsync(productId);

        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(10, result.Price);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync((Product?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _productService.GetProductByIdAsync(productId)
        );

        Assert.StartsWith($"Entity \"Product\" ({productId}) was not found", exception.Message);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct_WhenValidCommand()
    {
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            "Brand A",
            10,
            10,
            false
        );

        var brand = new Brand("Brand A");
        _brandRepositoryMock.Setup(x => x.FindByNameAsync(command.BrandName)).ReturnsAsync(brand);

        _productRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>())).Verifiable();
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _productService.CreateProductAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        _productRepositoryMock.Verify();
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrowException_WhenBrandIsNull()
    {
        var command = new CreateProductCommand("", "", null!, 10, 10, false);

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _productService.CreateProductAsync(command)
        );

        Assert.Equal("Brand is required.", exception.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct_WhenValidCommand()
    {
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Name = "Updated Product",
            Price = 15,
            IsService = true,
        };

        var product = CreateTestProduct("Old Product", "Old Description");

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _productService.UpdateProductAsync(productId, command);

        Assert.NotNull(result);
        Assert.Equal("Updated Product", result.Name);
        _productRepositoryMock.Verify();
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand { Name = "Updated Product", Price = 15 };

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync((Product?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _productService.UpdateProductAsync(productId, command)
        );

        Assert.StartsWith($"Entity \"Product\" ({productId}) was not found", exception.Message);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct_WhenProductExists()
    {
        var productId = Guid.NewGuid();
        var product = CreateTestProduct();

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Verifiable();
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        await _productService.DeleteProductAsync(productId);

        _productRepositoryMock.Verify();
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync((Product?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _productService.DeleteProductAsync(productId)
        );

        Assert.StartsWith($"Entity \"Product\" ({productId}) was not found", exception.Message);
    }

    [Fact]
    public async Task RestockProductAsync_ShouldRestockProduct_WhenValidQuantity()
    {
        var productId = Guid.NewGuid();
        var quantity = 10;
        var product = CreateTestProduct();

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Verifiable();
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        await _productService.RestockProductAsync(productId, quantity);

        _productRepositoryMock.Verify();
    }

    [Fact]
    public async Task RestockProductAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        var productId = Guid.NewGuid();
        var quantity = 10;

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync((Product?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _productService.RestockProductAsync(productId, quantity)
        );

        Assert.StartsWith($"Entity \"Product\" ({productId}) was not found", exception.Message);
    }

    [Fact]
    public async Task RestockProductAsync_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        var productId = Guid.NewGuid();
        var quantity = -1;

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _productService.RestockProductAsync(productId, quantity)
        );

        Assert.Equal("Quantity must be greater than 0 for restock.", exception.Message);
    }

    private Product CreateTestProduct(
        string name = "Test Product",
        string description = "Test Description",
        string brandName = "Brand A",
        decimal price = 10,
        int qty = 10,
        bool isService = false)
    {
        return new Product(
            new ProductName(name),
            new ProductDescription(description),
            new Brand(brandName),
            new Money(price, new Currency("USD")),
            qty,
            isService
        );
    }
}
