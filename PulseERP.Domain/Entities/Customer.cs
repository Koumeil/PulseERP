using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
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
            Email = new Email(email),
            Phone = phone is not null ? new PhoneNumber(phone) : null,
            Address = address,
            IsActive = true,
        };
    }

    public void UpdateDetails(string? firstName, string? lastName, string? email, string? phone)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName;

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName;

        if (!string.IsNullOrWhiteSpace(email))
            ChangeEmail(email);

        if (!string.IsNullOrWhiteSpace(phone))
            ChangePhone(phone);

        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeEmail(string email)
    {
        var newEmail = new Email(email);
        if (Email != newEmail)
        {
            Email = newEmail;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ChangePhone(string? phone)
    {
        var newPhone = phone is not null ? new PhoneNumber(phone) : null;
        if (Phone != newPhone)
        {
            Phone = newPhone;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateAddress(string? street, string? city, string? postalCode, string? country)
    {
        // 1. Tentative de création/mise à jour
        var updatedAddress = Address.TryCreateOrUpdate(
            existingAddress: Address,
            street: street,
            city: city,
            zipCode: postalCode,
            country: country
        );

        if (updatedAddress is not null && !updatedAddress.Equals(Address))
        {
            Address = updatedAddress;
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
