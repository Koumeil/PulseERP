namespace PulseERP.Domain.Entities;

using PulseERP.Domain.Enums.Customer;
using Errors;
using Events.CustomerEvents;
using VO;

/// <summary>
/// Represents a customer in the system. Acts as an aggregate root for customer-related operations.
/// </summary>
public sealed class Customer : BaseEntity
{
    #region Fields

    private readonly List<string> _notes = new();
    private readonly List<string> _tags = new();

    #endregion

    #region Properties

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string CompanyName { get; private set; } = default!;
    public EmailAddress Email { get; private set; } = default!;
    public Phone Phone { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public CustomerType Type { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime FirstContactDate { get; private set; }
    public DateTime? LastInteractionDate { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? Industry { get; private set; }
    public string? Source { get; private set; }
    public bool IsVip { get; private set; }
    public IReadOnlyCollection<string> Notes => _notes.AsReadOnly();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    #endregion

    #region Constructors

    private Customer() { }

    public Customer(
        string firstName,
        string lastName,
        string companyName,
        EmailAddress email,
        Phone phone,
        Address address,
        CustomerType type,
        CustomerStatus status,
        DateTime firstContact,
        bool isVip,
        string? industry = null,
        string? source = null,
        DateTime? lastInteractionDate = null,
        Guid? assignedToUserId = null
    )
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        CompanyName = companyName.Trim();
        Email = email;
        Phone = phone;
        Address = address;
        Type = type;
        Status = status;
        FirstContactDate = firstContact;
        LastInteractionDate = lastInteractionDate;
        AssignedToUserId = assignedToUserId;
        Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim();
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
        IsVip = isVip;

        AddDomainEvent(new CustomerCreatedEvent(Id));
    }

    #endregion

    #region Methods

    private bool TryUpdate<T>(T current, T? updated, Action<T> apply)
        where T : IEquatable<T>
    {
        if (updated is not null && !current.Equals(updated))
        {
            apply(updated);
            return true;
        }
        return false;
    }

    private bool TryUpdateString(string current, string? updated, Action<string> apply)
    {
        if (!string.IsNullOrWhiteSpace(updated))
        {
            var trimmed = updated.Trim();
            if (!current.Equals(trimmed))
            {
                apply(trimmed);
                return true;
            }
        }
        return false;
    }

    public void UpdateDetails(
        string? firstName = null,
        string? lastName = null,
        string? companyName = null,
        EmailAddress? email = null,
        Phone? phone = null,
        Address? address = null,
        CustomerType? type = null,
        CustomerStatus? status = null,
        bool? isVip = null,
        string? industry = null,
        string? source = null
    )
    {
        bool changed = false;

        changed |= TryUpdateString(FirstName, firstName, value => FirstName = value);
        changed |= TryUpdateString(LastName, lastName, value => LastName = value);
        changed |= TryUpdateString(CompanyName, companyName, value => CompanyName = value);
        changed |= TryUpdate(Email, email, value => Email = value);
        changed |= TryUpdate(Phone, phone, value => Phone = value);
        changed |= TryUpdate(Address, address, value => Address = value);

        if (type.HasValue && Type != type.Value)
        {
            Type = type.Value;
            changed = true;
        }

        if (status.HasValue && Status != status.Value)
        {
            Status = status.Value;
            changed = true;
        }

        if (isVip.HasValue && IsVip != isVip.Value)
        {
            IsVip = isVip.Value;
            changed = true;
        }

        changed |= TryUpdateString(Industry ?? string.Empty, industry, value => Industry = value);
        changed |= TryUpdateString(Source ?? string.Empty, source, value => Source = value);

        if (changed)
        {
            MarkAsUpdated();
            AddDomainEvent(new CustomerDetailsUpdatedEvent(Id));
        }
    }

    public void UpdateStatus(CustomerStatus newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
            MarkAsUpdated();
            AddDomainEvent(new CustomerStatusChangedEvent(Id, newStatus));
        }
    }

    public void RecordInteraction()
    {
        LastInteractionDate = DateTime.UtcNow;
        MarkAsUpdated();
        AddDomainEvent(new CustomerInteractionRecordedEvent(Id));
    }

    public void AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainValidationException("Assigned user id invalid.");

        AssignedToUserId = userId;
        MarkAsUpdated();
        AddDomainEvent(new CustomerAssignedEvent(Id, userId));
    }

    public void AddNote(string note)
    {
        if (!string.IsNullOrWhiteSpace(note))
        {
            _notes.Add(note.Trim());
            MarkAsUpdated();
            AddDomainEvent(new CustomerNoteAddedEvent(Id));
        }
    }

    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
        {
            _tags.Add(tag.Trim());
            MarkAsUpdated();
            AddDomainEvent(new CustomerTagAddedEvent(Id));
        }
    }

    public void SetIndustry(string? industry)
    {
        Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim();
        MarkAsUpdated();
    }

    public void SetSource(string? source)
    {
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
        MarkAsUpdated();
    }

    public override void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            base.MarkAsDeleted();
            AddDomainEvent(new CustomerDeletedEvent(Id));
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
            AddDomainEvent(new CustomerRestoredEvent(Id));
        }
    }

    public override void MarkAsDeactivate()
    {
        base.MarkAsDeactivate();
        AddDomainEvent(new CustomerDeactivatedEvent(Id));
    }

    public override void MarkAsActivate()
    {
        base.MarkAsActivate();
        AddDomainEvent(new CustomerActivatedEvent(Id));
    }

    #endregion

    #region Additional Methods

    public string GetFullName() => $"{FirstName} {LastName}";

    public bool IsAssigned() => AssignedToUserId.HasValue;

    public bool HasNotes() => _notes.Count > 0;

    public bool HasTags() => _tags.Count > 0;

    #endregion
}
