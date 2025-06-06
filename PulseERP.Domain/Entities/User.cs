namespace PulseERP.Domain.Entities;

using System;
using PulseERP.Domain.Common;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Events.UserEvents;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.VO;

/// <summary>
/// Aggregate root representing an application user, including authentication and security behavior.
/// </summary>
public sealed class User : BaseEntity
{
    #region Constants

    private const int PASSWORD_EXPIRATION_DAYS = 60;
    private const int MAX_FAILED_LOGIN_ATTEMPTS = 5;
    private static readonly TimeSpan LOCKOUT_DURATION = TimeSpan.FromMinutes(15);

    #endregion

    #region Properties

    /// <summary>
    /// First name of the user.
    /// </summary>
    public string FirstName { get; private set; } = default!;

    /// <summary>
    /// Last name of the user.
    /// </summary>
    public string LastName { get; private set; } = default!;

    /// <summary>
    /// Email address of the user (Value Object).
    /// </summary>
    public EmailAddress Email { get; private set; } = default!;

    /// <summary>
    /// Phone number of the user (Value Object). May be null.
    /// </summary>
    public Phone PhoneNumber { get; private set; } = default!;

    /// <summary>
    /// Hashed password string. Never stores plain text.
    /// </summary>
    public string PasswordHash { get; private set; } = default!;

    /// <summary>
    /// UTC timestamp when the password was last changed (null if never changed).
    /// </summary>
    public DateTime? PasswordLastChangedAt { get; private set; }

    /// <summary>
    /// True if the user must change password on next login (expired or forced).
    /// </summary>
    public bool RequirePasswordChange { get; private set; }

    /// <summary>
    /// Number of consecutive failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// UTC timestamp when the lockout ends. Null if not locked.
    /// </summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>
    /// UTC timestamp of the last successful login. Null if never logged in.
    /// </summary>
    public DateTime? LastLoginDate { get; private set; }

    /// <summary>
    /// Role of the user (Value Object).
    /// </summary> /// <summary>
    /// Role of the user (Value Object).
    /// </summary>
    public Role Role { get; private set; }

    /// <summary>
    /// Returns true if the next failed login attempt will lock the account.
    /// </summary>
    public bool WillBeLockedOutAfterNextFailure =>
        FailedLoginAttempts + 1 >= MAX_FAILED_LOGIN_ATTEMPTS;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new user with required invariants.
    /// </summary>
    /// <param name="firstName">First name (1–100 characters).</param>
    /// <param name="lastName">Last name (1–100 characters).</param>
    /// <param name="email">EmailAddress VO (non-null).</param>
    /// <param name="passwordHash">Hashed password string (non-null, non-empty).</param>
    /// <param name="role">Role VO (non-null).</param>
    /// <exception cref="DomainValidationException">
    /// Thrown if any invariant is violated.
    /// </exception>
    public User(
        string firstName,
        string lastName,
        EmailAddress email,
        Phone phoneNumber,
        string passwordHash
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainValidationException("FirstName cannot be null or whitespace.");
        var trimmedFirst = firstName.Trim();
        if (trimmedFirst.Length > 100)
            throw new DomainValidationException(
                $"FirstName cannot exceed 100 characters; got {trimmedFirst.Length}."
            );

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainValidationException("LastName cannot be null or whitespace.");
        var trimmedLast = lastName.Trim();
        if (trimmedLast.Length > 100)
            throw new DomainValidationException(
                $"LastName cannot exceed 100 characters; got {trimmedLast.Length}."
            );
        Email = email ?? throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException("PasswordHash cannot be null or whitespace.");

        FirstName = trimmedFirst;
        LastName = trimmedLast;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        PasswordLastChangedAt = DateTime.UtcNow;
        RequirePasswordChange = false;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginDate = null;
        Role = SystemRoles.Default;

        AddDomainEvent(new UserCreatedEvent(Id));
    }

    #endregion

    #region Domain Behaviors

