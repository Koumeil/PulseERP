using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public bool IsActive { get; private set; }

    private User() { }

    // Factory method pour une création contrôlée
    public static User Create(string firstName, string lastName, string email, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Le prénom est obligatoire", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Le nom est obligatoire", nameof(lastName));

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = new Email(email), // La validation est dans le constructeur de Email
            Phone = phone is not null ? new PhoneNumber(phone) : null,
            IsActive = true,
        };

        return user;
    }

    public void UpdateName(string? firstName, string? lastName)
    {
        if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
        {
            FirstName = firstName!.Trim();
            LastName = lastName!.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
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

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

// namespace PulseERP.Domain.Entities;

// public class User : BaseEntity
// {
//     public string FirstName { get; private set; }
//     public string LastName { get; private set; }
//     public string Email { get; private set; }
//     public string? Phone { get; private set; }
//     public bool IsActive { get; private set; }

//     private User() { }

//     public User(string firstName, string lastName, string email, string? phone = null)
//     {
//         FirstName = firstName;
//         LastName = lastName;
//         Email = email;
//         Phone = phone;
//         IsActive = true;
//     }

//     public void UpdateFirstName(string? newName)
//     {
//         if (!string.IsNullOrWhiteSpace(newName) && newName != FirstName)
//             FirstName = newName;
//     }

//     public void UpdateLastName(string? newLastName)
//     {
//         if (!string.IsNullOrWhiteSpace(newLastName) && newLastName != LastName)
//             LastName = newLastName;
//     }

//     public void UpdateEmail(string? newEmail)
//     {
//         if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != Email)
//             Email = newEmail;
//     }

//     public void UpdatePhone(string? newPhone)
//     {
//         if (!string.IsNullOrWhiteSpace(newPhone) && newPhone != Phone)
//             Phone = newPhone;
//     }

//     public void Deactivate() => IsActive = false;
// }
