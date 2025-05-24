using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public Address? Address { get; private set; }
    public bool IsActive { get; private set; }

    private Customer() { }

    public static Customer Create(
        string firstName,
        string lastName,
        string email,
        Address? address,
        string? phone = null
    )
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Name fields cannot be empty");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");

        return new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim(),
            Phone = phone?.Trim(),
            Address = address,
            IsActive = true,
        };
    }

    public void UpdateDetails(string? firstName, string? lastName, string? email, string? phone)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName.Trim();
        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName.Trim();
        if (!string.IsNullOrWhiteSpace(email))
            Email = email.Trim();
        if (phone != null)
            Phone = phone.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(string? street, string? city, string? postalCode, string? country)
    {
        var newAddress = Address.TryCreateIfValid(street, city, postalCode, country);

        // Si aucune nouvelle adresse (tous les champs vides), ne rien faire
        if (newAddress is null && Address is null)
            return;

        // Si on a une nouvelle adresse ET (aucune ancienne OU changement)
        if (newAddress is not null && !newAddress.Equals(Address))
        {
            Address = newAddress;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
