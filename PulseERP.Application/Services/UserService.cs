using AutoMapper;
using MediatR;
using PulseERP.Abstractions.Common.DTOs.Users.Commands;
using PulseERP.Abstractions.Common.DTOs.Users.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Extensions;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper,
    IMediator mediator,
    IUnitOfWork unitOfWork
)
    : IUserService
{
    public async Task<PagedResult<UserSummary>> GetAllAsync(UserFilter userFilter)
    {
        var result = await userRepository.GetAllAsync(userFilter);

        var summaries = mapper.Map<List<UserSummary>>(result.Items);

        return new PagedResult<UserSummary>()
        {
            Items = summaries,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = summaries.Count,
        };
    }

    public async Task<UserDetails> GetByIdAsync(Guid id)
    {
        var user =
            await userRepository.FindByIdAsync(id, true)
            ?? throw new NotFoundException("User", id);

        var nowUtc = dateTimeProvider.UtcNow;
        user.CheckPasswordExpiration(nowUtc);

        return mapper.Map<UserDetails>(user);
    }

    public async Task<UserInfo> CreateAsync(CreateUserCommand cmd)
    {
        var errors = new Dictionary<string, string[]>();

        var emailVo = ValueObjectExtensions.TryCreateValueObject(
            () => new EmailAddress(cmd.Email),
            nameof(cmd.Email),
            errors
        );
        var phoneNumberVo = ValueObjectExtensions.TryCreateValueObject(
            () => new Phone(cmd.PhoneNumber),
            nameof(cmd.PhoneNumber),
            errors
        );

        var existingUser = await userRepository.FindByEmailAsync(emailVo);
        if (existingUser != null)
        {
            errors.Add(nameof(cmd.Email), ["Email is already in use."]);
            throw new ValidationException(errors);
        }

        var user = new User(cmd.FirstName, cmd.LastName, emailVo, phoneNumberVo);

        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAndDispatchEventsAsync();

        return mapper.Map<UserInfo>(user);
    }

    public async Task<UserDetails> UpdateAsync(Guid id, UpdateUserCommand cmd)
    {
        var user =
            await userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        if (!string.IsNullOrWhiteSpace(cmd.FirstName) || !string.IsNullOrWhiteSpace(cmd.LastName))
        {
            user.UpdateName(cmd.FirstName, cmd.LastName);
        }

        if (!string.IsNullOrWhiteSpace(cmd.Email))
        {
            var newEmailVo = new EmailAddress(cmd.Email);
            user.UpdateEmail(newEmailVo);
        }

        if (!string.IsNullOrWhiteSpace(cmd.Phone))
        {
            var newPhoneVo = new Phone(cmd.Phone);
            user.UpdatePhone(newPhoneVo);
        }

        if (!string.IsNullOrWhiteSpace(cmd.Role))
        {
            var newRoleVo = new Role(cmd.Role);
            user.ChangeRole(newRoleVo);
        }

        await userRepository.UpdateAsync(user);
        user.ClearDomainEvents();
        await unitOfWork.SaveChangesAndDispatchEventsAsync();

        return mapper.Map<UserDetails>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user =
            await userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        await userRepository.DeleteAsync(user);
        await userRepository.SaveChangesAsync();

        await unitOfWork.SaveChangesAndDispatchEventsAsync();
    }

    public async Task ActivateUserAsync(Guid id)
    {
        var user =
            await userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        user.ClearDomainEvents();

        if (user.IsDeleted)
        {
            throw new InvalidOperationException(
                $"Impossible d'activer l'utilisateur ({id}) : il est marqué comme supprimé."
            );
        }

        if (user.IsActive)
        {
            throw new InvalidOperationException($"L'utilisateur ({id}) est déjà actif.");
        }

        user.MarkAsActivate();

        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAndDispatchEventsAsync();
    }

    public async Task DeactivateUserAsync(Guid id)
    {
        var user =
            await userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        user.ClearDomainEvents();

        if (user.IsDeleted)
        {
            throw new InvalidOperationException(
                $"Impossible de désactiver l'utilisateur ({id}) : il est marqué comme supprimé."
            );
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException($"L'utilisateur ({id}) est déjà désactivé.");
        }

        user.MarkAsDeactivate();

        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAndDispatchEventsAsync();
    }

    public async Task ResetLockoutAsync(Guid id)
    {
        var user =
            await userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        user.ResetLockout();

        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        await unitOfWork.SaveChangesAndDispatchEventsAsync();
    }

    public async Task RestoreUserAsync(Guid id)
    {
        var user =
            await userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        user.MarkAsRestored();

        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        await unitOfWork.SaveChangesAndDispatchEventsAsync();
    }
}