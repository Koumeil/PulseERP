using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Events.CustomerEvents;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain.Entities;

public class CustomerTests
{
    private readonly EmailAddress _email = new("john.doe@example.com");
    private readonly Phone _phone = new("+1234567890");
    private readonly Address _address = new("Main St", "City", "1000", "BE");

    [Fact]
    public void Constructor_ValidData_ShouldCreateCustomer()
    {
        var customer = CreateSampleCustomer();

        Assert.Equal("John", customer.FirstName);
        Assert.Equal("Doe", customer.LastName);
        Assert.Equal("Company", customer.CompanyName);
        Assert.Equal(_email, customer.Email);
        Assert.Equal(_phone, customer.Phone);
        Assert.Equal(_address, customer.Address);
        Assert.True(customer.IsVIP);
        Assert.Contains(customer.DomainEvents, e => e is CustomerCreatedEvent);
    }

    [Fact]
    public void UpdateDetails_ValidChanges_ShouldUpdateFields()
    {
        var customer = CreateSampleCustomer();
        customer.UpdateDetails(firstName: "Jane", industry: "Tech");

        Assert.Equal("Jane", customer.FirstName);
        Assert.Equal("Tech", customer.Industry);
        Assert.Contains(customer.DomainEvents, e => e is CustomerDetailsUpdatedEvent);
    }

    [Fact]
    public void UpdateStatus_ShouldChangeStatus()
    {
        var customer = CreateSampleCustomer();
        customer.UpdateStatus(CustomerStatus.Inactive);

        Assert.Equal(CustomerStatus.Inactive, customer.Status);
        Assert.Contains(customer.DomainEvents, e => e is CustomerStatusChangedEvent);
    }

    [Fact]
    public void RecordInteraction_ShouldUpdateDate()
    {
        var customer = CreateSampleCustomer();
        customer.RecordInteraction();

        Assert.NotNull(customer.LastInteractionDate);
        Assert.Contains(customer.DomainEvents, e => e is CustomerInteractionRecordedEvent);
    }

    [Fact]
    public void AssignTo_ValidUserId_ShouldAssign()
    {
        var customer = CreateSampleCustomer();
        var userId = Guid.NewGuid();
        customer.AssignTo(userId);

        Assert.Equal(userId, customer.AssignedToUserId);
        Assert.Contains(customer.DomainEvents, e => e is CustomerAssignedEvent);
    }

    [Fact]
    public void AddNote_ShouldAddToCollection()
    {
        var customer = CreateSampleCustomer();
        customer.AddNote("Follow-up needed");

        Assert.Single(customer.Notes);
        Assert.Contains(customer.DomainEvents, e => e is CustomerNoteAddedEvent);
    }

    [Fact]
    public void AddTag_ShouldAddToCollection()
    {
        var customer = CreateSampleCustomer();
        customer.AddTag("Important");

        Assert.Single(customer.Tags);
        Assert.Contains(customer.DomainEvents, e => e is CustomerTagAddedEvent);
    }

    [Fact]
    public void ActivateCustomer_ShouldChangeState()
    {
        var customer = CreateSampleCustomer();
        customer.MarkAsDeactivate();
        customer.MarkAsActivate();

        Assert.True(customer.IsActive);
        Assert.Contains(customer.DomainEvents, e => e is CustomerActivatedEvent);
    }

    [Fact]
    public void DeactivateCustomer_ShouldChangeState()
    {
        var customer = CreateSampleCustomer();
        customer.MarkAsDeactivate();

        Assert.False(customer.IsActive);
        Assert.Contains(customer.DomainEvents, e => e is CustomerDeactivatedEvent);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        var customer = CreateSampleCustomer();
        customer.MarkAsDeleted();

        Assert.True(customer.IsDeleted);
    }

    [Fact]
    public void Restore_ShouldUnmarkAsDeleted()
    {
        var customer = CreateSampleCustomer();
        customer.MarkAsDeleted();
        customer.MarkAsRestored();

        Assert.False(customer.IsDeleted);
    }

    private Customer CreateSampleCustomer()
    {
        return new Customer(
            "John",
            "Doe",
            "Company",
            _email,
            _phone,
            _address,
            CustomerType.Prospect,
            CustomerStatus.Active,
            DateTime.UtcNow,
            true
        );
    }
}
