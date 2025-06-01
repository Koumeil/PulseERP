using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system. Acts as an aggregate root for user-related operations.
    /// </summary>
    public sealed class User : BaseEntity
    {
        #region Fields

        private const int MaxFailedLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

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
        /// Email address of the user.
        /// </summary>
        public EmailAddress Email { get; private set; } = default!;

        /// <summary>
        /// Phone number of the user.
        /// </summary>
        public Phone Phone { get; private set; } = default!;

        /// <summary>
        /// Hashed password.
        /// </summary>
        public string PasswordHash { get; private set; } = default!;

        /// <summary>
        /// Role of the user.
        /// </summary>
        public Role Role { get; private set; } = default!;

        /// <summary>
        /// Indicates if the user is active.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Indicates if a password change is required.
        /// </summary>
        public bool RequirePasswordChange { get; private set; }

        /// <summary>
        /// Timestamp of last successful login.
        /// </summary>
        public DateTime? LastLoginDate { get; private set; }

        /// <summary>
        /// Number of consecutive failed login attempts.
        /// </summary>
        public int FailedLoginAttempts { get; private set; }

        /// <summary>
        /// UTC timestamp when the lockout ends.
        /// </summary>
        public DateTime? LockoutEnd { get; private set; }

        /// <summary>
        /// Indicates if the next failed login attempt will lock the account.
        /// </summary>
        public bool WillBeLockedNextAttempt => FailedLoginAttempts + 1 == MaxFailedLoginAttempts;

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private User() { }

        #endregion

        #region Factory

        /// <summary>
        /// Creates a new user with the specified parameters. Throws <see cref="DomainException"/> on invalid input.
        /// </summary>
        /// <param name="firstName">First name (non-empty).</param>
        /// <param name="lastName">Last name (non-empty).</param>
        /// <param name="email">Email address (non-null).</param>
        /// <param name="phone">Phone number (non-null).</param>
        /// <param name="passwordHash">Password hash (non-empty).</param>
        /// <returns>New <see cref="User"/> instance.</returns>
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

        #endregion

        #region Methods

        /// <summary>
        /// Determines if the user is currently locked out given <paramref name="nowUtc"/>.
        /// </summary>
        /// <param name="nowUtc">Current UTC time.</param>
        /// <returns>True if locked out, otherwise false.</returns>
        public bool IsLockedOut(DateTime nowUtc) => LockoutEnd.HasValue && LockoutEnd > nowUtc;

        /// <summary>
        /// Registers a failed login attempt. If maximum attempts reached, sets lockout.
        /// </summary>
        /// <param name="nowUtc">Current UTC time.</param>
        /// <returns>Lockout end time if locked, otherwise null.</returns>
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

        /// <summary>
        /// Registers a successful login, resetting failed attempts and lockout.
        /// </summary>
        /// <param name="nowUtc">Current UTC time.</param>
        public void RegisterSuccessfulLogin(DateTime nowUtc)
        {
            FailedLoginAttempts = 0;
            LockoutEnd = null;
            LastLoginDate = nowUtc;
            MarkAsUpdated();
        }

        /// <summary>
        /// Resets the lockout state (failed attempts and lockout end).
        /// </summary>
        public void ResetLockout()
        {
            FailedLoginAttempts = 0;
            LockoutEnd = null;
            MarkAsUpdated();
        }

        /// <summary>
        /// Marks that a password reset is required.
        /// </summary>
        public void RequirePasswordReset()
        {
            if (!RequirePasswordChange)
            {
                RequirePasswordChange = true;
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Clears the password reset requirement if set.
        /// </summary>
        public void ClearPasswordResetRequirement()
        {
            if (RequirePasswordChange)
            {
                RequirePasswordChange = false;
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Updates the password hash. Throws <see cref="DomainException"/> if new hash is empty.
        /// </summary>
        /// <param name="newPasswordHash">New password hash (non-empty).</param>
        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new DomainException("Password hash cannot be empty.");

            PasswordHash = newPasswordHash;
            MarkAsUpdated();
        }

        /// <summary>
        /// Assigns a new role. Throws <see cref="DomainException"/> for invalid role.
        /// </summary>
        /// <param name="newRole">New role (non-empty).</param>
        public void SetRole(Role newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole.Value))
                throw new DomainException("Role cannot be empty.");

            if (Role != newRole)
            {
                Role = newRole;
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Checks if the user has the specified role.
        /// </summary>
        /// <param name="roleToCheck">Role to verify.</param>
        /// <returns>True if matches, otherwise false.</returns>
        public bool HasRole(Role roleToCheck) =>
            !string.IsNullOrWhiteSpace(roleToCheck.Value) && Role == roleToCheck;

        /// <summary>
        /// Updates user's first and/or last name if non-empty and different.
        /// </summary>
        /// <param name="firstName">New first name or null to keep existing.</param>
        /// <param name="lastName">New last name or null to keep existing.</param>
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

        /// <summary>
        /// Updates the email address. Throws <see cref="DomainException"/> if newEmail is null.
        /// </summary>
        /// <param name="newEmail">New email (non-null).</param>
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

        /// <summary>
        /// Updates the phone number. Throws <see cref="DomainException"/> if newPhone is null.
        /// </summary>
        /// <param name="newPhone">New phone (non-null).</param>
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

        /// <summary>
        /// Activates the user if currently inactive.
        /// </summary>
        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Deactivates the user if currently active.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsUpdated();
            }
        }

        #endregion
    }
}
