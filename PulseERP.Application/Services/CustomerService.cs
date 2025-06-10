using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.DTOs.Customers.Commands;
using PulseERP.Abstractions.Common.DTOs.Customers.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Services;

public sealed class CustomerService(ICustomerRepository repo, IMapper mapper, ILogger<CustomerService> log)
    : ICustomerService
{
    public async Task<PagedResult<CustomerSummary>> GetAllCustomersAsync(
        CustomerFilter customerFilter
    )
    {
        var result = await repo.GetAllAsync(customerFilter);
        var summaries = mapper.Map<List<CustomerSummary>>(result.Items);

        return new PagedResult<CustomerSummary>
        {
            Items = summaries,
            TotalItems = result.TotalItems,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
        };
    }

    public async Task<CustomerDetails> FindCustomerByIdAsync(Guid id)
    {
        var customer = await repo.FindByIdAsync(id);
        return mapper.Map<CustomerDetails>(customer);
    }

    public async Task<CustomerDetails> FindCustomerByEmailAsync(EmailAddress email)
    {
        var customer = await repo.FindByEmailAsync(email);
        return mapper.Map<CustomerDetails>(customer);
    }

    public async Task<CustomerDetails> CreateCustomerAsync(CreateCustomerCommand cmd)
    {
        var customer = new Customer(
            cmd.FirstName,
            cmd.LastName,
            cmd.CompanyName,
            new EmailAddress(cmd.Email),
            new Phone(cmd.Phone),
            new Address(cmd.Street, cmd.City, cmd.ZipCode, cmd.Country),
            Enum.Parse<CustomerType>(cmd.Type),
            Enum.Parse<CustomerStatus>(cmd.Status),
            DateTime.UtcNow,
            cmd.IsVip
        );

        customer.SetIndustry(cmd.Industry);
        customer.SetSource(cmd.Source);

        await repo.AddAsync(customer);
        await repo.SaveChangesAsync();

        return mapper.Map<CustomerDetails>(customer);
    }

    public async Task<CustomerDetails> UpdateCustomerAsync(Guid id, UpdateCustomerCommand cmd)
    {
        var customer = await repo.FindByIdAsync(id) ?? throw new NotFoundException("Customer", id);

        customer.UpdateDetails(
            firstName: cmd.FirstName,
            lastName: cmd.LastName,
            companyName: cmd.CompanyName,
            email: cmd.Email is null ? null : new EmailAddress(cmd.Email),
            phone: cmd.Phone is null ? null : new Phone(cmd.Phone),
            address: (cmd.Street, cmd.City, cmd.ZipCode, cmd.Country) switch
            {
                ({ } s, { } c, { } z, { } co) => new Address(s, c, z, co),
                _ => null,
            },
            type: cmd.Type is null ? null : Enum.Parse<CustomerType>(cmd.Type),
            status: cmd.Status is null ? null : Enum.Parse<CustomerStatus>(cmd.Status),
            isVip: cmd.IsVip,
            industry: cmd.Industry,
            source: cmd.Source
        );

        await repo.UpdateAsync(customer);
        await repo.SaveChangesAsync();

        return mapper.Map<CustomerDetails>(customer);
    }

    public async Task AssignCustomerToUserAsync(Guid customerId, Guid userId)
    {
        var customer = await repo.FindByIdAsync(customerId);
        if (customer is null)
        {
            log.LogWarning("Customer {CustomerId} not found for assignment.", customerId);
            throw new NotFoundException("Customer", customerId);
        }
        customer.AssignTo(userId);

        await repo.UpdateAsync(customer);
        await repo.SaveChangesAsync();

        log.LogInformation("Customer {CustomerId} assigned to user {UserId}.", customerId, userId);
    }

    public async Task RecordCustomerInteractionAsync(Guid customerId, string note)
    {
        var customer = await repo.FindByIdAsync(customerId);
        if (customer is null)
        {
            log.LogWarning(
                "Customer {CustomerId} not found for recording interaction.",
                customerId
            );
            throw new NotFoundException("Customer", customerId);
        }

        customer.RecordInteraction();
        customer.AddNote(note);

        await repo.UpdateAsync(customer);
        await repo.SaveChangesAsync();

        log.LogInformation("Interaction recorded for customer {CustomerId}.", customerId);
    }

    public async Task RestoreCustomerAsync(Guid id)
    {
        var customer = await repo.FindByIdAsync(id) ?? throw new NotFoundException("Customer", id);

        if (customer.IsDeleted)
            throw new InvalidOperationException($"Le customer ({id}) a déjà été restauré.");

        customer.MarkAsRestored();

        await repo.UpdateAsync(customer);
        await repo.SaveChangesAsync();
    }

    public async Task ActivateCustomerAsync(Guid id)
    {
        var customer = await repo.FindByIdAsync(id) ?? throw new NotFoundException("Customer", id);

        if (customer.IsDeleted)
            throw new InvalidOperationException(
                $"Impossible d'activer le customer ({id}) : il est marqué comme sup​primé."
            );

        if (customer.IsActive)
            throw new InvalidOperationException($"La customer ({id}) est déjà actif.");

        customer.MarkAsActivate();

        await repo.UpdateAsync(customer);
        await repo.SaveChangesAsync();
    }

    public async Task DeactivateCustomerAsync(Guid id)
    {
        var customer = await repo.FindByIdAsync(id) ?? throw new NotFoundException("Customer", id);

        if (customer.IsDeleted)
            throw new InvalidOperationException(
                $"Impossible de désactiver le customer ({id}) : il est marqué comme sup​primée."
            );

        if (!customer.IsActive)
            throw new InvalidOperationException($"La marque ({id}) est déjà désactivée.");

        customer.MarkAsDeactivate();

        await repo.UpdateAsync(customer);
        await repo.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var customer = await repo.FindByIdAsync(id) ?? throw new NotFoundException("Brand", id);
        await repo.DeleteAsync(customer);
        await repo.SaveChangesAsync();
    }
}
