namespace PulseERP.Domain.Entities;

using System;
using Errors;
using Events.UserEvents;
using ValueObjects;
using VO;
public sealed class User : BaseEntity
{
    #region Constants

    private const int PasswordExpirationDays = 60;
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    #endregion

    #region Properties

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = default!;
    public EmailAddress Email { get; private set; } = default!;
    public Phone PhoneNumber { get; private set; } = default!;
    public string? PasswordHash { get; private set; }
    public DateTime? PasswordLastChangedAt { get; private set; }
    public bool RequirePasswordChange { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public Role Role { get; private set; }
    public bool WillBeLockedOutAfterNextFailure =>
        FailedLoginAttempts + 1 >= MaxFailedLoginAttempts;

    #endregion

    #region Constructors

    public User(
        string firstName,
        string lastName,
        EmailAddress email,
        Phone phoneNumber
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


        FirstName = trimmedFirst;
        LastName = trimmedLast;
        PhoneNumber = phoneNumber;
        PasswordLastChangedAt = DateTime.UtcNow;
        RequirePasswordChange = false;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginDate = null;
        Role = SystemRoles.Default;
        SetIsActive(true);


        AddDomainEvent(new UserCreatedEvent(Id, FirstName, LastName, Email.Value));
    }

    #endregion

    #region Domain Behaviors

    public void CheckPasswordExpiration(DateTime nowUtc)
    {
        if (PasswordLastChangedAt is null)
        {
            RequirePasswordChange = true;
        }
        else
        {
            var ageInDays = (nowUtc - PasswordLastChangedAt.Value).TotalDays;
            RequirePasswordChange = (ageInDays > PasswordExpirationDays);
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

        AddDomainEvent(new UserPasswordChangedEvent(Id, FirstName, LastName, Email.Value));
    }

    /// <summary>
    /// Forces the user to change their password on next login.
    /// </summary>
    public void ForcePasswordReset()
    {
        RequirePasswordChange = true;
        MarkAsUpdated();

        AddDomainEvent(new UserPasswordResetForcedEvent(Id, FirstName, LastName, Email.Value, LockoutEnd!.Value));
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

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
            LockoutEnd = nowUtc.Add(LockoutDuration);

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
            AddDomainEvent(new UserRoleChangedEvent(Id, newRole.ToString()));
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
            AddDomainEvent(new UserEmailChangedEvent(Id, newEmail.ToString()));
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

        if (PhoneNumber.Equals(newPhone)) return;
        PhoneNumber = newPhone;
        MarkAsUpdated();
        AddDomainEvent(new UserPhoneChangedEvent(Id, newPhone.ToString()));
    }

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
            AddDomainEvent(new UserDeletedEvent(Id));
        }
    }

    public override void MarkAsRestored()
    {
        if (IsDeleted)
        {
            base.MarkAsRestored();
            AddDomainEvent(new UserRestoredEvent(Id, FirstName, LastName, Email.Value));
        }
    }

    public override void MarkAsDeactivate()
    {
        base.MarkAsDeactivate();
        AddDomainEvent(new UserDeactivatedEvent(Id, FirstName, LastName, Email.Value));
    }

    public override void MarkAsActivate()
    {
        base.MarkAsActivate();
        AddDomainEvent(new UserActivatedEvent(Id, FirstName, LastName, Email.Value));
    }

    #endregion
}

