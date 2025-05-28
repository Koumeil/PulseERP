using PulseERP.Domain.Exceptions;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public bool IsActive { get; private set; }

    // Constructeur EF Core privé pour ORM
    private User() { }

    public static User Create(string firstName, string lastName, Email email, PhoneNumber phone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");
        if (email is null)
            throw new DomainException("Email is required");
        if (phone is null)
            throw new DomainException("Phone is required");

        return new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            Phone = phone,
            IsActive = true,
        };
    }

    public void UpdateName(string? firstName = null, string? lastName = null)
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

        if (updated)
            MarkAsUpdated();
    }

    public void ChangeEmail(Email? newEmail)
    {
        if (newEmail is not null && !newEmail.Equals(Email))
        {
            Email = newEmail;
            MarkAsUpdated();
        }
    }

    public void ChangePhone(PhoneNumber newPhone)
    {
        if (newPhone is null)
            throw new DomainException("Phone cannot be null");

        if (!newPhone.Equals(Phone))
        {
            Phone = newPhone;
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
