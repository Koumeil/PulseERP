namespace PulseERP.Tests.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PulseERP.Abstractions.Common.DTOs.Products.Commands;
using PulseERP.Abstractions.Common.DTOs.Products.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Application.Mapping.Products;
using PulseERP.Application.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;
using Xunit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        // Mock des dépendances
        _productRepositoryMock = new Mock<IProductRepository>();
        _brandRepositoryMock = new Mock<IBrandRepository>();

        // Configurer AutoMapper avec les profils nécessaires
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProductProfile()); // Remplacez par votre profil réel de mappage
        });

        _mapper = configuration.CreateMapper();
        _logger = new Mock<ILogger<ProductService>>().Object;

        // Injection des mocks dans le ProductService
        _productService = new ProductService(
            _productRepositoryMock.Object,
            _brandRepositoryMock.Object,
            _mapper,
            _logger
        );
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnPagedProducts_WhenFilterIsValid()
    {
        var filter = new ProductFilter();
        var productList = new List<Product>
        {
            new Product(
                new ProductName("Product 1"),
                new ProductDescription("Description 1"),
                new Brand("Brand 1"),
                new Money(10, new Currency("USD")),
                10,
                false
            ),
            new Product(
                new ProductName("Product 2"),
                new ProductDescription("Description 2"),
                new Brand("Brand 2"),
                new Money(20, new Currency("USD")),
                20,
                true
            ),
        };

        var productSummaryList = new List<ProductSummary>
        {
            new ProductSummary { Name = "Product 1", Price = 10 },
            new ProductSummary { Name = "Product 2", Price = 20 },
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
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        var productId = Guid.NewGuid();
        var product = new Product(
            new ProductName("Test Product"),
            new ProductDescription("Test Description"),
            new Brand("Brand A"),
            new Money(10, new Currency("USD")),
            10,
            false
        );
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

        // Le message attendu correspond au format de la NotFoundException
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

        var product = new Product(
            new ProductName(command.Name),
            new ProductDescription(command.Description),
            brand,
            new Money(command.Price, new Currency("USD")),
            command.Quantity,
            command.IsService
        );

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
        var product = new Product(
            new ProductName("Old Product"),
            new ProductDescription("Old Description"),
            new Brand("Brand A"),
            new Money(10, new Currency("USD")),
            10,
            false
        );

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Verifiable();
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

        // Le message attendu correspond au format de la NotFoundException
        Assert.StartsWith($"Entity \"Product\" ({productId}) was not found", exception.Message);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct_WhenProductExists()
    {
        var productId = Guid.NewGuid();
        var product = new Product(
            new ProductName("Test Product"),
            new ProductDescription("Test Description"),
            new Brand("Brand A"),
            new Money(10, new Currency("USD")),
            10,
            false
        );

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

        // Le message attendu correspond au format de la NotFoundException
        Assert.StartsWith($"Entity \"Product\" ({productId}) was not found", exception.Message);
    }

    [Fact]
    public async Task RestockProductAsync_ShouldRestockProduct_WhenValidQuantity()
    {
        var productId = Guid.NewGuid();
        var quantity = 10;
        var product = new Product(
            new ProductName("Test Product"),
            new ProductDescription("Test Description"),
            new Brand("Brand A"),
            new Money(10, new Currency("USD")),
            10,
            false
        );

        _productRepositoryMock.Setup(x => x.FindByIdAsync(productId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Verifiable();
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        await _productService.RestockProductAsync(productId, quantity);

        _productRepositoryMock.Verify();
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

    [Fact]
    public async Task CreateProductAsync_ShouldThrowException_WhenBrandIsNullAndNullReferenceIsRaised()
    {
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            null!,
            10,
            10,
            false
        );

        var exception = await Assert.ThrowsAsync<DomainValidationException>(() =>
            _productService.CreateProductAsync(command)
        );

        Assert.Equal("Brand is required.", exception.Message);
    }
}
