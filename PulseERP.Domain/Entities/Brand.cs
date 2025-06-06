namespace PulseERP.Domain.Entities;

using System;
using System.Collections.Generic;
using PulseERP.Domain.Common;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Events.BrandEvents;
using PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Represents a brand of products. Acts as an aggregate root for brand-related operations.
/// </summary>
public sealed class Brand : BaseEntity
{
    #region Fields

    private readonly List<Product> _products = new();

    #endregion

    #region Properties

    /// <summary>
    /// Name of the brand.
    /// </summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Read-only collection of associated products.
    /// </summary>
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    #endregion

    #region Constructors

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    private Brand() { }

    /// <summary>
    /// Creates a new brand instance with a validated name.
    /// </summary>
    /// <param name="name">Brand name (non-empty).</param>
    /// <exception cref="DomainValidationException">Thrown if the name is invalid.</exception>
    public Brand(string name)
    {
        SetName(name);
        // IsActive est déjà true par défaut dans BaseEntity
        AddDomainEvent(new BrandCreatedEvent(Id, Name));
    }

    #endregion

    #region Behaviors

    /// <summary>
    /// Updates the brand's name.
    /// </summary>
    /// <param name="newName">New brand name (non-empty).</param>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainValidationException("Brand name is required.");

        var trimmed = newName.Trim();
        if (Name.Equals(trimmed, StringComparison.Ordinal))
            return;

        Name = trimmed;
        MarkAsUpdated();
        AddDomainEvent(new BrandNameUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Adds a product to the brand’s product collection.
    /// </summary>
    /// <param name="product">Product to add (non-null).</param>
    public void AddProduct(Product product)
    {
        if (product is null)
            throw new DomainValidationException("Cannot add null product to brand.");

        if (_products.Contains(product))
            return;

        _products.Add(product);
        MarkAsUpdated();
        AddDomainEvent(new BrandProductAddedEvent(Id, product.Id));
    }

    /// <summary>
    /// Removes a product from the brand’s product collection.
    /// </summary>
    /// <param name="product">Product to remove (non-null).</param>
    public void RemoveProduct(Product product)
    {
        if (product is null)
            throw new DomainValidationException("Cannot remove null product from brand.");

        if (_products.Remove(product))
        {
            MarkAsUpdated();
            AddDomainEvent(new BrandProductRemovedEvent(Id, product.Id));
        }
    }

    public override void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            base.MarkAsDeleted();
            AddDomainEvent(new BrandDeletedEvent(Id));
        }
    }

    /// <summary>
    /// Restores the brand from soft-deleted state.
    /// </summary>
    public override void MarkAsRestored()
    {
        if (IsDeleted)
        {
            base.MarkAsRestored();
            AddDomainEvent(new BrandRestoredEvent(Id));
        }
    }

    public override void MarkAsDeactivate()
    {
        base.MarkAsDeactivate();
        AddDomainEvent(new BrandDeactivatedEvent(Id));
    }

    public override void MarkAsActivate()
    {
        base.MarkAsActivate();
        AddDomainEvent(new BrandActivatedEvent(Id));
    }

    #endregion

    #region Helpers

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Brand name is required.");

        Name = name.Trim();
    }

    #endregion
}
