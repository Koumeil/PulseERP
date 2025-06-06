using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Events.CustomerEvents;
using PulseERP.Domain.VO;

namespace PulseERP.Tests.Domain.Entities;

public class CustomerEnumTests
{
    private Customer CreateSampleCustomer(
        CustomerType type = CustomerType.Lead,
        CustomerStatus status = CustomerStatus.Active
    )
    {
        return new Customer(
            firstName: "Alice",
            lastName: "Durand",
            companyName: "PulseCorp",
            email: new EmailAddress("alice@pulse.com"),
            phone: new Phone("+32470123456"),
            address: new Address("Rue Principale", "Bruxelles", "1000", "BE"),
            type: type,
            status: status,
            firstContact: DateTime.UtcNow,
            isVIP: false
        );
    }

    [Theory]
    [InlineData(CustomerType.Lead)]
    [InlineData(CustomerType.Prospect)]
    [InlineData(CustomerType.Client)]
    [InlineData(CustomerType.Former)]
    public void Constructor_ShouldSet_CustomerType(CustomerType type)
    {
        var customer = CreateSampleCustomer(type: type);
        Assert.Equal(type, customer.Type);
    }

    [Theory]
    [InlineData(CustomerStatus.Active)]
    [InlineData(CustomerStatus.Inactive)]
    [InlineData(CustomerStatus.Pending)]
    [InlineData(CustomerStatus.InDispute)]
    public void Constructor_ShouldSet_CustomerStatus(CustomerStatus status)
    {
        var customer = CreateSampleCustomer(status: status);
        Assert.Equal(status, customer.Status);
    }

    [Fact]
    public void UpdateStatus_ShouldChange_AndAddEvent()
    {
        var customer = CreateSampleCustomer(status: CustomerStatus.Active);
        customer.UpdateStatus(CustomerStatus.InDispute);

        Assert.Equal(CustomerStatus.InDispute, customer.Status);
        Assert.Contains(customer.DomainEvents, e => e is CustomerStatusChangedEvent);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdate_Enums()
    {
        var customer = CreateSampleCustomer(
            type: CustomerType.Lead,
            status: CustomerStatus.Pending
        );

        customer.UpdateDetails(type: CustomerType.Client, status: CustomerStatus.Active);

        Assert.Equal(CustomerType.Client, customer.Type);
        Assert.Equal(CustomerStatus.Active, customer.Status);
        Assert.Contains(customer.DomainEvents, e => e is CustomerDetailsUpdatedEvent);
    }
}
