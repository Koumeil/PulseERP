using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Adresses;

namespace PulseERP.Domain.Entities;

/// <summary>
/// Represents a customer in the system. Acts as an aggregate root for customer-related operations.
/// </summary>
public sealed class Customer : BaseEntity
{
    #region Properties

    /// <summary>
    /// First name of the customer.
    /// </summary>
    public string FirstName { get; private set; } = default!;

    /// <summary>
    /// Last name of the customer.
    /// </summary>
    public string LastName { get; private set; } = default!;

    /// <summary>
    /// Company name of the customer.
    /// </summary>
    public string CompanyName { get; private set; } = default!;

    /// <summary>
    /// Email address of the customer.
    /// </summary>
    public EmailAddress Email { get; private set; } = default!;

    /// <summary>
    /// Phone number of the customer.
    /// </summary>
    public Phone Phone { get; private set; } = default!;

    /// <summary>
    /// Address of the customer.
    /// </summary>
    public Address Address { get; private set; } = default!;

    /// <summary>
    /// Type of the customer (e.g., Individual, Organization).
    /// </summary>
    public CustomerType Type { get; private set; }

    /// <summary>
    /// Current status of the customer (e.g., Active, Inactive).
    /// </summary>
    public CustomerStatus Status { get; private set; }

    /// <summary>
    /// Timestamp of first contact.
    /// </summary>
    public DateTime FirstContactDate { get; private set; }

    /// <summary>
    /// Timestamp of last interaction, if any.
    /// </summary>
    public DateTime? LastInteractionDate { get; private set; }

    /// <summary>
    /// User to whom this customer is assigned (nullable).
    /// </summary>
    public Guid? AssignedToUserId { get; private set; }

    /// <summary>
    /// Industry of the customer (nullable).
    /// </summary>
    public string? Industry { get; private set; }

    /// <summary>
    /// Source of the customer (nullable).
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    /// Indicates if the customer is VIP.
    /// </summary>
    public bool IsVIP { get; private set; }

    /// <summary>
    /// Indicates if the customer is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    private readonly List<string> _notes = new();

    /// <summary>
    /// Notes associated with the customer.
    /// </summary>
    public IReadOnlyCollection<string> Notes => _notes.AsReadOnly();

    private readonly List<string> _tags = new();

    /// <summary>
    /// Tags associated with the customer.
    /// </summary>
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Customer() { }

    #endregion

    #region Factory

    /// <summary>
    /// Creates a new customer. Throws <see cref="DomainException"/> on invalid input.
    /// </summary>
    /// <param name="firstName">First name (non-empty).</param>
    /// <param name="lastName">Last name (non-empty).</param>
    /// <param name="companyName">Company name (non-empty).</param>
    /// <param name="email">Email address (non-null).</param>
    /// <param name="phone">Phone number (non-null).</param>
    /// <param name="address">Address (non-null).</param>
    /// <param name="type">Customer type.</param>
    /// <param name="status">Customer status.</param>
    /// <param name="firstContact">First contact date.</param>
    /// <param name="isVIP">Indicates if VIP.</param>
    /// <returns>New <see cref="Customer"/> instance.</returns>
    public static Customer Create(
        string firstName,
        string lastName,
        string companyName,
        EmailAddress email,
        Phone phone,
        Address address,
        CustomerType type,
        CustomerStatus status,
        DateTime firstContact,
        bool isVIP,
        string? industry = null,
        string? source = null,
        DateTime? lastInteractionDate = null,
        Guid? assignedToUserId = null
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name required.");
        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainException("Company name required.");
        if (email is null)
            throw new DomainException("Email required.");
        if (phone is null)
            throw new DomainException("Phone required.");
        if (address is null)
            throw new DomainException("Address required.");

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
            LastInteractionDate = lastInteractionDate,
            AssignedToUserId = assignedToUserId,
            Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim(),
            Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            IsVIP = isVIP
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates multiple customer details. Ignores null or whitespace parameters.
    /// </summary>
    /// <param name="firstName">New first name (nullable).</param>
    /// <param name="lastName">New last name (nullable).</param>
    /// <param name="companyName">New company name (nullable).</param>
    /// <param name="email">New email (nullable).</param>
    /// <param name="phone">New phone (nullable).</param>
    /// <param name="address">New address (nullable).</param>
    /// <param name="type">New customer type (nullable).</param>
    /// <param name="status">New status (nullable).</param>
    /// <param name="isVIP">New VIP flag (nullable).</param>
    /// <param name="industry">New industry (nullable).</param>
    /// <param name="source">New source (nullable).</param>
    public void UpdateDetails(
        string? firstName = null,
        string? lastName = null,
        string? companyName = null,
        EmailAddress? email = null,
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

        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the customer status.
    /// </summary>
    /// <param name="newStatus">New <see cref="CustomerStatus"/> value.</param>
    public void UpdateStatus(CustomerStatus newStatus)
    {
        Status = newStatus;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records an interaction by updating the <see cref="LastInteractionDate"/> to now (UTC).
    /// </summary>
    public void RecordInteraction()
    {
        LastInteractionDate = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Assigns the customer to a user. Throws <see cref="DomainException"/> if <paramref name="userId"/> is empty.
    /// </summary>
    /// <param name="userId">User identifier (non-empty).</param>
    public void AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Assigned user id invalid.");

        AssignedToUserId = userId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Adds a note to the customer's note list. Trims whitespace.
    /// </summary>
    /// <param name="note">Note content (non-empty).</param>
    public void AddNote(string note)
    {
        if (!string.IsNullOrWhiteSpace(note))
        {
            _notes.Add(note.Trim());
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Adds a tag to the customer's tag list. Trims whitespace.
    /// </summary>
    /// <param name="tag">Tag value (non-empty).</param>
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
        {
            _tags.Add(tag.Trim());
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Sets or clears the <see cref="Industry"/>. Trims whitespace.
    /// </summary>
    /// <param name="industry">Industry value (nullable).</param>
    public void SetIndustry(string? industry) =>
        Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim();

    /// <summary>
    /// Sets or clears the <see cref="Source"/>. Trims whitespace.
    /// </summary>
    /// <param name="source">Source value (nullable).</param>
    public void SetSource(string? source) =>
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();

    /// <summary>
    /// Activates the customer if currently inactive.
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
    /// Deactivates the customer if currently active.
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