    /// <summary>
    /// Checks if the password is expired as of <paramref name="nowUtc"/>.
    /// If expired or never changed, sets <see cref="RequirePasswordChange"/> to true.
    /// </summary>
    /// <param name="nowUtc">Current UTC datetime.</param>
    public void CheckPasswordExpiration(DateTime nowUtc)
    {
        if (PasswordLastChangedAt is null)
        {
            RequirePasswordChange = true;
        }
        else
        {
            var ageInDays = (nowUtc - PasswordLastChangedAt.Value).TotalDays;
            RequirePasswordChange = (ageInDays > PASSWORD_EXPIRATION_DAYS);
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the password hash and resets expiration state.
    /// </summary>
    /// <param name="newHashedPassword">New hashed password (non-null, non-empty).</param>
    public void UpdatePassword(string newHashedPassword)
    {
        if (string.IsNullOrWhiteSpace(newHashedPassword))
            throw new DomainValidationException("New password hash cannot be null or whitespace.");

        PasswordHash = newHashedPassword;
        PasswordLastChangedAt = DateTime.UtcNow;
        RequirePasswordChange = false;
        MarkAsUpdated();

        AddDomainEvent(new UserPasswordChangedEvent(Id));
    }

    /// <summary>
    /// Forces the user to change their password on next login.
    /// </summary>
    public void ForcePasswordReset()
    {
        RequirePasswordChange = true;
        MarkAsUpdated();

        AddDomainEvent(new UserPasswordResetForcedEvent(Id));
    }

    /// <summary>
    /// Returns true if the account is currently locked out given <paramref name="nowUtc"/>.
    /// </summary>
    /// <param name="nowUtc">Current UTC datetime.</param>
    public bool IsLockedOut(DateTime nowUtc) => LockoutEnd.HasValue && LockoutEnd.Value > nowUtc;

    /// <summary>
    /// Registers a failed login attempt.
    /// If threshold reached, sets <see cref="LockoutEnd"/> accordingly.
    /// Returns the new <see cref="LockoutEnd"/> (null if not locked).
    /// </summary>
    /// <param name="nowUtc">Current UTC datetime.</param>
    public DateTime? RegisterFailedLogin(DateTime nowUtc)
    {
        if (IsLockedOut(nowUtc))
            return LockoutEnd;

        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MAX_FAILED_LOGIN_ATTEMPTS)
            LockoutEnd = nowUtc.Add(LOCKOUT_DURATION);

        MarkAsUpdated();
        AddDomainEvent(new UserLockedOutEvent(Id, LockoutEnd));
        return LockoutEnd;
    }

    /// <summary>
    /// Registers a successful login: resets failed attempts and lockout, updates <see cref="LastLoginDate"/>.
    /// </summary>
    /// <param name="nowUtc">Current UTC datetime.</param>
    public void RegisterSuccessfulLogin(DateTime nowUtc)
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginDate = nowUtc;
        MarkAsUpdated();

        AddDomainEvent(new UserLoginSuccessfulEvent(Id, nowUtc));
    }

    /// <summary>
    /// Resets all lockout state: clears failed attempts and <see cref="LockoutEnd"/>.
    /// </summary>
    public void ResetLockout()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        MarkAsUpdated();

        AddDomainEvent(new UserLockoutResetEvent(Id));
    }

    /// <summary>
    /// Changes the user’s role.
    /// </summary>
    /// <param name="newRole">New role (non-null).</param>
    public void ChangeRole(Role newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole.Value))
            throw new DomainValidationException("Role cannot be empty.");

        if (Role != newRole)
        {
            Role = newRole;
            MarkAsUpdated();
            AddDomainEvent(new UserRoleChangedEvent(Id, newRole));
        }
    }

    /// <summary>
    /// Updates the user’s email address.
    /// </summary>
    /// <param name="newEmail">New EmailAddress VO (non-null).</param>
    public void UpdateEmail(EmailAddress newEmail)
    {
        if (newEmail is null)
            throw new DomainValidationException("Email cannot be null.");

        if (!Email.Equals(newEmail))
        {
            Email = newEmail;
            MarkAsUpdated();
            AddDomainEvent(new UserEmailChangedEvent(Id, newEmail));
        }
    }

    /// <summary>
    /// Updates the user’s phone number.
    /// </summary>
    /// <param name="newPhone">New Phone VO (non-null).</param>
    public void UpdatePhone(Phone newPhone)
    {
        if (newPhone is null)
            throw new DomainValidationException("Phone cannot be null.");

        if (PhoneNumber is null || !PhoneNumber.Equals(newPhone))
        {
            PhoneNumber = newPhone;
            MarkAsUpdated();
            AddDomainEvent(new UserPhoneChangedEvent(Id, newPhone));
        }
    }

    /// <summary>
    /// Updates the user’s first and/or last name.
    /// </summary>
    /// <param name="firstName">New first name (optional, 1–100 chars).</param>
    /// <param name="lastName">New last name (optional, 1–100 chars).</param>
    public void UpdateName(string? firstName, string? lastName)
    {
        var updated = false;

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            var trimmedFirst = firstName.Trim();
            if (trimmedFirst.Length > 100)
                throw new DomainValidationException(
                    $"FirstName cannot exceed 100 characters; got {trimmedFirst.Length}."
                );

            if (!FirstName.Equals(trimmedFirst, StringComparison.Ordinal))
            {
                FirstName = trimmedFirst;
                updated = true;
            }
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            var trimmedLast = lastName.Trim();
            if (trimmedLast.Length > 100)
                throw new DomainValidationException(
                    $"LastName cannot exceed 100 characters; got {trimmedLast.Length}."
                );

            if (!LastName.Equals(trimmedLast, StringComparison.Ordinal))
            {
                LastName = trimmedLast;
                updated = true;
            }
        }

        if (updated)
        {
            MarkAsUpdated();
            AddDomainEvent(new UserNameChangedEvent(Id, FirstName, LastName));
        }
    }

    public override void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            base.MarkAsDeleted();
            AddDomainEvent(new UserDeactivatedEvent(Id));
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
            AddDomainEvent(new UserRestoredEvent(Id));
        }
    }

    public override void MarkAsDeactivate()
    {
        base.MarkAsDeactivate();
        AddDomainEvent(new UserDeactivatedEvent(Id));
    }

    public override void MarkAsActivate()
    {
        base.MarkAsActivate();
        AddDomainEvent(new UserActivatedEvent(Id));
    }

    #endregion
}


/// </summary>
