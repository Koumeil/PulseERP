using PulseERP.Domain.Entities;
using PulseERP.Domain.Exceptions;
using PulseERP.Domain.ValueObjects;

public sealed class Customer : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public Address Address { get; private set; }
    public bool IsActive { get; private set; }

    // Constructeur EF Core pour ORM (private)
    private Customer() { }

    public static Customer Create(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phone,
        Address address
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");
        if (email is null)
            throw new DomainException("Email is required");
        if (phone is null)
            throw new DomainException("Phone is required");
        if (address is null)
            throw new DomainException("Address is required");

        return new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            Phone = phone,
            Address = address,
            IsActive = true,
        };
    }

    // Mise à jour des détails, validations simples
    public void UpdateDetails(
        string? firstName = null,
        string? lastName = null,
        Email? email = null,
        PhoneNumber? phone = null
    )
    {
        var updated = false;

        if (!string.IsNullOrWhiteSpace(firstName) && firstName.Trim() != FirstName)
        {
            FirstName = firstName.Trim();
            updated = true;
        }

        if (!string.IsNullOrWhiteSpace(lastName) && lastName.Trim() != LastName)
        {
            LastName = lastName.Trim();
            updated = true;
        }

        if (email is not null && !email.Equals(Email))
        {
            Email = email;
            updated = true;
        }

        if (phone is not null && !phone.Equals(Phone))
        {
            Phone = phone;
            updated = true;
        }

        if (updated)
            MarkAsUpdated();
    }

    public void UpdateAddress(Address newAddress)
    {
        if (newAddress is null)
            throw new DomainException("Address cannot be null");

        if (!newAddress.Equals(Address))
        {
            Address = newAddress;
            MarkAsUpdated();
        }
    }

    public void Activate() => SetActive(true);

    public void Deactivate() => SetActive(false);

    private void SetActive(bool active)
    {
        if (IsActive != active)
        {
            IsActive = active;
            MarkAsUpdated();
        }
    }
}
