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

    public void UpdateDetails(string? firstName, string? lastName, Email? email, PhoneNumber? phone)
    {
        var changed =
            ApplyIfChanged(FirstName, firstName, v => FirstName = v)
            | ApplyIfChanged(LastName, lastName, v => LastName = v)
            | ApplyIfChanged(Email, email, v => Email = v)
            | ApplyIfChanged(Phone, phone, v => Phone = v);

        if (changed)
            MarkAsUpdated();
    }

    public void UpdateAddress(Address newAddress)
    {
        if (!Address.Equals(newAddress))
        {
            Address = newAddress;
            MarkAsUpdated();
        }
    }

    public void Activate() => ChangeStatus(true);

    public void Deactivate() => ChangeStatus(false);

    private void ChangeStatus(bool isActive)
    {
        if (IsActive != isActive)
        {
            IsActive = isActive;
            MarkAsUpdated();
        }
    }

    private static bool ApplyIfChanged<T>(T current, T updated, Action<T> apply)
    {
        if (!EqualityComparer<T>.Default.Equals(current, updated))
        {
            apply(updated);
            return true;
        }
        return false;
    }
}
