using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public sealed class Customer : BaseEntity
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string CompanyName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public Phone Phone { get; private set; } = default!;
    public Address Address { get; private set; } = default!;

    // Types valeur ou enum
    public CustomerType Type { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime FirstContactDate { get; private set; }
    public DateTime? LastInteractionDate { get; private set; }

    public Guid? AssignedToUserId { get; private set; }
    public string? Industry { get; private set; }
    public string? Source { get; private set; }
    public bool IsVIP { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<string> _notes = new();
    public IReadOnlyCollection<string> Notes => _notes.AsReadOnly();

    private readonly List<string> _tags = new();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    // Constructeur vide pour EF Core
    private Customer() { }

    public static Customer Create(
        string firstName,
        string lastName,
        string companyName,
        Email email,
        Phone phone,
        Address address,
        CustomerType type,
        CustomerStatus status,
        DateTime firstContact,
        bool isVIP
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name required");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name required");
        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainException("Company name required");
        if (email is null)
            throw new DomainException("Email required");
        if (phone is null)
            throw new DomainException("Phone required");
        if (address is null)
            throw new DomainException("Address required");

        return new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CompanyName = companyName.Trim(),
            Email = email,
            Phone = phone,
            Address = address,
            Type = type,
            Status = status,
            FirstContactDate = firstContact,
            IsVIP = isVIP,
            IsActive = true,
        };
    }

    public void UpdateDetails(
        string? firstName = null,
        string? lastName = null,
        string? companyName = null,
        Email? email = null,
        Phone? phone = null,
        Address? address = null,
        CustomerType? type = null,
        CustomerStatus? status = null,
        bool? isVIP = null,
        string? industry = null,
        string? source = null
    )
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName.Trim();

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName.Trim();

        if (!string.IsNullOrWhiteSpace(companyName))
            CompanyName = companyName.Trim();

        if (email is not null)
            Email = email;

        if (phone is not null)
            Phone = phone;

        if (address is not null)
            Address = address;

        if (type.HasValue)
            Type = type.Value;

        if (status.HasValue)
            Status = status.Value;

        if (isVIP.HasValue)
            IsVIP = isVIP.Value;

        if (!string.IsNullOrWhiteSpace(industry))
            Industry = industry.Trim();

        if (!string.IsNullOrWhiteSpace(source))
            Source = source.Trim();
    }

    public void UpdateStatus(CustomerStatus newStatus) => Status = newStatus;

    public void RecordInteraction()
    {
        LastInteractionDate = DateTime.UtcNow;
    }

    public void AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Assigned user id invalid");
        AssignedToUserId = userId;
    }

    public void AddNote(string note)
    {
        if (!string.IsNullOrWhiteSpace(note))
            _notes.Add(note.Trim());
    }

    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
            _tags.Add(tag.Trim());
    }

    public void SetIndustry(string? industry) =>
        Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim();

    public void SetSource(string? source) =>
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
