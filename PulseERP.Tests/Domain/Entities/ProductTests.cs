using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Product;
using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldThrowException_WhenBrandIsNull()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        Brand? brand = null;

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Product(name, null, brand!, price, quantity, false)
        );
        Assert.Equal("Brand is required.", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPriceIsZero()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(0, currency); // Zero price
        var quantity = 10;

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Product(name, null, brand, price, quantity, false)
        );
        Assert.Equal("Price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenQuantityIsNegative()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = -1; // Negative quantity

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Product(name, null, brand, price, quantity, false)
        );
        Assert.Equal("Quantity cannot be negative.", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldCreateProduct_WhenValidParameters()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var description = new ProductDescription("Test Description");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 100;

        // Act
        var product = new Product(name, description, brand, price, quantity, false);

        // Assert
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(brand, product.Brand);
        Assert.Equal(price, product.Price);
        Assert.Equal(quantity, product.Inventory.Quantity);
        Assert.Equal(ProductAvailabilityStatus.InStock, product.Status); // Ensure status is set correctly
    }

    [Fact]
    public void SetPrice_ShouldThrowException_WhenPriceIsZero()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);

        var newPrice = new Money(0, currency); // Zero price

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => product.SetPrice(newPrice));
        Assert.Equal("Price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void SetPrice_ShouldUpdatePrice_WhenValidPrice()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);

        var newPrice = new Money(20, currency); // New valid price

        // Act
        product.SetPrice(newPrice);

        // Assert
        Assert.Equal(newPrice, product.Price);
    }

    [Fact]
    public void ApplyDiscount_ShouldThrowException_WhenDiscountIsInvalid()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);

        // Act & Assert
        Assert.Throws<DomainValidationException>(() => product.ApplyDiscount(0)); // Discount <= 0
        Assert.Throws<DomainValidationException>(() => product.ApplyDiscount(100)); // Discount >= 100
    }

    [Fact]
    public void ApplyDiscount_ShouldApplyDiscountCorrectly()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(100, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);
        var discount = 10; // 10% discount

        // Act
        product.ApplyDiscount(discount);

        // Assert
        var expectedPrice = price.Multiply(0.9m); // Price after 10% discount
        Assert.Equal(expectedPrice, product.Price);
    }

    [Fact]
    public void SetBrand_ShouldThrowException_WhenBrandIsNull()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);

        Brand? newBrand = null;

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => product.SetBrand(newBrand!));
        Assert.Equal("Brand is required.", exception.Message);
    }

    [Fact]
    public void SetBrand_ShouldUpdateBrand_WhenValidBrandIsSet()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);

        var newBrand = new Brand("Brand B");

        // Act
        product.SetBrand(newBrand);

        // Assert
        Assert.Equal(newBrand, product.Brand);
    }


    [Fact]
    public void SetQuantity_ShouldThrowException_WhenQuantityIsNegative()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => product.SetQuantity(-5));
        Assert.Equal("Quantity cannot be negative.", exception.Message);
    }

    [Fact]
    public void SetQuantity_ShouldMarkOutOfStock_WhenQuantityIsZero()
    {
        // Arrange
        var name = new ProductName("Test Product");
        var brand = new Brand("Brand A");
        var currency = new Currency("EUR");
        var price = new Money(10, currency);
        var quantity = 10;
        var product = new Product(name, null, brand, price, quantity, false);

        // Act
        product.SetQuantity(0);

        // Assert
        Assert.Equal(ProductAvailabilityStatus.OutOfStock, product.Status);
    }
}
