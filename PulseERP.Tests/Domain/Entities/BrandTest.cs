using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Events.BrandEvents;
using PulseERP.Domain.VO;

public class BrandTests
{
    [Fact]
    public void Constructor_ValidName_ShouldSetNameAndRaiseEvent()
    {
        var brand = new Brand("  Acme Inc.  ");

        Assert.Equal("Acme Inc.", brand.Name);
        Assert.True(brand.IsActive);
        Assert.Single(brand.DomainEvents.OfType<BrandCreatedEvent>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_ShouldThrow(string? invalidName)
    {
        Assert.Throws<DomainValidationException>(() => new Brand(invalidName!));
    }

    [Fact]
    public void UpdateName_ValidNewName_ShouldUpdateAndRaiseEvent()
    {
        var brand = new Brand("Old Name");

        brand.UpdateName("  New Name  ");

        Assert.Equal("New Name", brand.Name);
        Assert.Contains(brand.DomainEvents, e => e is BrandNameUpdatedEvent);
        Assert.NotNull(brand.UpdatedAt);
    }

    [Fact]
    public void UpdateName_SameName_ShouldDoNothing()
    {
        var brand = new Brand("Same Name");

        brand.UpdateName("Same Name");

        Assert.DoesNotContain(brand.DomainEvents, e => e is BrandNameUpdatedEvent);
    }

    [Fact]
    public void AddProduct_ValidProduct_ShouldAddAndRaiseEvent()
    {
        var brand = new Brand("Acme");
        var product = TestHelpers.CreateSampleProduct(brand);

        brand.AddProduct(product);

        Assert.Contains(product, brand.Products);
        Assert.Contains(brand.DomainEvents, e => e is BrandProductAddedEvent);
    }

    [Fact]
    public void AddProduct_Null_ShouldThrow()
    {
        var brand = new Brand("Acme");
        Assert.Throws<DomainValidationException>(() => brand.AddProduct(null!));
    }

    [Fact]
    public void RemoveProduct_ValidProduct_ShouldRemoveAndRaiseEvent()
    {
        var brand = new Brand("Acme");
        var product = TestHelpers.CreateSampleProduct(brand);
        brand.AddProduct(product);
        brand.ClearDomainEvents(); // Reset events

        brand.RemoveProduct(product);

        Assert.DoesNotContain(product, brand.Products);
        Assert.Contains(brand.DomainEvents, e => e is BrandProductRemovedEvent);
    }

    [Fact]
    public void RemoveProduct_NotPresent_ShouldDoNothing()
    {
        var brand = new Brand("Acme");
        var product = TestHelpers.CreateSampleProduct(brand);

        brand.RemoveProduct(product); // not added before

        Assert.Empty(brand.DomainEvents.OfType<BrandProductRemovedEvent>());
    }

    [Fact]
    public void Delete_ShouldMarkDeletedAndRaiseEvent()
    {
        var brand = new Brand("Acme");

        brand.MarkAsDeleted();

        Assert.True(brand.IsDeleted);
        Assert.False(brand.IsActive);
        Assert.Contains(brand.DomainEvents, e => e is BrandDeletedEvent);
    }

    [Fact]
    public void Restore_ShouldClearDeletedAndRaiseEvent()
    {
        var brand = new Brand("Acme");
        brand.MarkAsDeleted();
        brand.ClearDomainEvents();
        brand.MarkAsRestored();

        Assert.False(brand.IsDeleted);
        Assert.True(brand.IsActive);
        Assert.Contains(brand.DomainEvents, e => e is BrandRestoredEvent);
    }

    [Fact]
    public void DeactivateBrand_ShouldMarkInactiveAndRaiseEvent()
    {
        var brand = new Brand("Acme");

        brand.MarkAsDeactivate();

        Assert.False(brand.IsActive);
        Assert.Contains(brand.DomainEvents, e => e is BrandDeactivatedEvent);
    }

    [Fact]
    public void ActivateBrand_ShouldMarkActiveAndRaiseEvent()
    {
        var brand = new Brand("Acme");
        brand.MarkAsDeactivate();
        brand.ClearDomainEvents();

        brand.MarkAsActivate();

        Assert.True(brand.IsActive);
        Assert.Contains(brand.DomainEvents, e => e is BrandActivatedEvent);
    }
}

public static class TestHelpers
{
    public static Product CreateSampleProduct(Brand brand)
    {
        return new Product(
            new ProductName("Sample Product"),
            new ProductDescription("Description"),
            brand,
            new Money(10.0m, new Currency("EUR")),
            5,
            false
        );
    }
}
