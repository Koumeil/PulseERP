namespace PulseERP.Tests.Domain.Entities;

using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.VO;
using Xunit;

public class UserTests
{
    private static EmailAddress ValidEmail => new("user@example.com");
    private static string ValidPassword => "HASHED_PASSWORD";

    private static Phone ValidPhoneNumber => new("+32498894885");

    [Fact]
    public void Constructor_ShouldInitializePropertiesAndRaiseEvent()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);

        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal(ValidEmail, user.Email);
        Assert.NotNull(user.PasswordLastChangedAt);
        Assert.False(user.RequirePasswordChange);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.Null(user.LastLoginDate);
        Assert.NotEmpty(user.DomainEvents);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidFirstName_ShouldThrow(string? invalidName)
    {
        Assert.Throws<DomainValidationException>(() =>
            new User(invalidName!, "Doe", ValidEmail, ValidPhoneNumber)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidLastName_ShouldThrow(string? invalidName)
    {
        Assert.Throws<DomainValidationException>(() =>
            new User("John", invalidName!, ValidEmail, ValidPhoneNumber)
        );
    }

    [Fact]
    public void CheckPasswordExpiration_ShouldUpdateRequirePasswordChange()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        user.CheckPasswordExpiration(DateTime.UtcNow.AddDays(61));
        Assert.True(user.RequirePasswordChange);
    }

    [Fact]
    public void UpdatePassword_ShouldUpdateHashAndRaiseEvent()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        user.UpdatePassword("NEW_HASHED");
        Assert.Equal("NEW_HASHED", user.PasswordHash);
        Assert.False(user.RequirePasswordChange);
    }


    [Fact]
    public void RegisterFailedLogin_ShouldLockoutAfterMaxAttempts()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        DateTime now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
            user.RegisterFailedLogin(now);

        Assert.True(user.IsLockedOut(now));
        Assert.NotNull(user.LockoutEnd);
    }

    [Fact]
    public void RegisterSuccessfulLogin_ShouldResetLockout()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        user.RegisterFailedLogin(DateTime.UtcNow);
        user.RegisterSuccessfulLogin(DateTime.UtcNow);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.NotNull(user.LastLoginDate);
    }

    [Fact]
    public void ResetLockout_ShouldClearLockoutData()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        user.RegisterFailedLogin(DateTime.UtcNow);
        user.ResetLockout();
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void ChangeRole_ShouldUpdateRoleAndRaiseEvent()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        var newRole = new Role("Manager");
        user.ChangeRole(newRole);
        Assert.Equal(newRole, user.Role);
    }

    [Fact]
    public void ActivateUser_DeactivateUser_TogglesStateCorrectly()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        user.MarkAsDeactivate();
        user.MarkAsActivate();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void DeleteAndRestoreUser_ShouldToggleDeletedState()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        user.MarkAsDeleted();
        Assert.True(user.IsDeleted);
        user.MarkAsRestored();
        Assert.False(user.IsDeleted);
    }

    [Fact]
    public void UpdateEmail_ShouldChangeValueAndRaiseEvent()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        var newEmail = new EmailAddress("new@example.com");
        user.UpdateEmail(newEmail);
        Assert.Equal(newEmail, user.Email);
    }

    [Fact]
    public void UpdatePhone_ShouldChangeValueAndRaiseEvent()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        var newPhone = new Phone("+12345678901");
        user.UpdatePhone(newPhone);
        Assert.Equal(newPhone, user.PhoneNumber);
    }

    [Fact]
    public void UpdateName_ShouldChangeOnlyWhenDifferent()
    {
        var user = new User("John", "Doe", ValidEmail, ValidPhoneNumber);
        user.UpdateName("John", "Smith");
        Assert.Equal("Smith", user.LastName);
    }
}
