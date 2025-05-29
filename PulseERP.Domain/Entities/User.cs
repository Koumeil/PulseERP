using PulseERP.Domain.Exceptions;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Domain.Shared.Roles;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public sealed class User : BaseEntity
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public bool IsActive { get; private set; }
    public bool RequirePasswordChange { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > _dateTime.UtcNow;
    public UserRole Role { get; private set; }

    private readonly IDateTimeProvider _dateTime;

    private User() { }

    public User(IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public static User Create(
        string firstName,
        string lastName,
        Email email,
        Phone phone,
        string passwordHash,
        IDateTimeProvider dateTime
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (email is null)
            throw new DomainException("Email is required.");
        if (phone is null)
            throw new DomainException("Phone is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        var user = new User(dateTime)
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            IsActive = true,
            FailedLoginAttempts = 0,
            LockoutEnd = null,
            Role = SystemRoles.User,
        };

        return user;
    }

    // --- Gestion des mots de passe ---

    public void RequirePasswordReset()
    {
        if (!RequirePasswordChange)
        {
            RequirePasswordChange = true;
            MarkAsUpdated();
        }
    }

    public void ClearPasswordResetRequirement()
    {
        if (RequirePasswordChange)
        {
            RequirePasswordChange = false;
            MarkAsUpdated();
        }
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    // --- Gestion du rôle unique ---

    public void SetRole(UserRole newRole)
    {
        if (newRole == null)
            throw new DomainException("Role cannot be null.");

        if (Role != null && Role.Equals(newRole))
            return; // Pas de changement

        Role = newRole;
        MarkAsUpdated();
    }

    public bool HasRole(UserRole roleToCheck)
    {
        if (roleToCheck == null)
            return false;

        return Role != null && Role.Equals(roleToCheck);
    }

    // --- Gestion de la connexion ---

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginDate = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void RegisterFailedLoginAttempt()
    {
        if (IsLockedOut)
            return; // déjà bloqué

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
        }

        MarkAsUpdated();
    }

    public void ResetLockout()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        MarkAsUpdated();
    }

    // --- Mise à jour profil ---

    public void UpdateName(string? firstName, string? lastName)
    {
        bool updated = false;

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

    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new DomainException("Email cannot be null.");
        if (!newEmail.Equals(Email))
        {
            Email = newEmail;
            MarkAsUpdated();
        }
    }

    public void UpdatePhone(Phone newPhone)
    {
        if (newPhone == null)
            throw new DomainException("Phone cannot be null.");
        if (!newPhone.Equals(Phone))
        {
            Phone = newPhone;
            MarkAsUpdated();
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            MarkAsUpdated();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            MarkAsUpdated();
        }
    }
}


// using PulseERP.Domain.Exceptions;
// using PulseERP.Domain.ValueObjects;

// namespace PulseERP.Domain.Entities;

// public sealed class User : BaseEntity
// {
//     public string FirstName { get; private set; }
//     public string LastName { get; private set; }
//     public Email Email { get; private set; }
//     public PhoneNumber Phone { get; private set; }
//     public bool IsActive { get; private set; }

//     private User() { }

//     public static User Create(string firstName, string lastName, Email email, PhoneNumber phone)
//     {
//         if (string.IsNullOrWhiteSpace(firstName))
//             throw new DomainException("First name is required");
//         if (string.IsNullOrWhiteSpace(lastName))
//             throw new DomainException("Last name is required");
//         if (email is null)
//             throw new DomainException("Email is required");
//         if (phone is null)
//             throw new DomainException("Phone is required");

//         return new User
//         {
//             FirstName = firstName.Trim(),
//             LastName = lastName.Trim(),
//             Email = email,
//             Phone = phone,
//             IsActive = true,
//         };
//     }

//     public void UpdateName(string? firstName = null, string? lastName = null)
//     {
//         var updated = false;

//         if (!string.IsNullOrWhiteSpace(firstName) && firstName.Trim() != FirstName)
//         {
//             FirstName = firstName.Trim();
//             updated = true;
//         }

//         if (!string.IsNullOrWhiteSpace(lastName) && lastName.Trim() != LastName)
//         {
//             LastName = lastName.Trim();
//             updated = true;
//         }

//         if (updated)
//             MarkAsUpdated();
//     }

//     public void ChangeEmail(Email? newEmail)
//     {
//         if (newEmail is not null && !newEmail.Equals(Email))
//         {
//             Email = newEmail;
//             MarkAsUpdated();
//         }
//     }

//     public void ChangePhone(PhoneNumber newPhone)
//     {
//         if (newPhone is null)
//             throw new DomainException("Phone cannot be null");

//         if (!newPhone.Equals(Phone))
//         {
//             Phone = newPhone;
//             MarkAsUpdated();
//         }
//     }

//     public void Activate() => SetActive(true);

//     public void Deactivate() => SetActive(false);

//     private void SetActive(bool active)
//     {
//         if (IsActive != active)
//         {
//             IsActive = active;
//             MarkAsUpdated();
//         }
//     }
// }
