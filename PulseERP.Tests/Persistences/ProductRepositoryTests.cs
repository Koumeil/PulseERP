namespace PulseERP.Tests.Persistences;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Product;
using PulseERP.Domain.VO;
using PulseERP.Infrastructure.Database;
using PulseERP.Infrastructure.Repositories;
using Xunit;

public class ProductRepositoryTests
{
    private readonly ProductRepository _repository;
    private readonly CoreDbContext _dbContext;
    private readonly Mock<ILogger<ProductRepository>> _mockLogger;

    public ProductRepositoryTests()
    {
        // Create in-memory database context
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new CoreDbContext(options); // Actual DbContext with in-memory DB
        _mockLogger = new Mock<ILogger<ProductRepository>>();

        // Initialize repository with actual DbContext
        _repository = new ProductRepository(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResults_WhenFiltersAreApplied()
    {
        // Arrange
        var filter = new ProductFilter
        {
            Search = "Test",
            MinPrice = 10,
            MaxPrice = 100,
            PageNumber = 1,
            PageSize = 10,
        };

        var mockProducts = new List<Product>
        {
            new Product(
                new ProductName("Test Product 1"),
                new ProductDescription("Description"),
                new Brand("Brand A"),
                new Money(50, new Currency("USD")),
                10,
                false
            ),
            new Product(
                new ProductName("Test Product 2"),
                new ProductDescription("Description"),
                new Brand("Brand B"),
                new Money(60, new Currency("USD")),
                20,
                true
            ),
        };

        await _dbContext.Products.AddRangeAsync(mockProducts);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(filter);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var mockProduct = new Product(
            new ProductName("Test Product"),
            new ProductDescription("Test Description"),
            new Brand("Brand A"),
            new Money(50, new Currency("USD")),
            10,
            false
        );
        typeof(Product)
            .GetProperty(
                "Id",
                System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic
            )
            ?.SetValue(mockProduct, productId);

        await _dbContext.Products.AddAsync(mockProduct);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProductToContext()
    {
        // Arrange
        var product = new Product(
            new ProductName("Test Product"),
            new ProductDescription("Description"),
            new Brand("Brand A"),
            new Money(50, new Currency("USD")),
            10,
            false
        );

        // Act
        await _repository.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Assert
        var addedProduct = await _dbContext.Products.FindAsync(product.Id);
        Assert.NotNull(addedProduct);
        Assert.Equal(product.Name, addedProduct.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProductInContext()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Create the product using the constructor
        var product = new Product(
            new ProductName("Test Product"),
            new ProductDescription("Description"),
            new Brand("Brand A"),
            new Money(50, new Currency("USD")),
            10,
            false
        );
        typeof(Product)
            .GetProperty(
                "Id",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            )
            ?.SetValue(product, productId);

        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act
        // Update the product's name using the UpdateDetails method
        product.UpdateDetails(name: new ProductName("Updated Product"));
        await _repository.UpdateAsync(product);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedProduct = await _dbContext.Products.FindAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Product", updatedProduct.Name.Value);
        Assert.Equal(ProductAvailabilityStatus.InStock, updatedProduct.Status);
    }

    // [Fact]
    // public async Task UpdateAsync_ShouldUpdateProductInContext()
    // {
    //     // Arrange
    //     var product = new Product(
    //         new ProductName("Test Product"),
    //         new ProductDescription("Description"),
    //         new Brand("Brand A"),
    //         new Money(50, new Currency("USD")),
    //         10,
    //         false
    //     );
    //     var productId = Guid.NewGuid();
    //     typeof(Product)
    //         .GetProperty(
    //             "Id",
    //             System.Reflection.BindingFlags.Instance
    //                 | System.Reflection.BindingFlags.Public
    //                 | System.Reflection.BindingFlags.NonPublic
    //         )
    //         ?.SetValue(product, productId);

    //     await _dbContext.Products.AddAsync(product);
    //     await _dbContext.SaveChangesAsync();

    //     // Act
    //     var productName = new ProductName("Updated Product");
    //     product.UpdateDetails(productName);
    //     await _repository.UpdateAsync(product);
    //     await _dbContext.SaveChangesAsync();

    //     // Assert
    //     var updatedProduct = await _dbContext.Products.FindAsync(product.Id);
    //     Assert.Equal("Updated Product", updatedProduct!.Name.Value);
    // }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProductFromContext()
    {
        // Arrange
        var product = new Product(
            new ProductName("Test Product"),
            new ProductDescription("Description"),
            new Brand("Brand A"),
            new Money(50, new Currency("USD")),
            10,
            false
        );
        typeof(Product)
            .GetProperty(
                "Id",
                System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic
            )
            ?.SetValue(product, Guid.NewGuid());

        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(product);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedProduct = await _dbContext.Products.FindAsync(product.Id);
        Assert.Null(deletedProduct);
    }
}
