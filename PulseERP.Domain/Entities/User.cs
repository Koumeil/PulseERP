namespace PulseERP.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }

    // EF only
    private User() { }

    public User(string firstName, string lastName, string email, string? phone = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;

    public void UpdateContact(string? phone) => Phone = phone;
}
