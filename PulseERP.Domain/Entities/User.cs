using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public sealed class User : BaseEntity
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public bool WillBeLockedNextAttempt => FailedLoginAttempts + 1 == MaxFailedLoginAttempts;

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public EmailAddress Email { get; private set; } = default!;
    public Phone Phone { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    // Types valeur
    public bool IsActive { get; private set; }
    public bool RequirePasswordChange { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    // Constructeur vide pour EF Core
    private User() { }

    public static User Create(
        string firstName,
        string lastName,
        EmailAddress email,
        Phone phone,
        string passwordHash
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

        return new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            IsActive = true,
            FailedLoginAttempts = 0,
            LockoutEnd = null,
            Role = Role.User,
        };
    }

    public bool IsLockedOut(DateTime nowUtc) => LockoutEnd.HasValue && LockoutEnd > nowUtc;

    public DateTime? RegisterFailedLoginAttempt(DateTime nowUtc)
    {
        if (IsLockedOut(nowUtc))
            return LockoutEnd;

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockoutEnd = nowUtc.Add(LockoutDuration);
        }

        MarkAsUpdated();
        return LockoutEnd;
    }

    public void RegisterSuccessfulLogin(DateTime nowUtc)
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginDate = nowUtc;
        MarkAsUpdated();
    }

    public void ResetLockout()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        MarkAsUpdated();
    }

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

    public void SetRole(Role newRole)
    {
        // Si on reçoit un struct par défaut, on le considère invalide
        if (string.IsNullOrWhiteSpace(newRole.Value))
            throw new DomainException("Role cannot be empty.");

        if (Role == newRole)
            return;

        Role = newRole;
        MarkAsUpdated();
    }

    public bool HasRole(Role roleToCheck) =>
        !string.IsNullOrWhiteSpace(roleToCheck.Value) && Role == roleToCheck;

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

    public void UpdateEmail(EmailAddress newEmail)
    {
        if (newEmail is null)
            throw new DomainException("Email cannot be null.");

        if (!newEmail.Equals(Email))
        {
            Email = newEmail;
            MarkAsUpdated();
        }
    }

    public void UpdatePhone(Phone newPhone)
    {
        if (newPhone is null)
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
